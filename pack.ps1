[CmdletBinding()]
param (
    [Parameter()]
    [bool]
    $install = $false
)

Write-Host 'Packing new version...'
dotnet build  ./src/BuildSite -c release 
dotnet pack  ./src/BuildSite -c release 

if ($install) {
    # get package name and version
    $VersionNode = Select-Xml -Path ./src/BuildSite/BuildSite.csproj -XPath '/Project//PropertyGroup/Version'
    $PackageNode = Select-Xml -Path ./src/BuildSite/BuildSite.csproj -XPath '/Project//PropertyGroup/PackageId'
    $Version = $VersionNode.Node.InnerText
    $PackageId = $PackageNode.Node.InnerText

    # uninstall old version
    Write-Host 'uninstall old version'
    dotnet tool uninstall -g $PackageId
 
    Write-Host 'install new version:'$PackageId $Version
    dotnet tool install -g --add-source ./src/BuildSite/nupkg $PackageId --version $Version
}