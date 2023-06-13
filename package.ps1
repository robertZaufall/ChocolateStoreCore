param (
    [string]$dir = "$pwd\ChocolateStoreCore"
)

$dirRelease = "$dir\..\release"
$dirPackage = "$dir\..\ChocolateyPackages\chocolatestore"
$dirPackageBinaries = "$dirPackage\tools"
$dirPackageNuspec = "$dirPackage\chocolatestore.nuspec"
$installps1 = "$dirPackageBinaries\chocolateyInstall.ps1"
$zip = 'ChocolateStoreCore.zip'

If (Test-Path (Join-Path $dirRelease $zip)) {
    Copy-Item (Join-Path $dirRelease $zip) -Destination (Join-Path $dirPackageBinaries $zip) -Force
} else {
    Write-Error 'Zip archive not found.'
}

$hash = (Get-FileHash -Path (Join-Path $dirRelease $zip) -Algorithm SHA256).Hash
$content = Get-Content -Path $installps1
$newContent = $content -replace "(?<=checksum\s*=\s*')[^']*", $hash
$newContent | Set-Content -Path $installps1

[xml]$nuspecContent = Get-Content $dirPackageNuspec
$nuspecContent.package.metadata.version = [string](Get-Content "$dir\..\version.txt")
$nuspecContent.Save($dirPackageNuspec)

choco pack $dirPackageNuspec --outdir $dirRelease
