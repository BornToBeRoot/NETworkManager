---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2021.11.30.0** <br>
Date: **30.11.2021**

| Download | Checksum [SHA256] |
|---|---|
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.11.30.0/NETworkManager_2021.11.30.0_Setup.exe' target='_blank'><button type="button" name="button" class="btn">Setup</button></a> | `AFDA99002A95D78127A26ADCED00F1A86DCBDB6DA0782A761088FFBF3BFF1DF9` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.11.30.0/NETworkManager_2021.11.30.0_Portable.zip' target='_blank'><button type="button" name="button" class="btn">Portable</button></a> | `6EE94EDF8EEDE8D65E2143D5BF60C59CA715536D4C15E680FFB0454174D22FE1` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.11.30.0/NETworkManager_2021.11.30.0_Archiv.zip' target='_blank'><button type="button" name="button" class="btn">Archive</button></a> | `5F72238031B7DB28F885BD8E74046734BDBCA37E4DFC5110ADB75EB216C50DF1` |

## System requirements
- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="_blank"}
- [Microsoft Edge - WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="_blank"}

# Package Manager
It is also available via the package managers [Chocolatey](https://chocolatey.org/packages/NETworkManager){:target="_blank"} and [WinGet](https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/BornToBeRoot/NETworkManager/){:target="_blank"}:

```PowerShell
choco install networkmanager
# or
winget install BornToBeRoot.NETworkManager
```

The latest version can also be viewed and downloaded via the PowerShell module [Evergreen](https://github.com/aaronparker/evergreen){:target="_blank"}:

```PowerShell
Get-EvergreenApp -Name NETworkManager
# or
Get-EvergreenApp -Name NETworkManager | Save-EvergreenApp -Path C:\Users\xxx\Downloads\
```

## Silent install
The setup is created with [InnoSetup](https://jrsoftware.org/isinfo.php){:target="_blank"} and you can use all available [command line parameters](https://jrsoftware.org/ishelp/index.php?topic=setupcmdline){:target="_blank"}. Use the following parameters to perform a silent installation and create a desktop icon:

```
NETworkManager_20xx.xx.x_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-
```
