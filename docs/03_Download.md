---
layout: default
title: Download
nav_order: 3
description: "Download the latest available version"
permalink: /Download
---

# Download

Version: **2023.3.19.0** <br />
Release date: **19.03.2023**

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
      <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.3.19.0/NETworkManager_2023.3.19.0_Setup.exe' style="text-decoration: none;" target="_blank">
        <span class="fs-5">
          <button type="button" name="button" class="btn btn-primary">:package: Setup</button>
        </span>
      </a>
    </td>
    <td>
      <code>440287A5B44944D4BF5F4516EB7FEB953FE62605628DFFC8EFE9396791D388E2</code>
    </td>
  </tr>
  <tr>
    <td>
      <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.3.19.0/NETworkManager_2023.3.19.0_Portable.zip' style="text-decoration: none;" target="_blank">
        <span class="fs-5">
          <button type="button" name="button" class="btn btn-primary">:package: Portable</button>
        </span>
      </a>
    </td>
    <td>
      <code>C4077E5B45A792860AE5C4448ED248DEFA8A56D2081ED9949AAFF262EFA2CA4A</code>
    </td>
  </tr>
  <tr>
    <td>
      <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.3.19.0/NETworkManager_2023.3.19.0_Archive.zip' style="text-decoration: none;" target="_blank">
        <span class="fs-5">
          <button type="button" name="button" class="btn btn-primary">:package: Archive</button>
        </span>
      </a> 
    </td>
    <td>
      <code>82983DC69D2AEC39B9A08A65F966150C49F6F30988155000DD80BE117BD2AD1C</code>
    </td>
  </tr>
</table>

{: .note }
If you don't see the new features ([AWS Session Manager](/NETworkManager//Documentation/Application/AWSSessionManager), [Bit Calculator](/NETworkManager//Documentation/Application/BitCalculator) or [SNTP Lookup](/NETworkManager//Documentation/Application/SNTPLookup)), try to reset the settings. You can do this under [`Settings > Settings > Reset`](/NETworkManager//Documentation/Settings/Settings#reset) or by starting the application with the following parameter [`NETworkManager.exe --reset-settings`](/NETworkManager//Documentation/CommandLineArguments#--reset-settings). The profiles are not affected when the settings are reset.

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
