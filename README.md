ChocolateStoreCore
==================

[![](.coverage/badge_combined.svg)](https://htmlpreview.github.io/?https://github.com/robertZaufall/ChocolateStoreCore/blob/main/.coverage/summary.html) &nbsp;
<sup>Line</sup> [![](.coverage/badge_shieldsio_linecoverage_blue.svg)](https://htmlpreview.github.io/?https://github.com/robertZaufall/ChocolateStoreCore/blob/main/.coverage/summary.html) &nbsp;
<sup>Branch</sup> [![](.coverage/badge_shieldsio_branchcoverage_blue.svg)](https://htmlpreview.github.io/?https://github.com/robertZaufall/ChocolateStoreCore/blob/main/.coverage/summary.html) &nbsp;
<sup>Method</sup> [![](.coverage/badge_shieldsio_methodcoverage_blue.svg)](https://htmlpreview.github.io/?https://github.com/robertZaufall/ChocolateStoreCore/blob/main/.coverage/summary.html)  

*Based on the idea of the [ChocolateStore](https://github.com/BahKoo/ChocolateStore) application by [BahKoo](https://github.com/BahKoo)*  
  
## Summary
Download, modify and cache chocolatey packages locally to be delivered through a local repository including binary downloads.  

```mermaid
%%{
  init: {
    "theme": "dark",
    "fontFamily": "Trebuchet MS, Verdana, Arial, Sans-Serif",
    "flowchart": {
      "htmlLabels": true,
      "curve": "linear"
    }
  }
}%%
flowchart TD
    A([ChocolateStoreCore.exe]) --> A1[mode: RUN]
    A1 --call--> A2[[PURGE]]
    A2 --get--> B[package ID]
    B --download latest--> C[/nupkg file/]
    C --dependencies--> B
    C --download--> D[/binaries/]
    C --replace URL--> E[chocolateyInstall.ps1]
    D --save--> F[/folder/]
    E --update--> G[/nupkg file/]
    F --> G2[/file share/]
    G --> G2[/file share/]
    AX([ChocolateStoreCore.exe -p]) --> H[mode: PURGE]
    H --inventory--> I[/file share/]
    I --delete--> J[/old nupkg files/]
    I --delete--> K[/old folders/] 
```  

Usage  

```mermaid
%%{
  init: {
    "theme": "dark",
    "fontFamily": "Trebuchet MS, Verdana, Arial, Sans-Serif",
    "flowchart": {
      "htmlLabels": true,
      "curve": "linear",
      "diagramPadding": 120
    }
  }
}%%
flowchart LR
    A([choco install xyz --source http://x.x.x.x]) --local--> C[/nupkg file/]
    B([choco install xyz --source \\file_share]) --local--> C[/nupkg file/]
    C -- powershell --> D[chocolateyInstall.ps1]
    D -- download from<br/>local webserver --> E[/binaries/]
```  

## License
Apache 2.0

## Compilation requirements
* Visual Studio 2022
* .NET 7.0

## Package dependencies
```
- CommandLineParser
- Newtonsoft.Json
- NuGet
- Polly
- Serilog
- (Test) AutoFixture
- (Test) FluentAssertions
- (Test) Moq
- (Test) xunit
```

## Syntax
Run `ChocolateStoreCore.exe` with existing `appsettings.json` configuration in same folder as the exe file and optional additional processing flags  
* `-p or --purge` purge only  
* `-w or --whatif` no writing or deletion of files  

## Configuration
#### `appsettings.json` example
```json
{
  "ChocolateyConfiguration": {
    "LocalRepoUrl": "http://192.168.1.1:8080",
    "ApiUrl": "https://community.chocolatey.org",
    "ApiUserAgent": "User-Agent: ChocolateStoreCore",
    "ApiPath": "/api/v2",
    "ApiPackageRequest": "/Packages()?$filter=(tolower(Id)%20eq%20'{0}')%20and%20IsLatestVersion",
    "ApiPackageRequestWithVersion": "/Packages()?$filter=(tolower(Id)%20eq%20'{0}')%20and%20version%20eq%20'{1}'",
    "ApiFindAllRequest": "/FindPackagesById()?id='{0}.app'",
    "ApiFindAllNextRequest": "/FindPackagesById?id='{0}'&$skiptoken='{0}','{1}'",
    "ApiGetRequest": "/package/{0}/{1}",
    "OptionalRemoteDownloadUrl": "https://packages.chocolatey.org/{0}.{1}.nupkg",
    "FolderPath": "c:\\deploy\\App_Data\\Packages",
    "DownloadListPath": "c:\\ChocolateStoreCore\\download.txt",
    "HttpTimeout": "5",
    "HttpTimeoutOverAll": "10",
    "HttpRetries": "3",
    "HttpHandlerLifetime": "10",
    "LogFile": "log.txt",
    "LogLevel": "Warning"
  }
}
```

| parameter             | description |  
| :---                  | :--- |  
| `LocalRepoUrl`        | url of the local webserver where the binaries are to be downloaded from |  
| `FolderPath`          | file location to cache the nupkg files and optional folders for download artefacts. a relative path is supported (e.g. ```.``` or ```.\\store```). the directory must exist |  
| `DownloadListPath`    | file location for the file containing the desired chocolatey ids (one line for each id). a relative path is supported (e.g. ```.\\download.txt```). |  
| `HttpTimeout`         | [s] |  
| `HttpTimeoutOverAll`  | [min] |  
| `HttpHandlerLifetime` | [min] |  
  
#### `download.txt` example 
```
googlechrome
nodejs
sysinternals
```

## Output
Example output:  
![cmdline output](.github/ChocolateStoreCore.png)

## Package installation  
From webserver  
```choco.exe Install firefox --source http://192.168.1.1:8080 -y```  

From fileshare (webserver is still needed for downloading the binaries!)  
```choco.exe Install firefox --source \\192.168.1.1\_deploy$\App_Data\Packages -y```  

I can recommend ['civetweb'](https://github.com/civetweb/civetweb/releases) as a simple webserver to deliver the binaries.

## Alternative
Chocolatey's (business edition) own feature ['Package Internalizer'](https://chocolatey.org/docs/features-automatically-recompile-packages).
