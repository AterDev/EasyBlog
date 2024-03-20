# 创建Blazor项目

FluentUI

## 使用TailwindCSS

安装

`npx tailwindcss init`

配置 tailwind.config.js

```typescript
module.exports = {
  content: ['./**/*.{razor,html}'],
  theme: {
    extend: {},
  },
  plugins: [],
}
```

创建一个开发时使用的app.css，并引入基础组件

```csharp

using System.Text;
using Markdig;
using Markdown.ColorCode;
using Markdown.ColorCode.CSharpToColoredHtml;

namespace BuildSite;

public class HtmlBuilder
{
    public void BuildBlogs()
    {
        var contentPath = Path.Combine(Environment.CurrentDirectory, "..", "Content");
        var wwwrootPath = Path.Combine(Environment.CurrentDirectory, "..", "src", "wwwroot");
        try
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseColorCodeWithCSharpToColoredHtml(HtmlFormatterType.CssWithCSharpToColoredHtml)
                .Build();


            var files = Directory.EnumerateFiles(contentPath, "*.md", SearchOption.AllDirectories)
                .ToList();

            files.ForEach(file =>
                {
                    var markdown = File.ReadAllText(file);
                    var html = Markdig.Markdown.ToHtml(markdown, pipeline);
                    var relativePath = file.Replace(contentPath, wwwrootPath).Replace(".md", ".html");

                    html = AddHtmlTags(html);
                    var dir = Path.GetDirectoryName(relativePath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.WriteAllText(relativePath, html, Encoding.UTF8);
                });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }

    private string AddHtmlTags(string content, string title = "")
    {
        var res = $"""
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
              <link rel="stylesheet" href="css/app.css">
              <title>{title}</title>
            </head>
            <body>
            {content}
            </body>
            </html>
            """;
        return res;
    }
}

```

## Css

```css
@tailwind base;
@tailwind components;
@tailwind utilities;
```

开启监测模式，并设置输出路径

`npx tailwindcss -i ./wwwroot/css/input.css -o ./wwwroot/css/output.css --watch`

## 使用FluentUI

安装

`Microsoft.FluentUI.AspNetCore.Components`

`Microsoft.FluentUI.AspNetCore.Components.Icons`
