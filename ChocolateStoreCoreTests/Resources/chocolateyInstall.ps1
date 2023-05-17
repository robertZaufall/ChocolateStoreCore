$ErrorActionPreference = 'Stop';
$packageArgs = @{
    packageName    = 'Test'
    url            = 'http://xyz/test.msi'
    Url64bit       = 'http://xyz/test64.msi'
    fileType       = "msi"
    softwareName   = 'Test'
    validExitCodes = @(0)
}
Install-ChocolateyPackage @packageArgs
