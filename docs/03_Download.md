---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2021.6.26.0** <br>
Date: **26.06.2021**

| Download | Checksum [SHA256] |
|---|---|
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.6.26.0/NETworkManager_2021.6.26.0_Setup.exe' target='_blank'><button type="button" name="button" class="btn">Setup</button></a> | `EAA8D1CC1F76FABD0C26DF70B9E23A21F9F83355D70D0FCCD43E5677A1AE7EAE` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.6.26.0/NETworkManager_2021.6.26.0_Portable.zip' target='_blank'><button type="button" name="button" class="btn">Portable</button></a> | `02A98625966C6229E4EC23463B3B7607365668CC1B63F47D563C26FF764710CE` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.6.26.0/NETworkManager_2021.6.26.0_Archiv.zip' target='_blank'><button type="button" name="button" class="btn">Archive</button></a> | `3C23B74EEBC8CF113D8F536F6D9D2BBC6CB3CCF1BABEF0275B9101D84A45ADDB` |


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
