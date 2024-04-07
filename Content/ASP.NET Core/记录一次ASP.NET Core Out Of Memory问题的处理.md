# 记录一次ASP.NET Core Out Of Memory问题的处理

## 概要
记录一次生产环境的异常事件，运维监控到CPU突然占用100%，其中一个docker 容器崩掉，经过一段时间的观察和搜索，最终通过 AppDomain.FirstChanceException去定位问题，因为错误使用了 LoggerFactory.Create而产生的Out of memory错误。

## 背景

### 运行环境

- 系统：Ubuntu 22.04
- 应用运行方式：Docker容器
- 运行环境：ASP.NET Core 7
- 负载均衡：两台机器

### 运维报警

运维在监控中发现CPU占用突然升高，然后内存占用会下降一部分，持续3-5分钟。两台机器行为一致，只是出现的时间相差5-60分钟不等。

看到这么奇怪的现象，我们先是检查了一下应用状态，发现有一个docker容器挂掉了，挂掉的时间符合问题出现的时间，内存占用下降就是这个应用结束后释放的内存，这个服务就是我们的接口服务。

## 问题追踪

我们一开始先排除程序之外的问题，是不是程序之外的异常，导致CPU占用过高，而CPU占用过高导致了程序崩溃，但很快我们排除了这个猜想：

- 运维和云服务商进行了排查，没有发现异常情况。
- 我们查询了一下日志信息，发现只输出了`Out of Memory`，这说明程序在内存分配时遇到了问题，但仍然不确定是因为CPU占用突然升高导致了程序出错，还是因为程序问题导致了CPU占用突然升高。
- 问题会在容器运行一段时间后(约40+小时)，重新出现，且只有该服务会挂掉，且每次都会输出`Out of Memory`，现在我们基本确定是程序本身的问题。

### 初步措施

- 首先排除了主机内存不够用的情况，我们监控了程序的运行，程序在出现问题前，内存会缓慢的增长，然后降低(GC触发)，最多时，总内存占用也不过500M，并没有什么异常。

- 面对只有`Out of Memory`这样的信息，我们调整了生产环境的日志级别，下次出错时，仍然只有`Out of Memory`。也就是说这个错误，我们没有捕获到。

- 关于`Out of Memory`可能出现的问题，我们参考了微软官方文档的文章，但经过思考和代码浏览，仍然没有想到问题出在哪里。

- 我们想在开发环境中进行复现，很难做到，因为这个问题，只有在程序运行一段时间后才会出现，大概40小时以上，而我们的开发环境是每天都会更新的。这样也很难复现，在周末时间复现后，仍然没有有效的日志信息。

**面对目前获取到的信息，很难采取进一步的措施，我们必须获取更多有效的信息。**
但我们隐约感觉到应该是跟我们的一个后台定时任务有关系，因为该问题总是在程序运行一段时间后出现。

## 获取错误信息

于是我们查询如何捕获`asp.net core`应用程序 `out of memory`相关的信息，微软官方提供了一些工具，可以帮助我们调试或监控内存分配的情况，但很多方式使用起来比较困难。

- VisualStudio调试功能，这个现象很难复现，而且通常是在正常运行了一段时间后，才会出现，所以我们很难在本机使用VS调试功能复现。
  
- 微软提供了一系列的dotnet cli 工具，如`dotnet trace`，`dotnet stack`等来对应用进行分析和监控，但我们想直接在docker环境中使用，并获取相关信息并转储分析，并不容易。

奇怪的地方在于，为什么应用只输出了`Out of Memory`，这意味着应用程序**知道**问题，只是输出的内容过于简单，有什么方式可以输出更完整的信息么？

于是我搜索了相关的关键词，终于在`Github`上找到了一些`Issue`,如:

- [OutOfMemoryException caused by user-code in ASP.NET Core runtime](https://github.com/dotnet/runtime/issues/64831)
- [How to handle globally OutOfMemory exception in large project](https://github.com/dotnet/runtime/issues/48157)

其中都指向了[`AppDomain.FirstChanceException`](https://learn.microsoft.com/zh-cn/dotnet/framework/app-domains/how-to-receive-first-chance-exception-notifications)，使用它可以帮助我们捕获更多的异常信息。

简单来说，我们可以理解为，使用这个事件，我们可以在捕获并处理最初的异常信息。而在ASP.NET Core中我们通过try-catch捕获的异常都是经过处理(过滤、格式化等)的。

## 代码实现

在`Program.cs`中添加以下代码:

```csharp
AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
{
    if (eventArgs.Exception is OutOfMemoryException)
    {
        Console.WriteLine($"=== OutOfMemory: {eventArgs.Exception.Message}, {eventArgs.Exception.StackTrace}");
    }
    else
    {
        Console.WriteLine($"Caught exception: {eventArgs.Exception.Message}");
    }
};
```

> [!TIP]
> AppDomain.CurrentDomain.FirstChanceException 是一个事件，它在应用程序域中的每个应用程序域中的每个线程上引发。它允许您在第一次尝试处理异常之前捕获异常。这是一个很好的机会，可以在异常被处理之前记录或处理异常

## 定位问题

当我加入这段代码后，在下一次问题出现时，终于看到了详细的错误信息。问题出现在一个实现类的构造函数中，使用LoggerFactory.Create时使用builder.AddConsole()时报错。

而这段初始化代码正是在某个实现类中，而这个实现类在后台定时任务时，通过创建的scope获取注入服务时调用了构造函数。也就是说，每次定时任务(每两分钟一次)都会最终调用创建ILogger的代码。

```csharp

_logger = LoggerFactory.Create(builder => builder.AddConsole())
    .CreateLogger<XXX>();
```

至于为什么这段代码会出现`Out of Memory`，现在还并不确定。但我们基本可以确认，每次在构造函数中去创建新的logger，似乎是不正确的使用方式。

还有个迷团，为什么程序出现了Out of Memory，CPU的占用会突然升到将近100%？这个可能跟runtime 的处理方式有关了。

## 解决方案

问题代码已经定位，于是我删除了这段代码，不再在构造函数中手动创建ILogger。
其实这样的代码本不应存在，只是为了兼容子类构造函数中没有传递ILogger，在基类的构造方法中手动创建，于是就出现了这样的问题。

## 总结

### 要正视问题

最终问题得到解决，而且似乎也不是什么大问题。而且这段代码本身在运行时是正常的，只是在经过长时期的定时任务调用后，突然出现了问题。

而问题出现，服务停止时，docker 服务会自动重启，我们有两个节点，还没遇到同时出现问题的情况，实际上该问题并没有影响到实际业务运行，只是通过最初的`CPU占用率突然100%`这个现象最终发现和解决的。

我们可以"忽略"这个问题，通过提高服务容错率的方式"无视"程序本身可能存在的问题，让运维去处理。但从发现程序日志中有异常报错信息开始，这个性质就变了，这应该开发去修复。

### 最重要的是定位问题

生产环境出现了问题，如果没办法在本地复现调试，处理起来会非常麻烦，我们要复现、跟踪、修复、测试问题，整体上可能要花费很长时间，比如这个事件中，平均每次问题复现的时间在两天左右，而我们从发现问题到验证解决，时间跨度长达三周，而修改代码的时间，大概只需要10分钟。

也就是说，我们定位问题花费的时间远远超过解决问题的时间。而定位问题的关键，是能够获取到异常记录，要想获取到异常记录，就要求我们对程序的各个环节都有记录。也就是说程序对于程序员来说，必须是透明公开的，不能存在黑箱。很幸运，我们找到了获取异常信息的事件。

### 解决问题才能进步

当然，这次也有收获，通过`AppDomain.CurrentDomain.FirstChanceException`，可以获取到过滤前的异常信息，通过`FirstChanceException`，我确实看到了更多详细的错误信息，
我现在已经将该段代码默认作为程序的一部分了。

作为程序员，不怕出现问题，因为我们本身就是问题解决者，这是我们的价值所在，我们最怕的是，出现问题但没有报错信息，你无法定位问题，就无法解决问题。而这些问题不解决，只会越来越多，越来越危险，它不会自己消失！
