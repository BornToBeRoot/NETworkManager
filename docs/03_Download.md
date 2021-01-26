---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2021.1.2** <br>
Date: **26.01.2021**

| File | Checksum [SHA256] |
|---|---|
|[:package: Setup](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.1.2/NETworkManager_2021.1.2_Setup.exe){:target="_blank"}| `600C5F6F57ECE66E87FD9AC5FA8024A2CE7DF156D1FA7D5D17C1809E89CC0548` |
|[:package: Portable](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.1.2/NETworkManager_2021.1.2_Portable.zip){:target="_blank"}| `C8CFE51FD787D56A7A4DF07EC9A1EF7F043D296E2E2F9EF2CB071D999E9DB3CD` |
|[:package: Archiv](https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.1.2/NETworkManager_2021.1.2_Archiv.zip){:target="_blank"}| `5705DEFD5457625B85B3F7FD5FC64A503E231BBFC29B6BF749DB2858B14E53D3` |

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
