<div align="center">   
  <p>   
    <a href="https://github.com/BornToBeroot/NETworkManager/releases" target="_blank">
      <img alt="All releases" src="https://img.shields.io/github/downloads/BornToBeroot/NETworkManager/total.svg?style=flat-square" />
    </a>
    <a href="https://github.com/BornToBeroot/NETworkManager/stargazers" target="_blank">
      <img alt="GitHub stars" src="https://img.shields.io/github/stars/BornToBeroot/NETworkManager.svg?style=flat-square" />
    </a>    
    <a href="https://github.com/BornToBeroot/NETworkManager/network" target="_blank">       
      <img alt="GitHub forks" src="https://img.shields.io/github/forks/BornToBeroot/NETworkManager.svg?style=flat-square" />
    </a>        
  </p> 
  <p> 
    <a href="https://ci.appveyor.com/project/BornToBeRoot/NETworkManager/branch/master">
      <img alt="AppVeyor" src="https://img.shields.io/appveyor/ci/BornToBeRoot/NETworkManager/master.svg?style=flat-square&&label=master" />
    </a>   
    <a href="https://ci.appveyor.com/project/BornToBeRoot/NETworkManager/branch/net5-develop">
      <img alt="AppVeyor" src="https://img.shields.io/appveyor/ci/BornToBeRoot/NETworkManager/net5-develop.svg?style=flat-square&&label=net5-develop" />
    </a>   
  </p> 
  <p> 
    <a href="https://transifex.com/BornToBeRoot/NETworkManager/">
      <img alt="Transifex" src="https://img.shields.io/badge/transifex-translate-green.svg?style=flat-square" />
    </a>   
    <a href="https://github.com/BornToBeRoot/NETworkManager/issues/new?labels=Feature-Request&template=Feature_request.md">
      <img alt="Transifex" src="https://img.shields.io/badge/github-feature_request-green.svg?style=flat-square" />
    </a>   
    <a href="https://github.com/BornToBeRoot/NETworkManager/issues/new?labels=Issue&template=Bug_report.md">
      <img alt="Transifex" src="https://img.shields.io/badge/github-bug_report-red.svg?style=flat-square" />
    </a>     
  </p> 
  <p> https://borntoberoot.net/NETworkManager/ </p>
  <img alt="NETworkManager Preview" src="https://github.com/BornToBeRoot/NETworkManager/blob/gh-pages/NETworkManager_Preview.gif?raw=true" />

  
</div>

# Build

__Requirements__
- [SDK .NET 5.0 (Preview 8 or later)](https://dotnet.microsoft.com/download/dotnet/5.0)

__Steps__
1) Create an empty folder
2) Open a PowerShell and navigate to this folder
3) Run the build script:
```powershell
iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/BornToBeRoot/NETworkManager/net5-develop/build.ps1'))
```
