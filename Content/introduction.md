# Blog

本博客系统通过构建工具生成`纯静态`的博客网站，借助`GitHub Pages`，你可以在5分钟内免费拥有个人博客。 它具有以下特点

- 生成纯静态网站，访问速度极快
- 使用markdown格式来编写博客内容
- 基于git代码管理来存储你的博客
- 使用CI工具来自动化部署你的博客站点

## 🎖️功能

- 主页博客列表，支持搜索和分类和存档筛选
- 自定义网站名称和说明
- 随系统变化的Light和Dark主题
- 移动端的自适应显示
- TOC支持
- 代码高亮及复制支持

## 使用Github Page部署

### Fork并配置GitHub Page

1. 点击`Fork`按钮，并创建自己的仓库。
2. 进入自己的仓库，点击`Actions`，启用workflows。
3. 点击`Settings`，找到Pages配置，在Build and deployment 选项中选择`GitHub Actions`.

### 配置

你可以通过根目录下的`webinfo.json`，对博客基础信息进行配置，如下所示：

```json
{
  "Name": "Niltor Blog", // 博客名称，显示在主页顶部导航
  "Description": "🗽 for freedom",// 说明，显示在主页顶部中间
  "AuthorName": "Ater", // 作者名称，显示在博客列表
  "BaseHref": "/blazor-blog/" // 子目录
}
```

当你使用Github Page或使用IIS子应用部署时，需要调整`BaseHref`。通常是你的**项目名称**或**子目录名**。

> [!IMPORTANT]
> 注意，`BaseHref`尾部的`/`是必需的。
>
> 如果你配置了自定义域名，并且没有使用子目录，请将BaseHref设置为`/`。

修改后提交代码，GitHub会触发Action自动构建。

### 编写博客

请使用任何你习惯的markdown编辑器编写博客,唯一的要求是将博客内容放到`Content`目录下。你可以在该目录下创建多级目录。

### 发布博客

你只需要正常提交代码即可，github action会自动构建并最终发布你的博客，发布成功后可打开您的 GitHub Page 查看。

## 部署到其他服务

如果你不使用Github Page，那么你也可以轻松的部署到其他服务。核心的步骤只需要两步。

### 生成静态内容

`BuildSite`项目是用来将markdown转换成html的，请在根目录执行:

```pwsh
 dotnet run --project .\Lib\BuildSite\ .\Content .\WebApp Production
```

在根目录下，你会看到`WebApp`目录。

### 上传到你的服务器

将`WebApp`中的所有文件复制到你的服务器即可。

> [!TIP]
> 根目录下的`publishToLocal.ps1`脚本可以自动完成构建和生成的操作，最终内容将在根目录下`WebApp`目录中。
>
> 如果你使用自动化部署，可参考.github/workflows中的脚本。

## 二次开发

fork之后，你将拥有所有的自定义权限，因为所有的源代码都已经在你自己的仓库中。

核心项目为`BuildSite`，该项目是用来生成数据文件的，其中包括将markdown文件转换成html文件。

你需要准备以下内容以便进行二次开发

- .NET SDK 8.0，以便运行 `BuildSite`项目
- tailwindcss，生成css样式内容
- http-server，用来启动本地静态内容，以便调试

### 运行项目

1. 预览项目
   1. 打开终端，在`WebApp`目录下执行`http-server`，然后在浏览器中打开`http://127.0.0.1:8080`。
2. 生成静态内容。
   1. 在根目录下执行`dotnet run --project ./Lib/BuildSite ./Content ./WebApp`，以生成最新的静态内容。
   2. 或者在

如果你使用`tailwindcss`，请在`WebApp`下执行`npx tailwindcss -o ./css/app.css --watch`

### 自定义主页内容

主页内容模板位于`Lib\BuildSite\template\index.html.tpl`，其中包括以下变量：

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

博客页内容模板位于`Lib\BuildSite\template\blog.html.tpl`，其中包括以下变量：

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