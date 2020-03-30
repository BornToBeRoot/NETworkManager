## Welcome

NETworkManager is a powerful tool for managing networks and troubleshoot network problems! You can view and configure network interfaces, scan for wlan networks, capture lldp or cdp packages, perform an IP or port scan, ping your hosts, and troubleshoot your connection using traceroute or a DNS lookup.

It contains a remote desktop and PowerShell for managing Windows devices. You can use PuTTY, TigerVNC or the web console to administrate Linux or other devices (e.g. switches). And best of all... everything has tabs and you can create a profile for each of your hosts that can be used in all features.

There are some more features such as a subnet calculator, a Whois lookup or a database in which you can find MAC address vendors or TCP / UDP ports with a description. It's best to take a look at the program yourself.

<img alt="NETworkManager" src="NETworkManager_Preview.gif" />

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
    <a href="https://ci.appveyor.com/project/BornToBeRoot/NETworkManager/branch/master">
      <img alt="AppVeyor" src="https://img.shields.io/appveyor/ci/BornToBeRoot/NETworkManager/master.svg?style=flat-square&&label=master" />
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
    <a href="https://twitter.com/intent/tweet?text=NETworkManager%20-%20A%20powerful%20tool%20for%20managing%20networks%20and%20troubleshoot network problems!&url=https%3A%2F%2Fgithub.com%2FBornToBeRoot%2FNETworkManager&hashtags=networkmanager,ipscanner,portscanner,ssh,vnc,remotedesktop,dns,traceroute,pingmonitor" target="_blank">
     <img alt="Transifex" src="https://img.shields.io/badge/twitter-tweet-blue.svg?style=flat-square" />     
    </a>          
  </p> 
</div>

## Download

| File | Checksum | Description
|---|---|---|
|[:package:&nbsp;Setup](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.1.0/NETworkManager_2020.1.0_Setup.exe)| `A7BD0182269F012701D56285141A6627` `9F41145F748539C7233C3129BE3765CB` | Installs the software in `%ProgramFiles(x86)%`. Settings are saved in `%AppData%`. See [available parameters](http://www.jrsoftware.org/ishelp/index.php?topic=setupcmdline). |
|[:package:&nbsp;Portable](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.1.0/NETworkManager_2020.1.0_Portable.zip)| `BEA66D1B8E1DE820B6077FD1F98ABDF5` `BDD4D7CD0477FC27941EFED326DCCEAD` | Portable version for USB-Stick/Cloud. Settings are saved in the program folder. |
|[:package:&nbsp;Archiv](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.1.0/NETworkManager_2020.1.0_Archiv.zip)| `497C6DEFAD22B074B0E8D0E439485451` `28503512434C096D221D2978B1344F91` | Binaries can be deployed anywhere (network share, etc.). Settings are saved in %AppData%. |

The Setup is also available via [:link:&nbsp;Chocolatey](https://chocolatey.org/packages/NETworkManager){:target="_blank"}.

## Features

- Dashboard 
- Network Interface - Information, Bandwidth, Configure
- WLAN - Networks, Channels
- IP Scanner
- Port Scanner
- Ping
- Ping Monitor
- Traceroute
- DNS Lookup
- Remote Desktop
- PowerShell
- PuTTY ([requires PuTTY](https://www.chiark.greenend.org.uk/~sgtatham/putty/latest.html){:target="_blank"})
- TigerVNC ([requires TigerVNC](https://tigervnc.org/){:target="_blank"})
- Web Console
- SNMP - Get, Walk, Set
- Discovery Protocol - LLDP, CDP
- Wake on LAN
- HTTP Headers
- Whois
- Subnet Calculator - Calculator, Subnetting, Supernetting
- Lookup - OUI, Port
- Connections
- Listeners
- ARP Table

## Changelog

[:page_facing_up: Changelog](Changelog.md)

## Documentation

[:book: Documentation](Documentation/README.md)

## Languages

Official:
- English
- German
- Russian

Community:
- Chinese
- Brazilian Portuguese
- Dutch
- French
- Italy
- Spanish

Help translate on [Transifex](https://www.transifex.com/BornToBeRoot/NETworkManager){:target="_blank"}.

## System requirements

- Windows 10 Build 1809 or later
- .NET-Framework 4.7.2

[Last release for Windows 7 / 8.1 and Server 2008 R2 / 2012 R2 / 2016](https://github.com/BornToBeRoot/NETworkManager/releases/tag/v1.11.0.0){:target="_blank"}

## License

This software is published under the [GNU General Public License v3](https://github.com/BornToBeRoot/NETworkManager/blob/master/LICENSE){:target="_blank"}.

The licenses of the libraries, which are used in the program, can be found [here](https://github.com/BornToBeRoot/NETworkManager/tree/master/Source/NETworkManager/Licenses){:target="_blank"}.
