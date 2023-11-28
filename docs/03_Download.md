---
layout: default
title: Download
nav_order: 3
description: "Download the latest available version"
permalink: /Download
---

# 2023.11.28.0

Release date: **11.28.2023**

<br />
<div style="text-align: center">  
  <p>
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.11.28.0/NETworkManager_2023.11.28.0_Setup.exe' style="text-decoration: none;" target="_blank">
      <span class="fs-5"><button type="button" name="button" class="btn btn-primary" style="width: 9.375rem;">:package: Setup</button></span>
    </a>
    &nbsp;
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.11.28.0/NETworkManager_2023.11.28.0_Portable.zip' style="text-decoration: none;" target="_blank">
      <span class="fs-5"><button type="button" name="button" class="btn btn-primary" style="width: 9.375rem;">:package: Portable</button></span>
    </a>
    &nbsp;
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.11.28.0/NETworkManager_2023.11.28.0_Archive.zip' style="text-decoration: none;" target="_blank">
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
      <td><code>AD9D6E569BFA61F9657A6C823409E4D4B4B67CA4BD0CC5129CCBB0B673D5DF24</code></td>
    </tr>
    <tr>
      <td>Portable</td>
      <td><code>8D15ECE18013C07D806173E051FFA79406A6B5D6D00D1CD48F403C8BDBF7136F</code></td>
    </tr>
    <tr>
      <td>Archive</td>
      <td><code>39FAC00FBB16D6EFCC3AB571B42AE61C8789E500059F3ED893CE38BFA06CD189</code></td>
    </tr>
  </table>
</details>

## System requirements

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 8.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime){:target="\_blank"}

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
