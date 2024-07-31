# Blog

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
