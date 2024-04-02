# 关于EF Core更新速度随时间越来越慢的解决办法

## 概要
本篇主要介绍使用 `context.ChangeTracker.Clear() `方法，在通过循环进行批量更新时，通过手动清除跟踪实体以提高性能的示例。

## 背景

最近在做一些数据分析时，遇到了一个问题，当我把计算结果更新到数据库时，一开始速度会很快，但随着时间的推移，更新速度会越来越慢。
本篇博客就来说明这种现象的原因和解决办法。

> 环境:`ASP.NET Core 7` 和 `EF Core 7`.

## 事例说明

我有1000W已处理好的数据需要更新到数据库，这些数据我也是从数据库中一次性查询出来的，这样可以只进行一次查询，并使用`AsNoTracking()`提高查询效率，然后我对这些数据进行了并行计算，最后将计算完的结果更新到数据库。最费时的操作就是更新到数据库。

请看以下代码示例:

```csharp
var bc = new ConcurrentBag<List<StockDailyKLineInfo>>();
// 并行计算
var computeTasks = group.AsParallel()
    .WithDegreeOfParallelism(Environment.ProcessorCount)
    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
    .Select(async g =>
    {
        var computedData = await service.ComputeAsync(g.ToList());
        if (computedData != null)
        {
            bc.Add(computedData);
        }
    });

await Task.WhenAll(computeTasks);

// 数据插入
var batchSize = 5000;
var items = bc.SelectMany(x => x).ToList();
left = items.Count;
_logger.LogInformation($"need update {left} daily!");

foreach (var batch in items.Chunk(batchSize))
{
    context.AttachRange(batch);
    foreach (var entity in batch)
    {
        var entry = context.Entry(entity);
        entry.Property(e => e.A).IsModified = true;
        entry.Property(e => e.B).IsModified = true;
        entry.Property(e => e.C).IsModified = true;
        entry.State = EntityState.Modified;
    }

    var count = await context.SaveChangesAsync();
}
await Console.Out.WriteLineAsync("[done] update all data");
```

并行计算速度非常快，几秒就能都完成了。

数据插入，我分批进行循环插入，每次5000条，通常不到1秒时间就能插入成功。但随着时间的推移，插入速度越来越慢。

> [!NOTE]
> 由于我有1000W的数据插入，如果最终一次性提交，如果出现了异常，那么所有数据都不会插入成功，并且会等待很长的时间，并且在最终执行完成之前，你得不到任何信息，以预估可能花费的时间。所以我需要分批插入。

## 原因

 EF Core 会在上下文中跟踪所有已加载或附加的实体。随着循环的进行，上下文将追踪越来越多的实体，这可能会导致性能下降。

 **也就是说在同一个`DbContext`上下文中，SaveChangesAsync()方法调用后，不会清除已更新的内容**，这意味着追踪的实体越来越多，最终多达1000W，并且这些都是已经标记为要更新的内容，也意味着你每次都会更新更多的内容到数据库。

## 解决办法

### 只进行一次SaveChanges

既然每次`saveChanges`不会清除，那么最后我只提交一次不就行了么？但这个方案不符合实际需求，上面已经提到过了。

### 使用多个DbContext

既然 同一个`DbContext`下会出现这个问题，那么每次更新，我再创建一个新的DbContext不就可以了么？
这个方法虽然可行，但对于1000W的数据来说，即使我每次更新1W条数据，也需要创建1000+次`DbContext`，也有一定的消耗。

### 清除追踪

既然问题是SaveChanges不会自动清除已追踪的更改，如果我可以手动去清除，不就可以了么？清除的操作比起创建新的`DbContext`实例，还是更快捷的。

那么我们修改代码:

```csharp
foreach (var batch in items.Chunk(batchSize))
{
    context.AttachRange(batch);
    foreach (var entity in batch)
    {
        var entry = context.Entry(entity);
        entry.Property(e => e.A).IsModified = true;
        entry.Property(e => e.B).IsModified = true;
        entry.Property(e => e.C).IsModified = true;
        entry.State = EntityState.Modified;
    }
    var count = await context.SaveChangesAsync();

    // ⚒️ add this line
    context.ChangeTracker.Clear();
}
```

> [!TIP]
> `context.ChangeTracker.Clear()` 方法清除上下文中的所有已跟踪实体。这将重置更改跟踪器并清除其跟踪的所有实体，从而释放内存并提高性能。

## 总结

`EF Core 7` 中已经添加了批量更新的方法，但这种方法也不适用于我遇到的场景，因为我不是按条件进行批量更新，而是每一条数据都需要更新。
`context.ChangeTracker.Clear()`可以在这样的场景下发挥作用，在一些关联插入或更新的场景，为避免追踪带来的冲突问题，也可以通过该方法清除追踪，然后再手动建立关系，进行提交。
