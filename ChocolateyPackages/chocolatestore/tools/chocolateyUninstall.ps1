$packageName  = 'chocolatestore'
$exeName      = 'ChocolateStoreCore.exe'
$AppPathKey   = "Registry::HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\$exeName"

Uninstall-ChocolateyZipPackage chocolatestore ChocolateStoreCore.zip

If (Test-Path $AppPathKey) {Remove-Item "$AppPathKey" -Force -Recurse -EA SilentlyContinue | Out-Null}

$destination     = Join-Path ${env:ProgramFiles(x86)} 'ChocolateStore'
$destination64   = Join-Path ${env:ProgramFiles} 'ChocolateStore'
$destinationLink = Join-Path 'C:\Users\Public\Desktop' 'ChocolateStore.lnk'

If (Test-Path $destinationLink){
	Remove-Item $destinationLink -Force -Confirm:$false
}

If (Test-Path $destination){
	Remove-Item $destination -Recurse -Force -Confirm:$false
}

If (Test-Path $destination64){
	Remove-Item $destination64 -Recurse -Force -Confirm:$false
}
