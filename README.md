# Blazor-blog

借助`GitHub Page`和`.NET Blazor`，你可以在5分钟内免费拥有个人博客。

## 使用Github Page部署

### Fork并配置GitHub Page

1. 点击`Fork`按钮，并创建自己的仓库。
2. 进入自己的仓库，点击`Actions`，启用workflows。
3. 点击`Settings`，找到Pages配置，在Build and deployment 选项中选择`GitHub Actions`.

### 修改 base href

当你使用Github Page或使用ISS子应用部署时，需要调整base href。

你只需要修改根目录下的`webinfo.json`文件中的`BaseHref`值即可，通常是你的项目名称或子目录名。
如:

```json
{
  "Name": "Niltor Blog",
  "Description": "My Blog - Powered by Ater Blog",
  "AuthorName": "Ater",
  "BaseHref": "/blazor-blog/"
}
```

> [!IMPORTANT]
> 注意，尾部的`/`是必需的。
>
> 如果你配置了自定义域名，并且没有使用子目录，请将BaseHref设置为`/`。

修改后提交代码，GitHub会触发Action自动构建。

### 编写博客

请使用任何你习惯的markdown编辑器编写博客,唯一的要求是将博客内容放到`Content`目录下。你可以在该目录下创建多级目录。

### 发布博客

你只需要正常提交代码即可，github action会自动构建并最终发布你的博客，发布成功后可打开您的 GitHub Page 查看。

## 部署到其他服务

如果你不使用Github Page，那么你也可以轻松的部署到其他应用。核心的步骤只需要两步。

### 构建Blazor项目

我们使用`Blazor WASM`来开发前端静态网站，所以，你只需要使用`dotnet publish`命令将网站发布成静态文件即可，在根目录下执行:

```dotnetcli
dotnet publish ./src/ -c Release -o ./_site
```

### 生成静态内容

`BuildSite`项目是用来将markdown转换成html的，请在根目录执行:

```pwsh
 dotnet run --project .\Lib\BuildSite\ .\Content .\_site Production
```

在根目录下，你会看到`_site`目录。

### 上传到你的服务器

将`_site\wwwroot`中的所有文件复制到你的服务器即可。

> [!TIP]
> 根目录下的`publishToLocal.ps1`脚本可以自动完成构建和生成的操作，最终内容将在根目录下`_site`目录中。
>
> 如果你使用自动化部署，可参考.github/workflows中的脚本。

## 二次开发

fork之后，你将拥有所有的自定义权限，因为所有的源代码都已经在你自己的仓库中。

仓库主要包含两个核心项目，一个是`BuildSite`，该项目是用来生成数据文件的，其中包括将markdown文件转换成html文件。

`Blog`项目是一个Blazor WASM项目，默认包含了博客的主页定义。你可以自由的添加和修改其他的功能。

### 自定义博客样式

`wwwroot/css`目录下的`markdown.css`文件，是用来定义博客页中的样式，你可以通过修改该文件来自定义自己想要的样式。

## 🎊功能列表

## 基础功能说明

- [x] 博客主页
- [x] 博客的分类和搜索
- [x] 博客详情页基础内容展示

## 生成工具

- [x] 生成分类和博客信息
- [x] 生成md对应的html文件

## 待添加和完善功能

- [ ] 博客页的TOC内容生成
- [ ] C#代码片段的加强
- [x] 博客详情页的darkMode样式
