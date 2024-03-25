$location = Get-Location

# 生成内容
dotnet run --project ./Lib/BuildSite

# 发布blazor
Set-Location $location
dotnet publish ./src/ -c Release -o $location/_site

