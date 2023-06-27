---
layout: default
title: Download
nav_order: 3
description: "Download the latest available version"
permalink: /Download
---

# 2023.6.27.0

Release date: **27.06.2023**

<br />
<div style="text-align: center">  
  <p>
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.6.27.0/NETworkManager_2023.6.27.0_Setup.exe' style="text-decoration: none;" target="_blank">
      <span class="fs-5"><button type="button" name="button" class="btn btn-primary" style="width: 9.375rem;">:package: Setup</button></span>
    </a>
    &nbsp;
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.6.27.0/NETworkManager_2023.6.27.0_Portable.zip' style="text-decoration: none;" target="_blank">
      <span class="fs-5"><button type="button" name="button" class="btn btn-primary" style="width: 9.375rem;">:package: Portable</button></span>
    </a>
    &nbsp;
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.6.27.0/NETworkManager_2023.6.27.0_Archive.zip' style="text-decoration: none;" target="_blank">
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
      <td><code>664A6BA437C27415E1ED69A57CA4D4435C8EF077C1556ABC9267517A93D48C08</code></td>
    </tr>
    <tr>
      <td>Portable</td>
      <td><code>D356874DD353864C164B2136B893A8B798A8C236990E44F3F222EF1260FDF040</code></td>
    </tr>
    <tr>
      <td>Archive</td>
      <td><code>2FC80F329A5B8C9B2EA79A0C11619EB32383802F180C7CF0FA720B4064B6E0B3</code></td>
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
