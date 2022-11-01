---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2022.10.31.0** <br>
Date: **31.10.2022**

| Download                                                                                                                                                                                                             | Checksum [SHA256]                                                  |
| -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.10.31.0/NETworkManager_2022.10.31.0_Setup.exe' target='_blank'><button type="button" name="button" class="btn">Setup</button></a>       | `18A9528247BFA1880AF61CEAA1401FCA7C8271BC6635B40BA72D77AB065867A0` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.10.31.0/NETworkManager_2022.10.31.0_Portable.zip' target='_blank'><button type="button" name="button" class="btn">Portable</button></a> | `7B5822191C1D8EB0DE8F223AA1AC581CFEB4ED726F652A3A8A7D0941A51BE68F` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.10.31.0/NETworkManager_2022.10.31.0_Archive.zip' target='_blank'><button type="button" name="button" class="btn">Archive</button></a>   | `4F1B60B42A1D935B56C0F044755DD5B3EB3DAE35C75961D0CD7FFFFB87596847` |

## System requirements

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="\_blank"}
- [Microsoft Edge - WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="\_blank"}

## Package Manager

It is also available via the package managers [Chocolatey](https://chocolatey.org/packages/NETworkManager){:target="\_blank"} and [WinGet](https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/BornToBeRoot/NETworkManager/){:target="\_blank"}:

```
# Chocolatey
choco install networkmanager

# WinGet
winget install BornToBeRoot.NETworkManager
```

The latest version can also be viewed and downloaded via the PowerShell module [Evergreen](https://github.com/aaronparker/evergreen){:target="\_blank"}:

```
# Evergreen
Get-EvergreenApp -Name NETworkManager

Get-EvergreenApp -Name NETworkManager | Save-EvergreenApp -Path C:\Users\$env:Username\Downloads\
```

## Silent install

The setup is created with [InnoSetup](https://jrsoftware.org/isinfo.php){:target="\_blank"} and you can use all available [command line parameters](https://jrsoftware.org/ishelp/index.php?topic=setupcmdline){:target="\_blank"}. Use the following parameters to perform a silent installation and create a desktop icon:

```
.\NETworkManager_20xx.xx.x_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-
```
