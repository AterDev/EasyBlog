# Blazor-blog

如果你想拥有一个免费的博客，那么这是你的一个选择。

借助`github pages`和`dotnet blazor`，你可以拥有一个自定义的个人博客。

## 如何使用

### Fork本仓库

点击Fork按钮，并创建自己的仓库。

### 修改 base href

当你使用Github Page或使用ISS子应用部署时，需要调整base href。

该文件位于`Blog/wwwroot/index.html`。

请将该值调整成你的子目录名称，如`/blazor-blog/`，请注意，尾部的`/`是必需的。

```html
<base href="/blazor-blog/" />
```

如果你使用自定义域名，且没有子目录，则将该值保持为`/`。

```html
<base href="/" />
```

> [!NOTE]
> 可以查看[官方文档](https://learn.microsoft.com/zh-cn/aspnet/core/blazor/host-and-deploy/?view=aspnetcore-3.1&tabs=visual-studio#configure-the-app-base-path)来了解更多内容。

## 自定义

fork之后，你将拥有所有的自定义权限，因为所有的源代码都已经在你自己的仓库中。

仓库主要包含两个核心项目，一个是`BuildSite`，该项目是用来生成静态站点的。

`Blog`项目是一个Blazor WASM项目，默认包含了博客的主页定义。你可以自由的添加和修改其他的功能。
