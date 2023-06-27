---
layout: default
title: Download
nav_order: 3
description: "Download the latest available version"
permalink: /Download
---

# 2023.6.27.1

Release date: **27.06.2023**

<br />
<div style="text-align: center">  
  <p>
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.6.27.1/NETworkManager_2023.6.27.1_Setup.exe' style="text-decoration: none;" target="_blank">
      <span class="fs-5"><button type="button" name="button" class="btn btn-primary" style="width: 9.375rem;">:package: Setup</button></span>
    </a>
    &nbsp;
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.6.27.1/NETworkManager_2023.6.27.1_Portable.zip' style="text-decoration: none;" target="_blank">
      <span class="fs-5"><button type="button" name="button" class="btn btn-primary" style="width: 9.375rem;">:package: Portable</button></span>
    </a>
    &nbsp;
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.6.27.1/NETworkManager_2023.6.27.1_Archive.zip' style="text-decoration: none;" target="_blank">
      <span class="fs-5"><button type="button" name="button" class="btn btn-primary" style="width: 9.375rem;">:package: Archive</button></span>
    </a>
  </p>  
  <a href='https://github.com/BornToBeRoot/NETworkManager/releases' target="_blank">>> Pre-release versions are available here <<</a>
</div>
<br />
<details>
  <summary>Hashes</summary>
  <table>
    <tr>
      <td style="text-align: center;"><b>File</b></td>
      <td style="text-align: center;"><b>Checksum</b> <code>SHA256</code></td>
    </tr>
    <tr>
      <td>Setup</td>
      <td><code>8126ED2D73CCF7E6F1C5EA5FD13A25C99DE1493F05A71C1435A956DA4409C836</code></td>
    </tr>
    <tr>
      <td>Portable</td>
      <td><code>49AFB59B249F760E58B20AB779D47E7FE9E39A082EB6F10601A5EF1A0D40A6B8</code></td>
    </tr>
    <tr>
      <td>Archive</td>
      <td><code>A9789E3BC8417875565385A4D93A8D20BD0A793EA1BAE390232B85B4BF51123D</code></td>
    </tr>
  </table>
</details>

## System requirements

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="\_blank"}

## Package Manager

NETworkManager is available through the package managers [Chocolatey](https://chocolatey.org/packages/NETworkManager){:target="\_blank"} and [WinGet](https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/BornToBeRoot/NETworkManager/){:target="\_blank"} and the PowerShell module [Evergreen](https://github.com/aaronparker/evergreen){:target="\_blank"}:

```
# Chocolatey
choco install networkmanager

# WinGet
winget install BornToBeRoot.NETworkManager

# Evergreen
Get-EvergreenApp -Name NETworkManager

# Evergreen (save to disk)
Get-EvergreenApp -Name NETworkManager | Save-EvergreenApp -Path C:\Users\$env:Username\Downloads\
```

## Silent install

The Setup is based on [InnoSetup](https://jrsoftware.org/isinfo.php){:target="\_blank"} and you can use all available [command line parameters](https://jrsoftware.org/ishelp/index.php?topic=setupcmdline){:target="\_blank"}. Use the following parameters to perform a silent installation and create a desktop icon:

```
.\NETworkManager_{VERSION}_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-
```
