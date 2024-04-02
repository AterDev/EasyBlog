# 如何部署ASP.NET Core 到Linux服务器
我们开发的最终目的，是将开发后的东西发布网络上，以便自己及其他人使用。

本篇博客介绍如果在 linux 上部署 ASP.NET Core应用，使用nginx+systemd 来管理我们的应用。


## 准备

- Ubuntu 20.04
- Nginx
- .NET SDK 或 Runtime

## 安装

- [安装Nginx](https://docs.nginx.com/nginx/admin-guide/installing-nginx/installing-nginx-open-source/)
- [安装.NET](https://learn.microsoft.com/zh-cn/dotnet/core/install/linux)

## 配置

### 配置Nginx

#### 配置文件

nginx默认配置会加载`/etc/nginx/sites-enabled`中的网站，通常我们并不会在该目录下直接配置网站，而只是定义需要生效网站的软链接，实际定义文件通常在`/etc/nginx/sites-available`目录去定义。

```bash
cd /etc/nginx/sites-availabe
sudo vim my-web

```

以为下nginx 的配置示例

```nginx
server {
    listen        80;
    server_name _;
    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

> [!TIP]
> proxy_pass，意思是用户访问先经过nginx，然后nginx会路由到 proxy_pass 地址，这个地址也就是asp.net core 服务实际运行使用的地址.

#### 让配置文件生效

我们已经在`/etc/nginx/sites-available`下创建了 `my-web`配置文件，现在来创建软链接。

```bash
cd  /etc/nginx/sites-enable
sudo ln -s /etc/nginx/sites-available/my-web  ./
```

接下来我们可以使用`nginx -t`来测试配置文件是否正确。
最后使用配置文件生效，可使用`nginx -s reload`来重载nginx的配置，以使新站点配置生效。

```bash
sudo nginx -t 
sudo nginx -s reload
```

### 配置服务以自动启动

`systemctl` 命令用于管理“服务”或“守护程序”。 守护程序的概念与 Windows 服务的概念类似。 当系统启动时，可以自动重启此类服务。

我们需要将我们的应用配置成服务，这样就可以自动运行。

#### 创建服务文件

在`/etc/systemd/system/`目录下创建服务文件，

```bash
cd /etc/systemd/system/
sudo vim dusi.service
```

内容如下:

```ini
[Unit]
Description=dusi web site

[Service]
WorkingDirectory=/var/dusi/
ExecStart=/usr/bin/dotnet /var/dusi/Http.API.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dusi-log
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

以下是此内容的一些关键方面：

- `WorkingDirectory` 是发布应用程序的目录。
- `ExecStart` 是启动应用程序的实际命令,**dotnet 需要指定到使用的dll文件**
- Restart=always  果由于某种原因（无论是手动还是由于崩溃）而停止，则始终会启动此过程。
- RestartSec=10 进程停止后，将在经过 10 秒后启动。
- `SyslogIdentifier` 很重要。 它表示“系统日志标识符”。 有关守护程序的信息记录在此名称下的系统日志中。 还可以使用此标识符查找进程的 PID。
- `User` 是管理服务的用户。 它应存在于系统中，并具有相应的应用程序文件所有权。
- `Environment`是设置应用运行时使用的环境变量，可以在设置任意数量的环境变量。

### 让服务文件生效

如同以上`nginx`配置，我们定义了文件，同时也需要创建软链接，以让系统使用它，不过服务我们可以通过命令来创建软链接.

```bash
sudo systemctl enable dusi.service
```

### 启动服务并查看状态

使用`systemctl start`启动服务，然后通过`systemctl status`命令查看服务运行状态。

```bash
sudo systemctl start stock.service
sudo systemctl status stock.service
```

## 部署

部署指将你的文件上传到服务器上，请注意上传的路径最终要和上面服务配置文件中的路径对应。

上传文件到服务器有很多种方式，这里我们使用`scp`手动传输我们的文件，当然你可以配置其他方式或使用CICD。

示例命令：

```powershell
scp /my/publish/*  user@10.10.10.1/var/dusi/
```

> [!NOTE]
> Windows系统`Powershell`(非Windows Powershell)也可以使用`scp`命令，你并不需要安装额外的软件。

## 其他参考

### 查看日志

在linux 中，你可以通过`journalctl`命令查看日志，示例:
`journalctl -fu dusi.service`

### 参考资料

- [使用 Nginx 在 Linux 上托管 ASP.NET Core](https://learn.microsoft.com/zh-cn/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-6.0&tabs=linux-ubuntu)
- [在 Linux 中创建和配置 ASP.NET Core应用程序](https://learn.microsoft.com/zh-cn/troubleshoot/developer/webapps/aspnetcore/practice-troubleshoot-linux/2-1-create-configure-aspnet-core-applications)
