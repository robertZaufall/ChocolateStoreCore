param (
    [string]$dir = "$pwd\ChocolateStoreCore"
)

$dirRelease = "$dir\..\release"
$dirPackage = "$dir\..\ChocolateyPackages\chocolatestore"
$dirPackageBinaries = "$dirPackage\tools"
$dirPackageNuspec = "$dirPackage\chocolatestore.nuspec"
$installps1 = "$dirPackageBinaries\chocolateyInstall.ps1"
$zip = 'ChocolateStoreCore.zip'

Copy-Item (Join-Path $dirRelease $zip) -Destination (Join-Path $dirPackageBinaries $zip) -Force
$hash = (Get-FileHash -Path (Join-Path $dirRelease $zip) -Algorithm SHA256).Hash
$content = Get-Content -Path $installps1
$newContent = $content -replace "(?<=checksum\s*=\s*')[^']*", $hash
$newContent | Set-Content -Path $installps1

choco pack $dirPackageNuspec --outdir $dirRelease