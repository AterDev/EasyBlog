# Blog

📘[Englisth](./README.md)   📘[中文](./README_cn.md)

This blog system generates a **pure static** blog website through build tools. With the help of **GitHub Pages**, you can have a personal blog for free in 5 minutes. It has the following features:

- Generate pure static websites for extremely fast access speed
- Use markdown format to write blog content
- Use git code management to store your blog
- Use CI tools to automatically deploy your blog site

**Demo:** NilTor's Blog: [https://blog.dusi.dev/](https://blog.dusi.dev/)

## 🎖️Features

- Blog list on the homepage, supporting search, category and archive filtering
- Customize website name and description
- Light and Dark themes that change with the system
- Adaptive display for mobile devices
- TOC support
- mermaid, nomnoml, Math rendering support
- Code highlighting and copy support

## Deploy with Github Page

### Fork and configure GitHub Page

1. Click the **Fork** button and create your own repository. Uncheck **Copy the main branch only**.
2. Go to your own GitHub repository, click **Actions**, and enable workflows.
3. Click **Settings**, find Pages configuration, and select **GitHub Actions** in Build and deployment.

### Configuration

You can configure the basic information of the blog through `webinfo.json` in the root directory, as shown below:

```json
{
  "Name": "Niltor Blog", // Blog name, displayed at the top navigation of the homepage
  "Description": " for freedom",// Description, displayed in the middle of the top of the homepage
  "AuthorName": "Ater", // Author name, displayed in the blog list
  "BaseHref": "/blazor-blog/", // Subdirectory
  "Domain": "https://aterdev.github.io" // Domain name, used for generating sitemap, leave blank if not generated
}
```

When you use Github Page or deploy using IIS sub-application, you need to adjust `BaseHref`. It is usually your **project name** or **subdirectory name**.

> [!IMPORTANT]
> Note that the `/` at the end of `BaseHref` is required.
>
> If you have configured a custom domain name and are not using a subdirectory, set BaseHref to `/`.

After modification, commit the code, GitHub will trigger Action to automatically build.

### Write a blog

Please use any markdown editor you are used to write a blog, the only requirement is to put the blog content in the `Content` directory. You can create multi-level directories under this directory.

### Publish a blog

You only need to commit the code normally, github action will automatically build and finally publish your blog. After the publication is successful, you can open your GitHub Page to view.

## Deploy to other services

If you don't use Github Page, you can also easily deploy it to other services. The core steps only require two steps.

### Generate static content

The `BuildSite` project is used to convert markdown to html. Please execute in the root directory:

```pwsh
 dotnet run --project .\src\BuildSite\ .\Content .\WebApp Production
```

Where `.\Content` is your markdown storage directory and `.\WebApp` is the generated static site directory.

### Upload to your server

Copy all files in `WebApp` to your server.

> [!TIP]
> The `publishToLocal.ps1` script in the root directory can automatically complete the build and generate operations. The final content will be in the `WebApp` directory in the root directory.
>
> If you use automated deployment, please refer to the scripts in .github/workflows.

## Update

The upstream code update is based on the `dev` branch. You can merge the `dev` branch into your `dev` branch to get the latest code updates.

`main` is the default branch for building and publishing. Please do not merge it into your `main` branch.

It is recommended to use `dev` or your own branch to write blogs and customize content, and then merge it to the `main` branch to trigger automatic build.

## Custom Develop

After forking, you will have all the custom permissions, because all the source codes are already in your own repository.

The core project is `BuildSite`, which is used to generate static files, including converting markdown files to html files.

> [!NOTE]
> It is recommended to create your own branch for custom development content, which is convenient for subsequent merging.

### Development Environment

The `BuildSite` project is a .NET project. You need to install .NET SDK 8.0.

Additionally, you can install (optional):

- **http-server**: To start local static content for debugging.
- **tailwindcss**: To generate CSS style content.
- **typescript**: To use the `tsc` command.

### Running the Project

1. **Preview the project:**
    1. Open the terminal, execute `http-server` in the `WebApp` directory, and then open `http://127.0.0.1:8080` in your browser.
2. **Generate static content:**
    1. Execute `dotnet run --project ./src/BuildSite ./Content ./WebApp` in the root directory to generate the latest static content.
    2. Alternatively, run the `build.ps1` script in the root directory.
3. **Refresh the browser** to see the latest generated content.

>[!TIP]
> If you use `Tailwindcss`, you can execute `npx tailwindcss -o ./css/app.css --watch` in the `WebApp` directory.
> If you use `Typescript`, you can execute `tsc -w` in the `WebApp` directory.

### Customizing the Homepage Content

The homepage content template is located at `src\BuildSite\template\index.html.tpl`. It includes the following variables:

| Template Variable | Description |
|---|---|
| @{BaseUrl} | Base path |
| @{Name} | Blog name |
| @{Description} | Description |
| @{blogList} | Blog list |
| @{siderbar} | Sidebar content: categories and archives |

You can modify the layout and style of the homepage according to your own ideas.

> [!NOTE]
> Pay attention to the `id` attributes in the tags. The `js` script relies on these `id` identifiers. If you modify these identifiers, you must also modify the `js` script.

The homepage content includes the search and category filtering functions of the blog. The functional code is in `WebApp\js\index.js`.

For customizing the blog list and category list, you can refer to the `GenBlogListHtml` and `GenSiderBar` methods in the `HtmlBuilder.cs` file in the `BuildSite` project.

We will provide more flexible customization methods in the future.

### Customizing the Blog Post Page

The blog post content template is located at `src\BuildSite\template\blog.html.tpl`. It includes the following variables:

| Template Variable | Description |
|---|---|
| @{BaseUrl} | Base path |
| @{Title} | Page title |
| @{content} | Blog content |
| @{toc} | Table of contents for secondary titles |

You can modify the style of the blog post page through `WebApp/css/markdown.css` and define the logic through `WebApp/js/markdown.js`.

### Customizing Code Highlighting

This project uses `ColorCode` to format the code content in markdown. `ColorCode` uses regular expressions to match the code content. If you need to define code highlighting, you need to:

1. Add or modify regular expression rules. You can find the corresponding language definition in the `ColorCode.Core/Compilation/Languages` directory. If it does not exist, you can add new language support.
2. If it is a newly added language, you need to load it in `ColorCode.Core/Languages.cs`.

> [!IMPORTANT]
> If you modify the code in the `BuildSite` project, you need to regenerate the static website to see the latest.
