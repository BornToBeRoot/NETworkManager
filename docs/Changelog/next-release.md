---
layout: default
title: Next release
parent: Changelog
nav_order: 1
description: "Changelog for next release"
permalink: /Changelog/next-release
---

# Next release

**System requirements**
- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="_blank"}
- [Microsoft Edge - WebView2 Runtime (Evergreen)](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="_blank"}

## What's new?
- AWS Systems Manger Session Manager [#1544](https://github.com/BornToBeRoot/NETworkManager/pull/1544){:target="\_blank"}

## Improvements
- IPScanner, PortScanner, DNSServers
  - Generate default lists at first run [#1544](https://github.com/BornToBeRoot/NETworkManager/pull/1544){:target="\_blank"}
- Port Scanner
  - Add more port profiles (LDAP, HTTP proxy, Filetransfer) [#1526](https://github.com/BornToBeRoot/NETworkManager/pull/1526){:target="\_blank"}
- PowerShell & PuTTY
  - Focus embedded window when the application has received the focus [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}
  - Focus embedded window when application is selected again [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}
  - Focus embedded window when switching tabs [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}
- PuTTY
  - Detect if PuTTY is installed at first run [#1542](https://github.com/BornToBeRoot/NETworkManager/pull/1542){:target="\_blank"}
- Discovery Protocol
  - Add local connection & local interface to output [#1533](https://github.com/BornToBeRoot/NETworkManager/pull/1533){:target="\_blank"}
- Settings > Language
  - Improve application restart [#1599](https://github.com/BornToBeRoot/NETworkManager/pull/1599){:target="\_blank"}
- Settings > Update
  - Option to check for pre-releases added [#1574](https://github.com/BornToBeRoot/NETworkManager/pull/1574){:target="\_blank"}
- Profiles > Group Dialog
  - Remove checkboxes in group dialog [#1530](https://github.com/BornToBeRoot/NETworkManager/pull/1530){:target="\_blank"}
- log4net added for error handling (Log file: `%LocalAppData%\NETworkManager\NETworkManager.log`) [#1539](https://github.com/BornToBeRoot/NETworkManager/pull/1539){:target="\_blank"}

## Bugfixes
- Dashboard / Status Window
  - Handle null exception properly [#1510](https://github.com/BornToBeRoot/NETworkManager/pull/1510){:target="\_blank"}
- Traceroute
  - Hops were skipped if they did not respond to ping [#1528](https://github.com/BornToBeRoot/NETworkManager/pull/1528){:target="\_blank"}
- Ping Monitor, Traceroute & SNMP
  - IPv6 is now resolved when selected in the settings [#1529](https://github.com/BornToBeRoot/NETworkManager/pull/1529){:target="\_blank"}
- Remote Desktop, PowerShell, PuTTY & TigerVNC
  - "Unlock profile" dialog is now displayed correctly if an embedded window is already open [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}
- PowerShell & TigerVNC
  - Embedded window is now resized correctly after the inital connect [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}
- Settings > Profiles
  - App crash fixed when a profile file is deleted and an encrypted but locked profile file is selected [#1512](https://github.com/BornToBeRoot/NETworkManager/pull/1512){:target="\_blank"}
  - Reselect group/profile on refresh (e.g. if profiles page is opened again) [#1516](https://github.com/BornToBeRoot/NETworkManager/pull/1516){:target="\_blank"}

## Other
- Code cleanup [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"} [#1599](https://github.com/BornToBeRoot/NETworkManager/pull/1600){:target="\_blank"} [#1600](https://github.com/BornToBeRoot/NETworkManager/pull/1599){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="_blank"}
