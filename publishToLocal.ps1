$location = Get-Location

# 发布blazor
dotnet publish ./src/ -c Release -o $location/_site

# 生成内容
dotnet run --project ./Lib/BuildSite ./Content ./_site Production

