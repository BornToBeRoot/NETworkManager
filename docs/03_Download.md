---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2020.12.2** <br>
Date: **28.12.2020**

| File | Checksum [SHA256] |
|---|---|
|[:package: Setup](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.12.2/NETworkManager_2020.12.2_Setup.exe){:target="_blank"}| `4F0EA9AB5969021901AA107A40B0F1C649AB2A39FFC55565DDEF318D983021F9` | 
|[:package: Portable](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.12.2/NETworkManager_2020.12.2_Portable.zip){:target="_blank"}| `7A26775C906586822AE3585CDD2FC3AD361D359D38AC591012E4CB9F82EEA8BA` | 
|[:package: Archiv](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.12.2/NETworkManager_2020.12.2_Archiv.zip){:target="_blank"}| `6094B2C27A279C004049D0AD083C8A23A57864FBD61616DECD2B75A9C3ABDD95` | 

The setup is also available on [Chocolatey](https://chocolatey.org/packages/NETworkManager){:target="_blank"} and can be installed with:
```
~# choco install networkmanager
```

## System requirements
- Windows 10 / Server (1809 or later)
- [.NET Desktop Runtime 5.0.0](https://dotnet.microsoft.com/download/dotnet/5.0){:target="_blank"} or later
- [Microsoft Edge - WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="_blank"}

## Silent install
The setup is created with [InnoSetup](https://jrsoftware.org/isinfo.php){:target="_blank"} and you can use all available [command line parameters](https://jrsoftware.org/ishelp/index.php?topic=setupcmdline){:target="_blank"}. Use the following parameters to perform a silent installation and create a desktop icon:

```
NETworkManager_20xx.xx.x_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-
```
