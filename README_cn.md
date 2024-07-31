# EasyBlog

📘[Englisth](./README.md)   📘[中文](./README-cn.md)

本工具通过命令将`markdown`文档生成`纯静态`的博客网站，借助`GitHub Pages`，你可以在5分钟内免费拥有个人博客。 它具有以下特点

- 提供命令行工具生成静态网站
- 生成纯静态网站，访问速度极快
- 支持markdown格式编写的内容
- 支持搜索、分类、存档筛选
- 自定义网站名称和说明等

效果展示：[NilTor's Blog](https://blog.dusi.dev/)

## 🎖️功能

- 主页博客列表，支持搜索和分类和存档筛选
- 自定义网站名称和说明
- 支持文档中本地图片路径
- 随系统变化的Light和Dark主题
- 移动端的自适应显示
- TOC支持
- mermaid,nomnoml,Math的渲染支持
- 代码高亮及复制支持

## 🚀快速开始

目前工具已经以`dotnet tool`的形式发布。你可以很方便的安装并使用。

### 安装工具

首先确认你们已经安装了`dotnet sdk`8.0或以上版本，然后在命令行中安装:

```dotnetcli
dotnet tool install -g Ater.EasyBlog --preview
```

安装完成后，你可以使用`ezblog`命令来操作。

### 使用工具

我们假设，你已经有一些markdown文档，它在`markdown`目录下。

现在我们使用命令:

```pwsh
ezblog init
```

初始化一个`webinfo.json`文件，用来配置你的博客基本信息，该文件在后续生成时可重复使用。该文件内容如下:

```json
{
  "Name": "Niltor Blog", // 博客名称，显示在主页顶部导航
  "Description": "🗽 for freedom",// 说明，显示在主页顶部中间
  "AuthorName": "Ater", // 作者名称，显示在博客列表
  "BaseHref": "/blazor-blog/", // 子目录
  "Domain": "https://aterdev.github.io" // 域名，生成sitemap使用，不生成则留空
}
```

> [!IMPORTANT]
> 注意，`BaseHref`尾部的`/`是必需的。
>
> 如果你配置了自定义域名，并且没有使用子目录，请将BaseHref设置为`/`。

然后我们使用命令

```pwsh
ezblog build .\markdown .\WebApp
```

该命令将会把`markdown`目录下的所有markdown文件转换成html文件，并生成到`WebApp`目录下。

你可以使用`http-server`命令来启动一个本地服务器，查看生成的内容。

`WebApp`目录下就是静态网站需要的一切，你可以将它自由的部署到你需要的地方。

## 使用Github Page部署

### 配置GitHub Page

1. 在Github上创建自己的仓库。
2. 进入自己的GitHub仓库，点击`Actions`，启用workflows。
3. 点击`Settings`，找到Pages配置，在Build and deployment 选项中选择`GitHub Actions`.

当你使用Github Page或使用IIS子应用部署时，需要调整`webinfo.json`中的`BaseHref`。通常是你的**项目名称**或**子目录名**。

### 编写博客

我们假设你的md文档都在`markdown`目录下。

请使用任何你习惯的markdown编辑器编写博客，你可以在`markdown`目录下创建多级目录，以对md文档进行分类。

### 生成静态内容

使用build命令，如：

```pwsh
ezblog build .\markdown .\_site
```

> [!NOTE]
> `.\markdown`是你存放md文件的目录，可根据实际情况自由修改。`.\_site`是生成后的静态网站目录。

### 发布博客

使用`GitHub Action`来自动化部署你的博客站点。

在仓库的根目录`.github/workflows`目录(没有则手动创建)下创建`build.yml`文件，内容如下：

```yml
name: Deploy static content to Pages
on:
  push:
    branches: ["main"]
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: "pages"
  cancel-in-progress: false

jobs:
  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      - name: Setup Pages
        uses: actions/configure-pages@v4

      - name: Dotnet Setup
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.x

      - run: dotnet tool install  -g Ater.EasyBlog --version 1.0.0
      - run: ezblog build ./Content ./_site
      
      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: '_site/'
          
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

现在只需要推送代码即可，`GitHub Action`会自动构建并最终发布你的博客，发布成功后可打开您的 GitHub Page 查看。

## 部署到其他服务

如果你不使用Github Page，那么你也可以轻松的部署到其他服务。核心的步骤只需要两步。

### 生成静态网站

我们假设你的文档都位于`markdown`目录下。

先使用`ezblog init`命令生成`webinfo.json`配置文件，并根据实际需求修改。

然后执行`ezblog build ./markdown ./wwwroot`命令。

### 上传到你的服务器

将`wwwroot`中的所有文件复制到你的服务器即可。

## 二次开发

fork之后，你将拥有所有的自定义权限，因为所有的源代码都已经在你自己的仓库中。

核心项目为`BuildSite`，用来生成静态文件，其中包括将markdown文件转换成html文件。

> [!NOTE]
> 建议创建自己的分支来自定义开发内容，方便后续的合并。

### 开发环境

`BuildSite`项目是一个.NET项目，你需要安装.NET SDK 8.0。

此外，你可以安装(可选)

- http-server，用来启动本地静态内容，以便调试。
- tailwindcss，用来生成css样式内容。
- typescript，以便使用`tsc`命令。

### 运行项目

1. 预览项目
   1. 打开终端，在`WebApp`目录下执行`http-server`，然后在浏览器中打开`http://127.0.0.1:8080`。
2. 生成静态内容。
   1. 在根目录下执行`dotnet run --project ./src/BuildSite ./Content ./WebApp`，以生成最新的静态内容。
   2. 或者直接运行根目录下的`build.ps1`脚本。
3. 刷新浏览器，可看到最新生成的内容。

> [!TIP]
> 如果你使用`Tailwindcss`，可在`WebApp`下执行`npx tailwindcss -o ./css/app.css --watch`。
>
> 如果你使用`Typescript`，可在`WebApp`下执行`tsc -w`。

### 自定义主页内容

主页内容模板位于`src\BuildSite\template\index.html.tpl`，其中包括以下变量：

|模板变量  |说明  |
|---------|---------|
|@{BaseUrl}   |  基础路径       |
|@{Name} |       博客名称  |
|@{Description} |    描述     |
|@{blogList} |    博客列表     |
|@{siderbar} |    侧边栏内容:分类和存档    |

你可以按照自己的想法修改主页的布局和样式。

> [!NOTE]
> 请注意标签中的`id`属性，`js`脚本将依赖于这些id标识，如果你修改了这些标识，同时要修改`js`脚本。

主页内容包括博客的搜索和分类筛选功能，其功能代码在`WebApp\js\index.js`中。

关于博客列表和分类列表的自定义，你可以参考`BuildSite`项目中`HtmlBuilder.cs`文件中的`GenBlogListHtml`和`GenSiderBar`方法。

后续我们会提供更灵活的自定义方式。

### 自定义博客展示页

博客页内容模板位于`src\BuildSite\template\blog.html.tpl`，其中包括以下变量：

|模板变量  |说明  |
|---------|---------|
|@{BaseUrl}   |  基础路径       |
|@{Title} |      页面标题  |
|@{content} |    博客内容     |
|@{toc} |   二级标题TOC    |

关于博客展示页的内容，你可以通过`WebApp/css/markdown.css`来修改样式，以及`WebApp/js/markdown.js`来定义逻辑。

### 自定义代码高亮

本项目使用`ColorCode`来格式化markdown中的代码内容，`ColorCode`使用正则来匹配代码内容。如果你需要对代码高亮进行定义，你需要：

- 添加或修改正则规则，你将在`ColorCode.Core/Compilation/Languages`目录下找到相应的语言定义，如果不存在，你可以添加新的语言支持。
- 如果是新添加的语言，需要在`ColorCode.Core/Languages.cs`中加载该语言。

> [!IMPORTANT]
> 如果你修改了`BuildSite`项目上中的代码，需要重新生成静态网站，才能看到最新效果。
