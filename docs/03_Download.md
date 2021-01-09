---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2021.1.0** <br>
Date: **09.01.2021**

| File | Checksum [SHA256] |
|---|---|
|[:package: Setup](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.1.0/NETworkManager_2021.1.0_Setup.exe){:target="_blank"}| `370FDED539C49E044209CEB6897AD76DD1E947754C0E2742FEED0658229BD3F3` |
|[:package: Portable](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.1.0/NETworkManager_2021.1.0_Portable.zip){:target="_blank"}| `BD6507198CC7EC5974229A58F9CB33F13CE0A33ABC2E20344DB42321C2382977` |
|[:package: Archiv](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.1.0/NETworkManager_2021.1.0_Archiv.zip){:target="_blank"}| `425517A996CE52BB2ADFF05D02537817C954329402AAE9F05773B36C61997E71` |

The setup is also available on [Chocolatey](https://chocolatey.org/packages/NETworkManager){:target="_blank"} and can be installed with:
```
~# choco install networkmanager
```

_Migrated to x64 (The x86 version has to be uninstalled manually)_

## System requirements
- Windows 10 / Server (1809 or later)
- [.NET Desktop Runtime 5.0.0](https://dotnet.microsoft.com/download/dotnet/5.0){:target="_blank"} or later
- [Microsoft Edge - WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="_blank"}

## Silent install
The setup is created with [InnoSetup](https://jrsoftware.org/isinfo.php){:target="_blank"} and you can use all available [command line parameters](https://jrsoftware.org/ishelp/index.php?topic=setupcmdline){:target="_blank"}. Use the following parameters to perform a silent installation and create a desktop icon:

```
NETworkManager_20xx.xx.x_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-
```
