param(
    [Parameter(Mandatory=$true)]
    [string]$dir
)

$csprojPath = "$dir\ChocolateStoreCore.csproj"
$nuspecPath = "$dir\..\ChocolateyPackages\chocolatestore\chocolatestore.nuspec"
$version = Get-Content "$dir\..\version.txt"

$versionParts = $version.Split('.')
$buildNumber = [int]$versionParts[3]
$buildNumber++
$versionParts[3] = $buildNumber.ToString()
$newVersion = [string]::Join('.', $versionParts)
$versionWithoutBuildNumber = [string]::Join('.', $versionParts[0..2])

Set-Content "$dir\..\version.txt" $newVersion

$csprojContent = Get-Content $csprojPath
$newCsprojContent = $newCsprojContent -replace '(<Version>)[0-9]+(\.[0-9]+){1,2}(</Version>)', "`${1}$versionWithoutBuildNumber`$3"
$newCsprojContent = $csprojContent -replace '(<FileVersion>)[0-9]+(\.[0-9]+){1,3}(</FileVersion>)', "`${1}$newVersion`$3"
$newCsprojContent = $newCsprojContent -replace '(<AssemblyVersion>)[0-9]+(\.[0-9]+){1,3}(</AssemblyVersion>)', "`${1}$newVersion`$3"
Set-Content $csprojPath $newCsprojContent

[xml]$nuspecContent = Get-Content $nuspecPath
$nuspecContent.package.metadata.version = $newVersion
$nuspecContent.Save($nuspecPath)
