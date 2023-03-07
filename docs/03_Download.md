---
layout: default
title: Download
nav_order: 3
description: "Download the latest available version"
permalink: /Download
---

# Download

Version: **2023.3.7.0** <br />
Release date: **07.03.2023**

<table>
  <tr>
    <td style="text-align: center;">
      <b>File</b>
    </td>
    <td style="text-align: center;">
      <b>Checksum</b> <code>SHA256</code>
    </td>
  </tr>
  <tr>
    <td>
      <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.3.7.0/NETworkManager_2023.3.7.0_Setup.exe' style="text-decoration: none;" target="_blank">
        <span class="fs-5">
          <button type="button" name="button" class="btn btn-primary">:package: Setup</button>
        </span>
      </a>
    </td>
    <td>
      <code>26CCD741CB9F2B52056C91C94DF281C563886AC0E4799A3D167A0566448574F6</code>
    </td>
  </tr>
  <tr>
    <td>
      <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.3.7.0/NETworkManager_2023.3.7.0_Portable.zip' style="text-decoration: none;" target="_blank">
        <span class="fs-5">
          <button type="button" name="button" class="btn btn-primary">:package: Portable</button>
        </span>
      </a>
    </td>
    <td>
      <code>B19782CAD036F1F1A64A204EAEBC762B4B519BB4C1836DCD5405C83F8488EB29</code>
    </td>
  </tr>
  <tr>
    <td>
      <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.3.7.0/NETworkManager_2023.3.7.0_Archive.zip' style="text-decoration: none;" target="_blank">
        <span class="fs-5">
          <button type="button" name="button" class="btn btn-primary">:package: Archive</button>
        </span>
      </a> 
    </td>
    <td>
      <code>7787FE873DD2704487A5A065EA473127C888256CCE3ADA928EEE7CB4B2BF9B6F</code>
    </td>
  </tr>
</table>

{: .note }
If you don't see the new features like [AWS Session Manager](./Documentation/Application/AWSSessionManager) or [Bit Calculator](./Documentation/Application/BitCalculator), you need to reset the settings. You can do this under [`Settings > Settings > Reset`](Documentation/Settings/Settings#reset) or by starting the application with the following parameter [`NETworkManager.exe --reset-settings`](./Documentation/CommandLineArguments#--reset-settings). The profiles are not affected when the settings are reset.

Pre-release versions are available [here](https://github.com/BornToBeRoot/NETworkManager/releases){:target="\_blank"}.

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
.\NETworkManager_{VERSION}_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-
```
