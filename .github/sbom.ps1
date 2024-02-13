dotnet tool install --global Microsoft.Sbom.DotNetTool
sbom-tool generate -b .\..\ChocolateStoreCore\bin\Release\net8.0\win-x86 -bc .\..\ChocolateStoreCore -pn ChocolateStoreCore -pv 1.0.0 -ps RobertZaufall -nsb https://sbom.zaufall.de -m .\..\.sbom
