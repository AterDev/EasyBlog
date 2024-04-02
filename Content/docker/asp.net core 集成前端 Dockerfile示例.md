# asp.net core 集成前端 Dockerfile 示例
---
## 说明

使用node构建前端项目，生成静态文件，然后copy到web项目下`wwwroot`目录下。
这样前后端只需要一个容器，只需要启动后端服务即可。

```docker
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Http.API/Http.API.csproj", "src/Http.API/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Share/Share.csproj", "src/Share/"]
COPY ["src/Core/Core.csproj", "src/Core/"]
COPY ["src/Database/EntityFramework/EntityFramework.csproj", "src/Database/EntityFramework/"]
RUN dotnet restore "src/Http.API/Http.API.csproj"
COPY . .
WORKDIR "/src/src/Http.API"
RUN dotnet build "Http.API.csproj" -c Release -o /app/build


# node构建
FROM node:18.15-alpine AS node
WORKDIR /src
COPY ./src/Http.API/ClientApp .
RUN npm install
RUN npm run build -- --configuration production


FROM build AS publish
RUN dotnet publish "Http.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node /src/dist ./wwwroot
ENTRYPOINT ["dotnet", "Http.API.dll"]
```