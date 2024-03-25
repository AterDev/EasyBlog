$location = Get-Location

# 生成内容
Set-Location  ./Lib/BuildSite
dotnet run

# 发布blazor
Set-Location $location
dotnet publish ./src/ -c Release -o $location/_site

