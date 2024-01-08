<div align="center">
  <img alt="NETworkManager Preview" src="https://github.com/BornToBeRoot/NETworkManager/blob/main/Images/NETworkManager_128x128.png?raw=true" />
  <h1>NETworkManager</h1>
  <p>
    <b>A powerful open source tool for managing networks and troubleshooting network problems!</b>
  </p>
  <p>
    Connect and manage remote systems with Remote Desktop, PowerShell, PuTTY, TigerVNC or AWS (Systems Manager) Session Manager. Analyze and troubleshoot your network and systems with features such as the WiFi Analyzer, IP Scanner, Port Scanner, Ping Monitor, Traceroute, DNS lookup or LLDP/CDP capture (and many <a href="https://borntoberoot.net/NETworkManager/docs/features">more</a>) in a unfied interface. Hosts (or networks) can be saved in (encrypted) profiles and used across all features.  
  <p>
     <a href="https://borntoberoot.net/NETworkManager/download" target="_blank">
      <img alt="All releases" src="https://img.shields.io/badge/>>_download_now_<<-00abbd?style=for-the-badge" height="48" />
    </a>
  </p>
  <p>
    <a href="https://github.com/BornToBeroot/NETworkManager/releases" target="_blank">
      <img alt="All releases" src="https://img.shields.io/github/downloads/BornToBeroot/NETworkManager/total.svg?style=for-the-badge&logo=github" />
    </a>    
    <a href="https://github.com/BornToBeroot/NETworkManager/releases/latest" target="_blank">
      <img alt="Latest release" src="https://img.shields.io/github/downloads/BornToBeroot/NETworkManager/latest/total.svg?style=for-the-badge&logo=github" />
    </a>
    <a href="https://github.com/BornToBeroot/NETworkManager/releases" target="_blank">
      <img alt="Latest pre-release" src="https://img.shields.io/github/downloads-pre/BornToBeroot/NETworkManager/latest/total.svg?label=downloads%40pre-release&style=for-the-badge&logo=github" />
    </a>
  </p>
  <p>
    <a href="https://github.com/BornToBeroot/NETworkManager/stargazers" target="_blank">
      <img alt="GitHub stars" src="https://img.shields.io/github/stars/BornToBeroot/NETworkManager.svg?style=for-the-badge&logo=github" />
    </a>    
    <a href="https://github.com/BornToBeroot/NETworkManager/network" target="_blank">       
      <img alt="GitHub forks" src="https://img.shields.io/github/forks/BornToBeroot/NETworkManager.svg?style=for-the-badge&logo=github" />
    </a>
  </p>
  <p> 
    <a href="https://ci.appveyor.com/project/BornToBeRoot/NETworkManager/branch/main">
      <img alt="AppVeyor" src="https://img.shields.io/appveyor/ci/BornToBeRoot/NETworkManager/main.svg?style=for-the-badge&logo=appveyor&&label=main" />
    </a>   
    <a href="https://github.com/BornToBeRoot/NETworkManager/blob/main/LICENSE">
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
  <p>
    <a href="#-download">Download</a> • <a href="#-changelog">Changelog</a> • <a href="#-documentation">Documentation</a> • <a href="#-contributing">Contributing</a> • <a href="#-build">Build</a> • <a href="#-license">License</a>
  </p>
</div>

<img alt="NETworkManager Preview" src="https://github.com/BornToBeRoot/NETworkManager/blob/main/docs/Preview.gif?raw=true" />

## 📦 Download

Prebuilt binaries (setup, portable and archive) are available on the [download page](https://borntoberoot.net/NETworkManager/Download). Here you will also find the system requirements and how to install the application silent. The files are provided via [GitHub releases](https://github.com/BornToBeRoot/NETworkManager/releases/latest).

In addition, NETworkManager is available through the package managers [Chocolatey](https://chocolatey.org/packages/NETworkManager), [WinGet](https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/BornToBeRoot/NETworkManager/) and [Evergreen](https://stealthpuppy.com/evergreen/apps/):

```PowerShell
# Chocolatey
choco install networkmanager

# WinGet
winget install BornToBeRoot.NETworkManager

# Evergreen
Get-EvergreenApp -Name NETworkManager | Save-EvergreenApp -Path C:\Users\$env:Username\Downloads\
```

## 📃 Changelog

You can find the changelog for each version [here](https://borntoberoot.net/NETworkManager/Changelog).

## 📖 Documentation

The documentation is provided via GitHub pages and can be found [here](https://borntoberoot.net/NETworkManager/docs/introduction).

## ✨ Contributing

Here you will find ways to contribute:

- [Request a feature, report a bug or ask a question](CONTRIBUTING.md#contributing)
- [Add a feature or fix a bug](CONTRIBUTING.md#code)
- [Add or improve a translation](CONTRIBUTING.md#translation)
- [Improve the documentation](CONTRIBUTING.md#documentation)
- [Report a security vulnerability](https://github.com/BornToBeRoot/NETworkManager/blob/main/SECURITY.md)

A list of all contributors can be found [here](https://github.com/BornToBeRoot/NETworkManager/blob/main/Contributors.md).

This project has adopted the [code of conduct](https://github.com/BornToBeRoot/NETworkManager/blob/main/CODE_OF_CONDUCT.md) defined by the [Contributor Covenant](https://contributor-covenant.org/).

## 🔧 Build

You can build the application like any other .NET Core / WPF application on Windows.

1. Make sure that the following requirements are installed:

   - [SDK .NET 6.x](https://dotnet.microsoft.com/download/dotnet/6.0)
   - Visual Studio 2019 or later with `.NET desktop development` and `Universal Windows Platform development`

2. (optional) Install [InnoSetup](https://jrsoftware.org/isinfo.php) to create an installer.

   - Download the additional languages from the [Inno Setup repository](https://github.com/jrsoftware/issrc/blob/main/Files/Languages/Unofficial/) and copy the following files to `%ProgramFiles%\Inno Setup 6\Languages`: `ChineseSimplified.isl`, `ChineseTraditional.isl`, `Hungarian.isl`, `Korean.isl`

   > **NOTE**: The languages files must be downloaded or cloned so that the encoding of the file is not changed
   > (e.g. Chinese should be `UTF-8-BOM`).

3. Clone the repository and all submodules:

```PowerShell
# Clone the repository
git clone https://github.com/BornToBeRoot/NETworkManager

# Change directory
cd NETworkManager

# Clone the submodules
git submodule update --init --recursive
```

4. Open the `Source\NETworkManager.sln` with Visual Studio or JetBrains Rider to build (or debug) the solution.

> **NOTE** If you have installed the requirements from step 1 (and optionally step 2), you can also directly build the
> binaries with PowerShell 7 or later:
>
> ```PowerShell
> # Clone the repository
> git clone https://github.com/BornToBeRoot/NETworkManager
>
> # Change directory
> cd NETworkManager
>
> # Clone the submodules
> git submodule update --init --recursive
>
> # Allow the execution of the build script
> Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
>
> # Build the binaries
> & .\build.ps1
> ```

## 📝 License

NETworkManager is published under the [GNU General Public License v3](https://github.com/BornToBeRoot/NETworkManager/blob/main/LICENSE). The licenses of the used libraries can be found [here](https://github.com/BornToBeRoot/NETworkManager/tree/main/Source/NETworkManager.Documentation/Licenses).
