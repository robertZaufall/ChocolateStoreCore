dotnet tool install -g dotnet-reportgenerator-globaltool
dotnet tool update  -g dotnet-reportgenerator-globaltool
$dir = "$pwd\ChocolateStoreCoreTests"
$license = [System.Environment]::GetEnvironmentVariable('license', 'Machine')

Remove-Item -Recurse -Force $dir/coveragereport/
Remove-Item -Recurse -Force $dir/TestResults/
if (!(Test-Path -path $dir/CoverageHistory)) {  
  New-Item -ItemType directory -Path $dir/CoverageHistory
}

dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"$dir\**\coverage.cobertura.xml" -targetdir:"$dir\coveragereport" -reporttypes:Html -historydir:$dir/CoverageHistory -license:"$license"

$osInfo = Get-CimInstance -ClassName Win32_OperatingSystem
(& "$dir/coveragereport/index.html")
