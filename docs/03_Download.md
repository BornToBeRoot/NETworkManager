---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2021.11.30.0** <br>
Date: **30.11.2021**

| Download | Checksum [SHA256] |
|---|---|
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.2.22.0/NETworkManager_2022.2.22.0_Setup.exe' target='_blank'><button type="button" name="button" class="btn btn-blue">Setup</button></a> | `F0CF905B14622588E658977BC2E7B85042858FA4191CDAEF4F84C1BAE2DC9311` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.2.22.0/NETworkManager_2022.2.22.0_Portable.zip' target='_blank'><button type="button" name="button" class="btn btn-blue">Portable</button></a> | `241F02978697BAF2E8BD695671D5197C081CB44C5D52925BC30CC3CECF0A4592` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.2.22.0/NETworkManager_2022.2.22.0_Archive.zip' target='_blank'><button type="button" name="button" class="btn btn-blue">Archive</button></a> | `DC5FC8FF4FC61E21F9D795B39E28C8CCDEC2FD4F01323031F27D691A93563CF8` |

:warning: [BREAKING CHANGE] :warning:
**Read this before upgrading from 2021.11.30.0 and earlier to 2022.2.22.0 or later!**

The profile (file) has been updated and needs to be migrated:
1. The encryption of the profile file(s) must be disabled in the currently installed version.
2. Then the update can be installed as usual.
3. At the first startup the loading of the profiles fails and a PowerShell script can be started to migrate the profiles.
4. After the migration the encryption of the profiles can be enabled again.

Note: _Steps 1 and 4 can be skipped if the profiles are not encrypted._

## System requirements
- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="_blank"}
- [Microsoft Edge - WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="_blank"}

## Package Manager
It is also available via the package managers [Chocolatey](https://chocolatey.org/packages/NETworkManager){:target="_blank"} and [WinGet](https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/BornToBeRoot/NETworkManager/){:target="_blank"}:

```
choco install networkmanager
# or
winget install BornToBeRoot.NETworkManager
```

The latest version can also be viewed and downloaded via the PowerShell module [Evergreen](https://github.com/aaronparker/evergreen){:target="_blank"}:

```
Get-EvergreenApp -Name NETworkManager
# or
Get-EvergreenApp -Name NETworkManager | Save-EvergreenApp -Path C:\Users\$env:Username\Downloads\
```

## Silent install
The setup is created with [InnoSetup](https://jrsoftware.org/isinfo.php){:target="_blank"} and you can use all available [command line parameters](https://jrsoftware.org/ishelp/index.php?topic=setupcmdline){:target="_blank"}. Use the following parameters to perform a silent installation and create a desktop icon:

```
NETworkManager_20xx.xx.x_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-
```
