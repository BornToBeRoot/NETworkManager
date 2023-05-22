---
layout: default
title: Download
nav_order: 3
description: "Download the latest available version"
permalink: /Download
---

# 2023.4.26.0

Release date: **26.04.2023**

<br />
<div style="text-align: center">  
  <p>
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.4.26.0/NETworkManager_2023.4.26.0_Setup.exe' style="text-decoration: none;" target="_blank">
      <span class="fs-5"><button type="button" name="button" class="btn btn-primary" style="width: 150px;">:package: Setup</button></span>
    </a>
    &nbsp;
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.4.26.0/NETworkManager_2023.4.26.0_Portable.zip' style="text-decoration: none;" target="_blank">
      <span class="fs-5"><button type="button" name="button" class="btn btn-primary" style="width: 150px;">:package: Portable</button></span>
    </a>
    &nbsp;
    <a href='https://github.com/BornToBeRoot/NETworkManager/releases/download/2023.4.26.0/NETworkManager_2023.4.26.0_Archive.zip' style="text-decoration: none;" target="_blank">
      <span class="fs-5"><button type="button" name="button" class="btn btn-primary" style="width: 150px;">:package: Archive</button></span>
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
      <td><code>5276974F44D1B32E759E1AAECF60B8440164B34B501B0587DEACB367AA1FF304</code></td>
    </tr>
    <tr>
      <td>Portable</td>
      <td><code>D24AE09AA2179CA23EE5F49E0B2CC0477DB9583B3474241051693E243D512668</code></td>
    </tr>
    <tr>
      <td>Archive</td>
      <td><code>7D5D613F9E759D169747B2B89AC20D9D9ABE2874B5CF620620F471963D52247D</code></td>
    </tr>
  </table>
</details>

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
