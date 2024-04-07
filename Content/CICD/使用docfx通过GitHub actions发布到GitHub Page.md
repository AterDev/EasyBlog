# 使用docfx发布到GitHub Page
---

## 什么是Docfx
Docfx是基于dotnet 的静态网站工具，被微软官方用来编写和生成官方文档；现在开源并调整为更通用的静态内容生成。

## 内容概要
使用docfx 创建静态网站，然后开通github pages，通过github actions去自动构建和发布静态网站内容的脚本参考。


## 脚本文件
```yml
on:
  push:
    branches: ["dev"]
  workflow_dispatch:

# Sets permissions of the GITHUB_TOKEN to allow deployment to GitHub Pages
permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: "pages"
  cancel-in-progress: false


jobs:
  publish-docs:
    runs-on: ubuntu-latest
    steps:
    - name: Chekout
      uses: actions/checkout@v3
      
    - name: Dotnet Setup
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 7.x

    - run: dotnet tool update -g docfx
    - run: docfx build ./zh/docfx-zh.json
    - run: docfx build ./en/docfx-en.json
    - run: rm -rf ./_site/styles/*.map
    - run: cp -f ./src/index.html ./_site/

    - name: Upload GitHub Pages artifact
      uses: actions/upload-pages-artifact@v1.0.8
        
    - name: Deploy GitHub Pages site
      uses: actions/deploy-pages@v2.0.1
      with:
        token:  ${{ secrets.GITHUB_TOKEN }}

```