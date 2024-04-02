# 禁用Windows的网络搜索，以实现秒开应用程序

## 问题

当我们按下`Win`可`Win+S`快捷键时，windows会弹出搜索或开始菜单，这时我们可以键入我们想要搜索的内容。
但很多时候，Windows会搜索网络上的信息，导致本地的应用也迟迟得不到响应！

在实际场景中，我使用该功能是快速打开本地应用，而不会进行网络搜索，如果我需要搜索内容，会使用浏览器，或直接使用`Copilot`。

## 方法

具体措施：

- 修改组策略
- 修改注册表

### 修改组策略

使用`Win+R`，然后输入`gpedit.msc`，回车。

找到计算机配置->管理模板->Windows组件->搜索。

将`不允许 Web搜索`和`请勿在Web中搜索或在"搜索"中显示Web结果`，两个配置设置为已启用。

### 修改注册表

使用`Win+R`，然后输入`regedit`，回车。

在地址栏中，复制`计算机\HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search`，回车。

新建`DWORD`值，名称为`BingSearchEnable`，值为1。

或者你可以使用脚本：

1. 首先创建一个文件，如：`disableWindowsWebSearch.ps1`。
2. 将下面脚本内容复制到文件，然后保存。

   ```powershell
    New-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Search" -Name "BingSearchEnable" -Value 1 -PropertyType DWORD -Force

   ```

3. 打开windows终端(管理员模式)，运行该脚本！
