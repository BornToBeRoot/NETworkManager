---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2021.3.28.0** <br>
Date: **28.03.2021**

| File | Checksum [SHA256] |
|---|---|
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.3.28.0/NETworkManager_2021.3.28.0_Setup.exe' target='_blank'><button type="button" name="button" class="btn">:cd: Setup</button></a> | `3153DE224DA69511331C07B89CF4738914BDBE9AFEE59C5BD289E657449CCC43` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.3.28.0/NETworkManager_2021.3.28.0_Portable.zip' target='_blank'><button type="button" name="button" class="btn">Portable</button></a> |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.3.28.0/NETworkManager_2021.3.28.0_Archiv.zip' target='_blank'><button type="button" name="button" class="btn">Archiv</button></a> | `6C5E003DC820B75B2B7C1381F2AE2F0D3AEF13EE84F67AE6EFBC235342B94490` |

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
