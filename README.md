# Blog

📘[Englisth](./README.md)   📘[中文](./README_cn.md)

This tool generates a `pure static` blog website from a `markdown` document through commands. With the help of `GitHub Pages`, you can have a personal blog for free in 5 minutes.It has the following characteristics

- Provide command line tools to generate static websites
- Generate a pure static website with extremely fast access speed
- Support for content written in markdown format
- Support search, classification, archiving and screening
- Customize website name and description, etc

**Demo:** NilTor's Blog: [https://blog.dusi.dev/](https://blog.dusi.dev/)

## 🎖️Features

- Blog list on the homepage, supporting search, category and archive filtering
- Customize website name and description
- Light and Dark themes that change with the system
- Adaptive display for mobile devices
- TOC support
- mermaid, nomnoml, Math rendering support
- Code highlighting and copy support

## 🚀Quick Start

Currently, the tool has been released in the form of 'dotnet tool'.You can easily install and use it.

### Install Tool

First, confirm that you have installed the `dotnet sdk` version 8.0 or higher, and then proceed to install it on the command line

```dotnetcli
dotnet tool install -g Ater.EasyBlog --preview
```

After installation, you can use the `ezblog` command to operate.

### Using tools

We assume that you already have some markdown documents in the `markdown` directory.

Now we use the command:

```pwsh
ezblog init
```

Initialize a 'webinfo.json' file to configure the basic information of your blog. This file can be reused during subsequent generation.The document reads as follows:

```json
{
  "Name": "Niltor Blog", // blog name, displayed at the top of the homepage navigation
  "Description": "🗽 for freedom",// description, displayed in the middle of the top of the homepage
  "AuthorName": "Ater", // Author name, displayed in the blog list
  "BaseHref": "/blazor-blog/", // sub directory
  "Domain": "https://aterdev.github.io" // Domain name, used for generating a sitemap. Leave it blank if not needed
}
```

> [!IMPORTANT]
Please note that the trailing `/` in `BaseHref` is mandatory.
>
If you have configured a custom domain name and are not using a subdirectory, set BaseHref to '/'.

Then we use the command

```pwsh
ezblog build .\markdown .\WebApp
```

This command will convert all markdown files in the 'markdown' directory into html files and generate them into the 'WebApp' directory.

You can use the `http-server` command to start a local server and view the generated content.

The 'WebApp' directory contains everything you need for a static website, and you can freely deploy it wherever you need.

## Deploy with Github Page

### Fork and configure GitHub Page

1. Create a new respository.
2. Go to your own GitHub repository, click **Actions**, and enable workflows.
3. Click **Settings**, find Pages configuration, and select **GitHub Actions** in Build and deployment.

### Writing a blog

We assume that your md documents are all in the 'markdown' directory.

Please use any markdown editor you are used to writing blogs. You can create multiple levels of directories under the 'markdown' directory to categorize md documents.

### Generate static content

Use the build command, such as:

```pwsh
ezblog build .\markdown .\_site
```

> [!NOTE]
> `.\markdown` is the directory where you store md files, and you can freely modify it according to your actual situation. `.\_site` is the generated static website directory.

### Publish blog

Use GitHub Actions to automate the deployment of your blog site.

Create a `build.yml` file in the root directory of the repository, under the `.github/workflows` directory (if it doesn't exist, create it manually). The content should be as follows:

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

      - run: dotnet tool install  -g Ater.EasyBlog --version 1.0.0-beta1
      - run: ezblog build ./Content ./_site
      
      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: '_site/'
          
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

Now you only need to push the code, and GitHub Action will automatically build and finally publish your blog. After the successful publishing, you can open your GitHub Page to view it.

## Deploy to other services

If you don't use Github Page, you can easily deploy to other services as well. The core steps only require two steps.

### Generate static website

We assume that your documents are all located in the 'markdown' directory.
First, use the 'ezblog init' command to generate the 'webinfo. json' configuration file, and modify it according to actual needs.
Then execute 'ezblog build'/ The markdown./wwroot ` command.

### Upload to your server

Copy all files from 'wwwroot' to your server.

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
