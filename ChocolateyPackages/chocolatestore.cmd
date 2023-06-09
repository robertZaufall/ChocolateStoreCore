@echo off
setlocal enabledelayedexpansion

set NAME=chocolatestore
set PACKAGE_FOLDER=%chocolatestore_fileshare%

cd /D "%~dp0"
set NUSPEC_FILE=%NAME%.nuspec

cd .\%NAME%

choco.exe pack %NUSPEC_FILE% --outdir %PACKAGE_FOLDER%

for /f "delims=" %%i in ('findstr /r /c:"<version>.*</version>" %NUSPEC_FILE%') do (
  set LINE=%%i
)
for /f "tokens=3 delims=<>" %%a in ("!LINE!") do set VERSION=%%a

set NUPKG_FILE=%PACKAGE_FOLDER%\%NAME%.%VERSION%.nupkg

echo The NAME is %NAME%.
echo The VERSION is %VERSION%.
echo The NUSPEC_FILE is %NUSPEC_FILE%.
echo The NUPKG_FILE is %NUPKG_FILE%.

pause

@echo on
choco apikey --key %CHOCOLATEY_API_KEY% --source https://push.chocolatey.org/
choco push %NUPKG_FILE% --source https://push.chocolatey.org/

endlocal
pause
