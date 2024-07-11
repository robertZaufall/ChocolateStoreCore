$packageName  = 'chocolatestore'
$toolsPath    = Split-Path $MyInvocation.MyCommand.Definition
$fileFullPath = "$toolsPath\ChocolateStoreCore.zip"
$destination  = Join-Path ${env:ProgramFiles} 'ChocolateStore'
$exeName      = 'ChocolateStoreCore.exe'

$packageArgs = @{
  packageName    = $packageName
  filefullpath   = $fileFullPath
  destination    = $destination
  checksum       = '2764745D7CEFC6F0C85532FD992F74816525222B7F4CAC3CF0D1754944B4CDC1'
  checksumType   = 'sha256'                
}

$filesToBackup = @(
    @("appsettings.json", "appsettings.json.bak"),
    @("download.txt", "download.txt.bak")
)

# backup
$filesToBackup | %{ if(Test-Path (Join-Path $destination $_[0])) { Copy-Item (Join-Path $destination $_[0]) -Destination (Join-Path $destination $_[1]) -Force } }

# install
Get-ChocolateyUnzip @packageArgs

# restore
$filesToBackup | %{ if(Test-Path (Join-Path $destination $_[1])) { Copy-Item (Join-Path $destination $_[1]) -Destination (Join-Path $destination $_[0]) -Force } }

# V1.1.0: patch appsettings.json
if(Test-Path (Join-Path $destination "appsettings.json")) {
    $oldString = '"ApiPackageRequestWithVersion": "/Packages()?$filter=(tolower(Id)%20eq%20''{0}'')%20and%20version%20eq%20''{1}''"'
    $newString = '"ApiPackageRequestWithVersion": "/Packages()?$filter=(tolower(Id)%20eq%20''{0}'')%20and%20(Version%20eq%20''{1}'')"' 
    $appsettings = Get-Content (Join-Path $destination "appsettings.json") -Raw
    $appsettings = $appsettings -replace [regex]::Escape($oldString), $newString
    $appsettings | Set-Content (Join-Path $destination "appsettings.json")
}

$destinationLink   = Join-Path 'C:\Users\Public\Desktop' 'ChocolateStore.lnk'
Install-ChocolateyShortcut -ShortcutFilePath $destinationLink -TargetPath $destination

$AppPathKey = "Registry::HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\$exeName"
If (!(Test-Path $AppPathKey)) {New-Item "$AppPathKey" | Out-Null}
Set-ItemProperty -Path $AppPathKey -Name "(Default)" -Value "$env:chocolateyinstall\lib\$packageName\tools\$exeName"
Set-ItemProperty -Path $AppPathKey -Name "Path" -Value "$env:chocolateyinstall\lib\$packageName\tools\"
