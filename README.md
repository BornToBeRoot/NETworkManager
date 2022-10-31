<div align="center">
  <img alt="NETworkManager Preview" src="https://github.com/BornToBeRoot/NETworkManager/blob/main/Images/NETworkManager_128x128.png?raw=true" />
  <h1>NETworkManager</h1>
  <h3>A powerful tool for managing networks and troubleshoot network problems!</h3> 
  <br />
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
</div>

[Features](#features)
•
[Download](#download)
•
[Changelog](#changelog)
•
[Documentation](#documentation)
•
[Contributing](#contributing)
•
[Build](#build)
•
[License](#license)
•
[Code of Conduct](#code-of-conduct)

<img alt="NETworkManager Preview" src="https://github.com/BornToBeRoot/NETworkManager/blob/main/docs/Preview.gif?raw=true" />

## Features

- [Dashboard](Documentation/Application/Dashboard)
- [Network Interface](Documentation/Application/NetworkInterface) - Information, Bandwidth, Configure
- [WiFi](Documentation/Application/WiFi) - Networks, Channels
- [IP Scanner](Documentation/Application/IPScanner)
- [Port Scanner](Documentation/Application/PortScanner)
- [Ping Monitor](Documentation/Application/PingMonitor)
- [Traceroute](Documentation/Application/Traceroute)
- [DNS Lookup](Documentation/Application/DNSLookup)
- [Remote Desktop](Documentation/Application/RemoteDesktop)
- [PowerShell](Documentation/Application/PowerShell)
- [PuTTY](Documentation/Application/PuTTY) (requires [PuTTY](https://www.chiark.greenend.org.uk/~sgtatham/putty/latest.html){:target="\_blank"})
- [AWS Session Manager](Documentation/Application/AWSSessionManager) (requires [AWS CLI](https://aws.amazon.com/cli/){:target="\_blank"} and [AWS Session Manager plugin](https://docs.aws.amazon.com/systems-manager/latest/userguide/session-manager-working-with-install-plugin.html){:target="\_blank"})
- [TigerVNC](Documentation/Application/TigerVNC) (requires [TigerVNC](https://tigervnc.org/){:target="\_blank"})
- [Web Console](Documentation/Application/WebConsole) (requires [Microsoft Edge - WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="\_blank"})
- [SNMP](Documentation/Application/SNMP) - Get, Walk, Set
- [Discovery Protocol](Documentation/Application/DiscoveryProtocol) - LLDP, CDP
- [Wake on LAN](Documentation/Application/WakeOnLAN)
- [Whois](Documentation/Application/Whois)
- [Subnet Calculator](Documentation/Application/SubnetCalculator) - Calculator, Subnetting, Supernetting
- [Lookup](Documentation/Application/Lookup) - OUI, Port
- [Connections](Documentation/Application/Connections)
- [Listeners](Documentation/Application/Listeners)
- [ARP Table](Documentation/Application/ARPTable)

## Download

- [Download](https://borntoberoot.net/NETworkManager/Download)

## Changelog

- [Changelog](https://borntoberoot.net/NETworkManager/Changelog)

## Documentation

- [Documentation](https://borntoberoot.net/NETworkManager/Documentation/Application)

## Contributing

- [How to contribute, add a translation, write documentation or report a bug?](https://github.com/BornToBeRoot/NETworkManager/blob/main/CONTRIBUTING.md)
- [List of contributors](https://github.com/BornToBeRoot/NETworkManager/blob/main/Contributors.md)
- [How to report a security vulnerability?](https://github.com/BornToBeRoot/NETworkManager/blob/main/SECURITY.md)

## Build

**Requirements**

- [SDK .NET 6.x](https://dotnet.microsoft.com/download/dotnet/6.0)
- Visual Studio 2019 or later with `.NET desktop development` and `Universal Windows Platform development`

**Optional**

- [InnoSetup](https://jrsoftware.org/isinfo.php) (if you want to create an installer)
  - Download `ChineseSimplified.isl` and `ChineseTraditional.isl` from the [official repo](https://github.com/jrsoftware/issrc/blob/main/Files/Languages/Unofficial/) and place them in the language folder of InnoSetup

**Build**

1. Clone or download the repository: `git clone https://github.com/BornToBeRoot/NETworkManager`
2. Run the `.\build.ps1` script with PowerShell to compile the solution and create a portable and a setup version (or open the file `Source/NETworkManager.sln` in Visual Studio to debug or build the solution)

You can also copy & paste this command in your PowerShell console :smile:

```PowerShell
git clone https://github.com/BornToBeRoot/NETworkManager; Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass; .\NETworkManager\build.ps1
```

## Code of Conduct

This project has adopted the [code of conduct](https://github.com/BornToBeRoot/NETworkManager/blob/main/CODE_OF_CONDUCT.md) defined by the [Contributor Covenant](http://contributor-covenant.org/).

## License

NETworkManager is published under the [GNU General Public License v3](https://github.com/BornToBeRoot/NETworkManager/blob/main/LICENSE). The licenses of the used libraries can be found [here](https://github.com/BornToBeRoot/NETworkManager/tree/main/Source/NETworkManager.Documentation/Licenses).
