param(
    [Parameter(Mandatory=$true)]
    [string]$projectDir
)

$csprojPath = "$projectDir\ChocolateStoreCore.csproj"
$nuspecPath = "$projectDir\..\ChocolateyPackages\chocolatestore\chocolatestore.nuspec"
$version = Get-Content "$projectDir\..\version.txt"

$versionParts = $version.Split('.')
$buildNumber = [int]$versionParts[3]
$buildNumber++
$versionParts[3] = $buildNumber.ToString()
$newVersion = [string]::Join('.', $versionParts)
$versionWithoutBuildNumber = [string]::Join('.', $versionParts[0..2])

Set-Content "$projectDir\..\version.txt" $newVersion

$csprojContent = Get-Content $csprojPath
$newCsprojContent = $newCsprojContent -replace '(<Version>)[0-9]+(\.[0-9]+){1,2}(</Version>)', "`${1}$versionWithoutBuildNumber`$3"
$newCsprojContent = $csprojContent -replace '(<FileVersion>)[0-9]+(\.[0-9]+){1,3}(</FileVersion>)', "`${1}$newVersion`$3"
$newCsprojContent = $newCsprojContent -replace '(<AssemblyVersion>)[0-9]+(\.[0-9]+){1,3}(</AssemblyVersion>)', "`${1}$newVersion`$3"
Set-Content $csprojPath $newCsprojContent

[xml]$nuspecContent = Get-Content $nuspecPath
$nuspecContent.package.metadata.version = $newVersion
$nuspecContent.Save($nuspecPath)
