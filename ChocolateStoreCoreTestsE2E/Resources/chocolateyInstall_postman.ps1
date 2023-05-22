$packageName= 'postman'
$toolsDir   = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)"
$url64      = 'https://dl.pstmn.io/download/version/9.22.2/win64'

$packageArgs = @{
  packageName   = $packageName
  fileType      = 'exe'
  silentArgs    = "-s"
  url64bit      = $url64
  checksum64    = '60ccd5ba613c471419e297ac66c7e91422b986836d4a1553e34ccb3dac1a146c'
  checksumType64= 'sha256'
}

Install-ChocolateyPackage @packageArgs
