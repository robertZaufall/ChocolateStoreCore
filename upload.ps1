param (
    [string]$dir = "$pwd\ChocolateStoreCore"
)

$dirPackageNuspec = "$dir\..\ChocolateyPackages\chocolatestore\chocolatestore.nuspec"
$dirRelease = "$dir\..\release"

[xml]$nuspecContent = Get-Content $dirPackageNuspec
$version = $nuspecContent.package.metadata.version.ToString()

$package = "$dirRelease\chocolatestore.$version.nupkg"
Write-Host "Package: $package"

If (Test-Path $package) {
    choco apikey --key $env:CHOCOLATEY_API_KEY --source https://push.chocolatey.org/
    choco push $package --source https://push.chocolatey.org/
} else {
    Write-Error "Package not found: $package"
}
