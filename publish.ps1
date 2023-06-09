param (
    [string]$dir = "$pwd\ChocolateStoreCore"
)

$projectFile = "$dir\ChocolateStoreCore.csproj"
$dirRelease = "$dir\..\release"
$dirReleaseAssets = "$dirRelease\ChocolateStoreCore"
$dirReleaseAssetsStore = "$dirReleaseAssets\store"
$zip = 'ChocolateStoreCore.zip'
If (Test-Path $dirRelease) { Remove-Item $dirRelease -Recurse -Force }
        
dotnet publish $projectFile -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true /p:PublishTrimmed=false /p:PublishDir="$dirReleaseAssets"

New-Item -ItemType Directory -Path $dirReleaseAssetsStore
New-Item -Path (Join-Path -Path $dirReleaseAssetsStore -ChildPath 'dummy.txt') -ItemType File
Compress-Archive -Path "$dirReleaseAssets\*" -DestinationPath (Join-Path $dirRelease $zip)
If (Test-Path $dirReleaseAssets) { Remove-Item $dirReleaseAssets -Recurse -Force }
