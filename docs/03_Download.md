---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2021.9.5.0** <br>
Date: **05.09.2021**

| Download | Checksum [SHA256] |
|---|---|
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.9.5.0/NETworkManager_2021.9.5.0_Setup.exe' target='_blank'><button type="button" name="button" class="btn">Setup</button></a> | `883188AD58FB936FCA82846A261840F67E19734C20093996CBCDA75F4CE41EAA` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.9.5.0/NETworkManager_2021.9.5.0_Portable.zip' target='_blank'><button type="button" name="button" class="btn">Portable</button></a> | `F186755939FC2A5A07C2D5BDED59128189D16968FD61DF8E17C100E181B3C39D` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.9.5.0/NETworkManager_2021.9.5.0_Archiv.zip' target='_blank'><button type="button" name="button" class="btn">Archive</button></a> | `E9680A1C52270A5B2AB192AADE08563CB3509CDCD3EF3F9BA2603CE3BB84DF45` |


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
