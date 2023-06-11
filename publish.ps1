$dir = "$pwd\ChocolateStoreCore"
$dirPackageBinaries = "$pwd\ChocolateyPackages\chocolatestore\tools"
$dirPackageNuspec = "$pwd\ChocolateyPackages\chocolatestore\chocolatestore.nuspec"
$dirRelease = "$pwd\release"
$dirReleaseAssets = "$pwd\release\ChocolateStoreCore"
$zip = 'ChocolateStoreCore.zip'
$projectFile = "$dir\ChocolateStoreCore.csproj"
$installps1 = "$pwd\ChocolateyPackages\chocolatestore\tools\chocolateyInstall.ps1"

If (Test-Path $dirRelease) { Remove-Item $dirRelease -Recurse -Force }

dotnet publish $projectFile -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:PublishDir="$dirReleaseAssets"

New-Item -ItemType Directory -Path (Join-Path $dirReleaseAssets 'store')
New-Item -Path (Join-Path -Path (Join-Path -Path $dirReleaseAssets -ChildPath 'store') -ChildPath 'dummy.txt') -ItemType File

Compress-Archive -Path "$dirReleaseAssets\*" -DestinationPath (Join-Path $dirRelease $zip)
Copy-Item (Join-Path $dirRelease $zip) -Destination (Join-Path $dirPackageBinaries $zip) -Force

$hash = (Get-FileHash -Path (Join-Path $dirRelease $zip) -Algorithm SHA256).Hash
$content = Get-Content -Path $installps1
$newContent = $content -replace "(?<=checksum\s*=\s*')[^']*", $hash
$newContent | Set-Content -Path $installps1

choco pack $dirPackageNuspec --outdir $dirRelease
