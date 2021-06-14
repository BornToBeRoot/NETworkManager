---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2021.5.9.0** <br>
Date: **09.05.2021**

| Download | Checksum [SHA256] |
|---|---|
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.5.9.0/NETworkManager_2021.5.9.0_Setup.exe' target='_blank'><button type="button" name="button" class="btn">Setup</button></a> | `1D44A5A07975A8364DE742E061E0D72B3DD1CC03E02F4D87103A0767E3C7F7A2` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.5.9.0/NETworkManager_2021.5.9.0_Portable.zip' target='_blank'><button type="button" name="button" class="btn">Portable</button></a> | `ED3DC6DFC111A3261DC6BF3F3D2820E0A1FF96ECF48A3C7DFBF3454A39694848` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.5.9.0/NETworkManager_2021.5.9.0_Archiv.zip' target='_blank'><button type="button" name="button" class="btn">Archive</button></a> | `DD0B800EA55B97BA0A6B3A5884D62FCE2C4E74EAB1CCE01163823EBD30DC9B4E` |

The setup is also available on [Chocolatey](https://chocolatey.org/packages/NETworkManager){:target="_blank"} and [WinGet](https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/BornToBeRoot/NETworkManager/){:target="_blank"}:
```
~# choco install networkmanager
or
~# winget install BornToBeRoot.NETworkManager
```

## System requirements
- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 5.0.0](https://dotnet.microsoft.com/download/dotnet/5.0){:target="_blank"} or later
- [Microsoft Edge - WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="_blank"}

## Silent install
The setup is created with [InnoSetup](https://jrsoftware.org/isinfo.php){:target="_blank"} and you can use all available [command line parameters](https://jrsoftware.org/ishelp/index.php?topic=setupcmdline){:target="_blank"}. Use the following parameters to perform a silent installation and create a desktop icon:

```
NETworkManager_20xx.xx.x_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-
```
