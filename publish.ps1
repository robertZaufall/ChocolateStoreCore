$root = "$pwd"

$dir = "$root\ChocolateStoreCore"
$projectFile = "$dir\ChocolateStoreCore.csproj"

$dirRelease = "$root\release"
$dirReleaseAssets = "$dirRelease\ChocolateStoreCore"
$dirReleaseAssetsStore = "$dirReleaseAssets\store"

$dirPackage = "$root\ChocolateyPackages\chocolatestore"
$dirPackageNuspec = "$dirPackage\chocolatestore.nuspec"
$dirPackageBinaries = "$dirPackage\tools"
$installps1 = "$dirPackageBinaries\chocolateyInstall.ps1"

$zip = 'ChocolateStoreCore.zip'

If (Test-Path $dirRelease) { Remove-Item $dirRelease -Recurse -Force }

dotnet publish $projectFile -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:PublishDir="$dirReleaseAssets"

New-Item -ItemType Directory -Path $dirReleaseAssetsStore
New-Item -Path (Join-Path -Path $dirReleaseAssetsStore -ChildPath 'dummy.txt') -ItemType File

Compress-Archive -Path "$dirReleaseAssets\*" -DestinationPath (Join-Path $dirRelease $zip)
Copy-Item (Join-Path $dirRelease $zip) -Destination (Join-Path $dirPackageBinaries $zip) -Force

$hash = (Get-FileHash -Path (Join-Path $dirRelease $zip) -Algorithm SHA256).Hash
$content = Get-Content -Path $installps1
$newContent = $content -replace "(?<=checksum\s*=\s*')[^']*", $hash
$newContent | Set-Content -Path $installps1

choco pack $dirPackageNuspec --outdir $dirRelease

If (Test-Path (Join-Path $dirPackageBinaries $zip)) {
    Remove-Item (Join-Path $dirPackageBinaries $zip) -Force 
}

[xml]$nuspecContent = Get-Content $dirPackageNuspec
$version = $nuspecContent.package.metadata.version.ToString()
$package = "$dirRelease\chocolatestore.$version.nupkg"
$ziparchive = (Join-Path $dirRelease $zip)

