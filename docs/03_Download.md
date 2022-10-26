---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

Version: **2022.8.18.0** <br>
Date: **18.8.2022**

| Download                                                                                                                                                                                                             | Checksum [SHA256]                                                  |
| -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.8.18.0/NETworkManager_2022.8.18.0_Setup.exe' target='_blank'><button type="button" name="button" class="btn">Setup</button></a>       | `F705C66072E98E011CAA1EAC3C8B085DBB58CF9476EF9E50490CE526522BA6AA` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.8.18.0/NETworkManager_2022.8.18.0_Portable.zip' target='_blank'><button type="button" name="button" class="btn">Portable</button></a> | `56CFA317D1123C191C443F7950A0096D0CA1F1A6D93EEB9B6C9128EFF6C3EF07` |
| <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.8.18.0/NETworkManager_2022.8.18.0_Archive.zip' target='_blank'><button type="button" name="button" class="btn">Archive</button></a>   | `1C778ED1EF28A18315B8B5581F3BF9D14F282CE6D4521FE98F0057DAE2AFC30A` |

## System requirements

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="\_blank"}
- [Microsoft Edge - WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="\_blank"}

## Package Manager

It is also available via the package managers [Chocolatey](https://chocolatey.org/packages/NETworkManager){:target="\_blank"} and [WinGet](https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/BornToBeRoot/NETworkManager/){:target="\_blank"}:

```PowerShell
choco install networkmanager
# or
winget install BornToBeRoot.NETworkManager
```

The latest version can also be viewed and downloaded via the PowerShell module [Evergreen](https://github.com/aaronparker/evergreen){:target="\_blank"}:

```PowerShell
Get-EvergreenApp -Name NETworkManager
# or
Get-EvergreenApp -Name NETworkManager | Save-EvergreenApp -Path C:\Users\$env:Username\Downloads\
```

## Silent install

The setup is created with [InnoSetup](https://jrsoftware.org/isinfo.php){:target="\_blank"} and you can use all available [command line parameters](https://jrsoftware.org/ishelp/index.php?topic=setupcmdline){:target="\_blank"}. Use the following parameters to perform a silent installation and create a desktop icon:

```PowerShell
.\NETworkManager_20xx.xx.x_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-
```
