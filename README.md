<div align="center">       
  <h1>NETworkManager</h1>
  <h3>A powerful tool for managing networks and troubleshoot network problems!</h3>
  
  <img alt="NETworkManager Preview" src="https://github.com/BornToBeRoot/NETworkManager/blob/master/docs/Preview.gif?raw=true" />
  
  <p>   
    <a href="https://github.com/BornToBeroot/NETworkManager/releases" target="_blank">
      <img alt="All releases" src="https://img.shields.io/github/downloads/BornToBeroot/NETworkManager/total.svg?style=for-the-badge&logo=github" />
    </a>    
    <a href="https://github.com/BornToBeroot/NETworkManager/releases/latest" target="_blank">
      <img alt="Latest release" src="https://img.shields.io/github/downloads/BornToBeroot/NETworkManager/latest/total.svg?style=for-the-badge&logo=github" />
    </a>    
    <a href="https://github.com/BornToBeroot/NETworkManager/stargazers" target="_blank">
      <img alt="GitHub stars" src="https://img.shields.io/github/stars/BornToBeroot/NETworkManager.svg?style=for-the-badge&logo=github" />
    </a>    
    <a href="https://github.com/BornToBeroot/NETworkManager/network" target="_blank">       
      <img alt="GitHub forks" src="https://img.shields.io/github/forks/BornToBeroot/NETworkManager.svg?style=for-the-badge&logo=github" />
    </a>     
  </p> 
  <p> 
    <a href="https://ci.appveyor.com/project/BornToBeRoot/NETworkManager/branch/master">
      <img alt="AppVeyor" src="https://img.shields.io/appveyor/ci/BornToBeRoot/NETworkManager/master.svg?style=for-the-badge&logo=appveyor&&label=master" />
    </a>   
    <a href="https://github.com/BornToBeRoot/NETworkManager/blob/master/LICENSE">
      <img alt="AppVeyor" src="https://img.shields.io/github/license/BornToBeroot/NETworkManager.svg?style=for-the-badge&logo=github" />
    </a>     
  </p> 
  <p> 
    <a href="https://transifex.com/BornToBeRoot/NETworkManager/">
      <img alt="Transifex" src="https://img.shields.io/badge/transifex-translate-green.svg?style=for-the-badge" />
    </a>   
    <a href="https://github.com/BornToBeRoot/NETworkManager/issues/new?labels=Feature-Request&template=Feature_request.md">
      <img alt="Feature request" src="https://img.shields.io/badge/github-feature_request-green.svg?style=for-the-badge&logo=github" />
    </a>       
    <a href="https://github.com/BornToBeRoot/NETworkManager/issues/new?labels=Issue&template=Bug_report.md">
      <img alt="Bug report" src="https://img.shields.io/badge/github-bug_report-red.svg?style=for-the-badge&logo=github" />
    </a>     
  </p>
</div>

# More informations
- [Download](https://borntoberoot.net/NETworkManager/Download)
- [Changelog](https://borntoberoot.net/NETworkManager/Changelog)
- [Documentation](https://borntoberoot.net/NETworkManager/Documentation/Application)
- [How to contribute, add a translation, write documentation or report a bug?](https://github.com/BornToBeRoot/NETworkManager/blob/master/CONTRIBUTING.md)
- [List of contributors](https://github.com/BornToBeRoot/NETworkManager/blob/master/Contributors.md)
- [How to report a security vulnerability?](https://github.com/BornToBeRoot/NETworkManager/blob/master/SECURITY.md)

# Build
__Requirements__
- [SDK .NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
- Visual Studio 2019 or later with `.NET desktop development` and `Universal Windows Platform development` 

__Optional__
- [InnoSetup](https://jrsoftware.org/isinfo.php) (if you want to create an installer)
  - Download `ChineseSimplified.isl` and `ChineseTraditional.isl` from the [official repo](https://github.com/jrsoftware/issrc/blob/main/Files/Languages/Unofficial/) and place them in the language folder of InnoSetup

__Build__
1. Clone or download the repository: `git clone https://github.com/BornToBeRoot/NETworkManager`
2. Run the `.\build.ps1` script with PowerShell to compile the solution and create a portable and a setup version (or open the file `Source/NETworkManager.sln` in Visual Studio to debug or build the solution)

You can also copy & paste this command in your PowerShell console :smile:
```PowerShell
git clone https://github.com/BornToBeRoot/NETworkManager; Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass; .\NETworkManager\build.ps1
```

# Code of Conduct
This project has adopted the [code of conduct](https://github.com/BornToBeRoot/NETworkManager/blob/master/CODE_OF_CONDUCT.md) defined by the [Contributor Covenant](http://contributor-covenant.org/).

# License
NETworkManager is published under the [GNU General Public License v3](https://github.com/BornToBeRoot/NETworkManager/blob/master/LICENSE). The licenses of the used libraries can be found [here](https://github.com/BornToBeRoot/NETworkManager/tree/master/Source/NETworkManager.Documentation/Licenses).
