---
layout: default
title: Next release
parent: Changelog
nav_order: 1
description: "Changelog for next release"
permalink: /Changelog/next-release
---

Version: **Next release** <br />
Release date: **xx.xx.2023**

**System requirements**

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="\_blank"}

## What's new?

New Feature
{: .label .label-green }

Breaking Change
{: .label .label-red }

- **SNTP Lookup** [#1821](https://github.com/BornToBeRoot/NETworkManager/pull/1821){:target="\_blank"} (See [documentation](https://borntoberoot.net/NETworkManager/Documentation/Application/SNTPLookup){:target="\_blank"} for more details)
- Profiles and settings are now saved in the `%UserProfile%\Documents\NETworkManager` folder instead of the `%AppData%\NETworkManager` folder. If folder redirection is set up (e.g. OneDrive, Group Policy, etc.). The profiles and settings (files) are automatically transferred to other devices and may be automatically backed up depending on your configuration. I hope this will provide better support for virtual environments / terminal servers. [#1984](https://github.com/BornToBeRoot/NETworkManager/pull/1984){:target="\_blank"}

{: .note }
The profiles and settings are automatically migrated to the new location when the application is started. You can check the log under `%LocalAppData%\NETworkManager\NETworkManager.log` for more details and possible errors. This does not apply to portable version.

## Improvements

- Move .NET runtime components and dependencies into a subfolder to simplify the installation/portable folder [#1832](https://github.com/BornToBeRoot/NETworkManager/pull/1832){:target="\_blank"}
- Installer (Inno Setup)
  - Uninstall previous versions of NETworkManager when installing a new version [2382f1f](https://github.com/BornToBeRoot/NETworkManager/commit/2382f1fc5e95d7165f56cb7f42c27e1e281abbf2){:target="\_blank"}
- Reduce the size of the installer, portable and archive build [#1832](https://github.com/BornToBeRoot/NETworkManager/pull/1832){:target="\_blank"}
- Profiles
  - Show password dialog again if the password is not correct [#1962](https://github.com/BornToBeRoot/NETworkManager/pull/1962){:target="\_blank"}
- DataGrid Column header design improved [#1910](https://github.com/BornToBeRoot/NETworkManager/pull/1910){:target="\_blank"}
- DataGrid Columns can now be resized [#1910](https://github.com/BornToBeRoot/NETworkManager/pull/1910){:target="\_blank"}
- Add text wrapping for status textboxes [#1949](https://github.com/BornToBeRoot/NETworkManager/pull/1949){:target="\_blank"}
- Toggle button added to hide profile list if the profiles are locked [#1991](https://github.com/BornToBeRoot/NETworkManager/pull/1991){:target="\_blank"}
- Settings > Network
  - Global option added to set the prefered IP protocol (IPv4/IPv6) for DNS resolution. [#1950](https://github.com/BornToBeRoot/NETworkManager/pull/1950){:target="\_blank"}
- **Network Interface**
  - Release & Renew button now targets the selected interface instead of all interfaces [#1982](https://github.com/BornToBeRoot/NETworkManager/pull/1982){:target="\_blank"}
  - Release & Renew button now supports IPv6 [#1982](https://github.com/BornToBeRoot/NETworkManager/pull/1982){:target="\_blank"}  
- **WiFi**
  - Improve search - you can now search for SSID, Security, Channel, BSSID (MAC address), Vendor and Phy kind [#1941](https://github.com/BornToBeRoot/NETworkManager/pull/1941){:target="\_blank"}
- **IP Scanner**
  - Max threads changed to to 1024 [#1927](https://github.com/BornToBeRoot/NETworkManager/pull/1927){:target="\_blank"}
  - Event handling when a host is found, user has canceled, etc. [#1969](https://github.com/BornToBeRoot/NETworkManager/pull/1969){:target="\_blank"}
  - Remove option to set the preferred IP protocol for DNS resolution (can now be set globally) [#1950](https://github.com/BornToBeRoot/NETworkManager/pull/1950){:target="\_blank"}
- **Port Scanner**
  - Add new port profiles / improve existing ones [#1909](https://github.com/BornToBeRoot/NETworkManager/pull/1909){:target="\_blank"}
  - New Port state "Timed out" if the timelimit is reached. [#1969](https://github.com/BornToBeRoot/NETworkManager/pull/1969){:target="\_blank"}
  - Select multiple port profiles with `Ctrl` or holding left mouse button [#1979](https://github.com/BornToBeRoot/NETworkManager/pull/1979){:target="\_blank"}
  - Remove option to set the preferred IP protocol for DNS resolution (can now be set globally) [#1950](https://github.com/BornToBeRoot/NETworkManager/pull/1950){:target="\_blank"}
  - Max host threads changed to to 256 [#1927](https://github.com/BornToBeRoot/NETworkManager/pull/1927){:target="\_blank"}
  - Max port threads changed to to 1024 [#1927](https://github.com/BornToBeRoot/NETworkManager/pull/1927){:target="\_blank"}
- **Ping Monitor**
  - Allow multiple hosts like `server-01; 1.1.1.1; example.com` as input [#1933](https://github.com/BornToBeRoot/NETworkManager/pull/1933){:target="\_blank"}
  - Use a fixed size for the view and enable horizonal scrollbar [#1933](https://github.com/BornToBeRoot/NETworkManager/pull/1933){:target="\_blank"} [#1990](https://github.com/BornToBeRoot/NETworkManager/pull/1990){:target="\_blank"}
  - Remove option to set the preferred IP protocol for DNS resolution (can now be set globally) [#1950](https://github.com/BornToBeRoot/NETworkManager/pull/1950){:target="\_blank"}
  - Display ping error in the UI (with error message as tooltip) instead of silently aborting the ping [#1993](https://github.com/BornToBeRoot/NETworkManager/pull/1993){:target="\_blank"}
- **Traceroute**
  - Add more validation (host, ip address) to input [#1932](https://github.com/BornToBeRoot/NETworkManager/pull/1932){:target="\_blank"}
  - Remove option to set the preferred IP protocol for DNS resolution (can now be set globally) [#1950](https://github.com/BornToBeRoot/NETworkManager/pull/1950){:target="\_blank"}
- **DNS Lookup**
  - Improve overall DNS lookup performance with multiple DNS servers and hosts [#1940](https://github.com/BornToBeRoot/NETworkManager/pull/1940){:target="\_blank"}
  - New dialog to add/edit DNS servers [#1960](https://github.com/BornToBeRoot/NETworkManager/pull/1960){:target="\_blank"}
- **PuTTY**
  - Parameter `-hostkey` is now supported and can be configured in the profile. Multiple hostkeys can be separated by a comma. [#1977](https://github.com/BornToBeRoot/NETworkManager/pull/1977){:target="\_blank"}
- **SNMP**
  - Remove option to set the preferred IP protocol for DNS resolution (can now be set globally) [#1950](https://github.com/BornToBeRoot/NETworkManager/pull/1950){:target="\_blank"}
- **Wake on LAN**
  - Update view & code cleanup [#1995](https://github.com/BornToBeRoot/NETworkManager/pull/1995){:target="\_blank"}
  - Add history for MAC address & broadcast [#1995](https://github.com/BornToBeRoot/NETworkManager/pull/1995){:target="\_blank"}

## Bugfixes

- Detect if the DNS result for a query is null even when the DNS server doesn't send an error code (because some providers do not implement their DNS server correctly...) [#1949](https://github.com/BornToBeRoot/NETworkManager/pull/1949){:target="\_blank"}
- Improve the error message for DNS lookup to get more details (translation is not supported) [#1949](https://github.com/BornToBeRoot/NETworkManager/pull/1949){:target="\_blank"}
- **Dashboard**
  - `F5` key is now working to refresh the dashboard (again) [#1969](https://github.com/BornToBeRoot/NETworkManager/pull/1969){:target="\_blank"}
- **IP Scanner**
  - In some cases the IP scan is not completed, but the user interface indicates that it is completed [#1969](https://github.com/BornToBeRoot/NETworkManager/pull/1969){:target="\_blank"}
- **Port Scanner**
  - In some cases the IP scan is not completed, but the user interface indicates that it is completed [#1969](https://github.com/BornToBeRoot/NETworkManager/pull/1969){:target="\_blank"}
- **DNS Lookup**
  - Detect if the DNS result for a query is null even when the DNS server doesn't send an error code and improve the processing of the resource records answers [#1949](https://github.com/BornToBeRoot/NETworkManager/pull/1949){:target="\_blank"}
  - In some cases the DNS lookup is not completed, but the user interface indicates that it is completed [#1940](https://github.com/BornToBeRoot/NETworkManager/pull/1940){:target="\_blank"} [#1949](https://github.com/BornToBeRoot/NETworkManager/pull/1949){:target="\_blank"}
  - Detect if a dns server profile with this name already exists [#1960](https://github.com/BornToBeRoot/NETworkManager/pull/1960){:target="\_blank"}
- **Traceroute**
  - Don't block the UI if an Exception occurs and show an error message [#1994](https://github.com/BornToBeRoot/NETworkManager/pull/1994){:target="\_blank"}
- **AWS Session Manager**
  - Use UTF-8 encoding for embedded PowerShell console window [#1925](https://github.com/BornToBeRoot/NETworkManager/pull/1925){:target="\_blank"}
- **Discovery Protocol**
  - Discovery Protocol was not working in release because a .dll was missing... Fixed by setting the dotnet RuntimeIdentfier from `win-x64` to `win10-x64` (See [PowerShell/PowerShell#7909](https://github.com/PowerShell/PowerShell/issues/7909){:target="\_blank"} for more details) [#1951](https://github.com/BornToBeRoot/NETworkManager/pull/1951){:target="\_blank"}


## Deprecated

- Profiles
  - The settings folder path can no longer be set to a custom path [#1984](https://github.com/BornToBeRoot/NETworkManager/pull/1984){:target="\_blank"}
- Settings
  - The profiles folder path can no longer be set to a custom path [#1984](https://github.com/BornToBeRoot/NETworkManager/pull/1984){:target="\_blank"}
  - Import and export of profiles is no longer supported [#1984](https://github.com/BornToBeRoot/NETworkManager/pull/1984){:target="\_blank"}

{: .note }
The outdated functions were buggy, incomplete and difficult to maintain. I decided to remove them and adjust the path to the profiles and settings (See [What's new](#whats-new)). You may also perform a backup/import/export/migration by manually copying the profile or settings files (See [FAQ > Where are files stored?](https://borntoberoot.net/NETworkManager/FAQ#where-are-files-stored)).

## Other

- Code cleanup [#1932](https://github.com/BornToBeRoot/NETworkManager/pull/1932){:target="\_blank"} [#1949](https://github.com/BornToBeRoot/NETworkManager/pull/1940){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
- Add documentation for: [#265](https://github.com/BornToBeRoot/NETworkManager/pull/265){:target="\_blank"}
  - [Dashboard](https://borntoberoot.net/NETworkManager/Documentation/Application/Dashboard){:target="\_blank"}
  - [Network Interface](https://borntoberoot.net/NETworkManager/Documentation/Application/NetworkInterface){:target="\_blank"
  - [WiFi](https://borntoberoot.net/NETworkManager/Documentation/Application/WiFi){:target="\_blank"}
  - [IP Scanner](https://borntoberoot.net/NETworkManager/Documentation/Application/IPScanner){:target="\_blank"}
