$packageName  = 'chocolatestore'
$toolsPath    = Split-Path $MyInvocation.MyCommand.Definition
$fileFullPath = "$toolsPath\ChocolateStoreCore.zip"
$destination  = Join-Path ${env:ProgramFiles} 'ChocolateStore'
$exeName      = 'ChocolateStoreCore.exe'

$packageArgs = @{
  packageName    = $packageName
  filefullpath   = $fileFullPath
  destination    = $destination
  checksum       = '4FE47D1514735BD74D7529D3439165AFD1C3EDEDCDEA12EB05C01D325B233A5D'
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

$destinationLink   = Join-Path 'C:\Users\Public\Desktop' 'ChocolateStore.lnk'
Install-ChocolateyShortcut -ShortcutFilePath $destinationLink -TargetPath $destination

$AppPathKey = "Registry::HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\$exeName"
If (!(Test-Path $AppPathKey)) {New-Item "$AppPathKey" | Out-Null}
Set-ItemProperty -Path $AppPathKey -Name "(Default)" -Value "$env:chocolateyinstall\lib\$packageName\tools\$exeName"
Set-ItemProperty -Path $AppPathKey -Name "Path" -Value "$env:chocolateyinstall\lib\$packageName\tools\"
