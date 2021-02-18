---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2021.2.17.0** <br>
Date: **17.02.2021**

| File | Checksum [SHA256] |
|---|---|
|[:package: Setup](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.2.17.0/NETworkManager_2021.2.17.0_Setup.exe){:target="_blank"}| `5BDD82CA59CC0A635559848CA183C21ED92D4D0C143625F1B367946F033E2A79` |
|[:package: Portable](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.2.17.0/NETworkManager_2021.2.17.0_Portable.zip){:target="_blank"}| `4D5AA9CC1A4FCE5AB80060A1009E87FA207D642B7E25A63A4DA38F9F6FD43472` |
|[:package: Archiv](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.2.17.0/NETworkManager_2021.2.17.0_Archiv.zip){:target="_blank"}| `9996197F0F58A7A23F0A78DB7336D4201F98F1C6E8CCD7D02BF97095C5A6DDA8` |

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
