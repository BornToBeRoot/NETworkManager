---
layout: default
title: Download
nav_order: 3
description: "Download of the latest available version"
permalink: /Download
---

# Download

## 2022.12.20.0

<div align="center">
  <br />
  <p>
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.12.20.0/NETworkManager_2022.12.20.0_Setup.exe' style="text-decoration: none;" target="_blank">
      <span class="fs-5">
        <button type="button" name="button" class="btn">:package: Setup</button>
      </span>
    </a>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;    
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.12.20.0/NETworkManager_2022.12.20.0_Portable.zip' style="text-decoration: none;" target="_blank">
      <span class="fs-5">
        <button type="button" name="button" class="btn">:package: Portable</button>
      </span>
    </a>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.12.20.0/NETworkManager_2022.12.20.0_Archive.zip' style="text-decoration: none;" target="_blank">
      <span class="fs-5">
        <button type="button" name="button" class="btn">:package: Archive</button>
      </span>
    </a> 
  </p>
  <br />
</div>

{: .note }
If you don't see the new features like [AWS Session Manager](./Documentation/Application/AWSSessionManager) or [Bit Calculator](./Documentation/Application/BitCalculator), you need to reset the settings. You can do this under [`Settings > Settings > Reset`](Documentation/Settings/Settings#reset) or by starting the application with the following parameter [`NETworkManager.exe --reset-settings`](./Documentation/CommandLineArguments#--reset-settings). The profiles are not affected when the settings are reset.

Pre-release versions are available [here](https://github.com/BornToBeRoot/NETworkManager/releases){:target="\_blank"}.

## Checksum

| File                                       | SHA256                                                             |
| ------------------------------------------ | ------------------------------------------------------------------ |
| [`NETworkManager_2022.12.20.0_Setup.exe`](https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.12.20.0/NETworkManager_2022.12.20.0_Setup.exe){:target="\_blank"}    | `36F915D917DF6461CB52C1524F3E7373EC0BC40D3A48E9D28E3D02BAA416E369` |
| [`NETworkManager_2022.12.20.0_Portable.zip`](https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.12.20.0/NETworkManager_2022.12.20.0_Portable.zip){:target="\_blank"} | `5A5553BF3609749C1214B8B724857AE954C4F8881907BD0DE5F1C44E48D2CEFA` |
| [`NETworkManager_2022.12.20.0_Archive.zip`](https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.12.20.0/NETworkManager_2022.12.20.0_Archive.zip){:target="\_blank"}  | `29D6EE0FD1A822E5CF45F0A175A0CECAE22C64F5551CCBB117C5EE76DC07DF61` |

## System requirements

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="\_blank"}

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
