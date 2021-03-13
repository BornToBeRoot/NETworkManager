---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2021.3.14.0** <br>
Date: **14.03.2021**

| File | Checksum [SHA256] |
|---|---|
|[:package: Setup](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.2.17.0/NETworkManager_2021.2.17.0_Setup.exe){:target="_blank"}| `B87F9E4AC704F0FF55A7A41DEA27D9A47CA160D64179A6F1FC3638A1DAFEAD2B` |
|[:package: Portable](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.2.17.0/NETworkManager_2021.2.17.0_Portable.zip){:target="_blank"}| `92B362B50ACC2685235DEB4002C75C6005FA2978E2B3F25B297854504A8E22F7` |
|[:package: Archiv](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.2.17.0/NETworkManager_2021.2.17.0_Archiv.zip){:target="_blank"}| `451A24B0AC8C2E63A15F5370EC2FC11E3957086BA272BDD17B00EB1D03E8E342` |

The setup is also available on [Chocolatey](https://chocolatey.org/packages/NETworkManager){:target="_blank"} and can be installed with:
```
~# choco install networkmanager
```

_If you had version 2020.12.2 or earlier installed, you have to uninstall the x86 version manually in the control panel!_

## System requirements
- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 5.0.0](https://dotnet.microsoft.com/download/dotnet/5.0){:target="_blank"} or later
- [Microsoft Edge - WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="_blank"}

## Silent install
The setup is created with [InnoSetup](https://jrsoftware.org/isinfo.php){:target="_blank"} and you can use all available [command line parameters](https://jrsoftware.org/ishelp/index.php?topic=setupcmdline){:target="_blank"}. Use the following parameters to perform a silent installation and create a desktop icon:

```
NETworkManager_20xx.xx.x_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-
```
