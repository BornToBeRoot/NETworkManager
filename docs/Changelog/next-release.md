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

## Breaking changes

Breaking changes
{: .label .label-red }

- AWS Session Manager
  - Setting `Default profile` changed to `Profile`. You need to set the profile again [#2299](https://github.com/BornToBeRoot/NETworkManager/pull/2299){:target="\_blank"}
  - Setting `Default region` changed to `Region`. You need to set the region again [#2299](https://github.com/BornToBeRoot/NETworkManager/pull/2299){:target="\_blank"}

## What's new?

## Improvements

- **DNS Lookup**
  - Improved validation when adding/changing DNS server profiles [#2282](https://github.com/BornToBeRoot/NETworkManager/pull/2282){:target="\_blank"}
- **Remote Desktop**
  - Allow `.` as domain to authenticate with local accounts [#2305](https://github.com/BornToBeRoot/NETworkManager/pull/2305){:target="\_blank"}
- **TigerVNC**
  - Use port from default config instead of settings if creating a new group [#2249](https://github.com/BornToBeRoot/NETworkManager/pull/2249){:target="\_blank"}
- **SNTP Lookup**
  - Improved validation when adding/changing SNTP server profiles [#2282](https://github.com/BornToBeRoot/NETworkManager/pull/2282){:target="\_blank"}
- **Wake on LAN**
  - Change default port from 7 to 9 [#2242](https://github.com/BornToBeRoot/NETworkManager/pull/2242){:target="\_blank"}
- Group dialog
  - Show an error next to the tab if the settings on the page contain errors [#2309](https://github.com/BornToBeRoot/NETworkManager/pull/2309){:target="\_blank"}
- Profile dialog
  - Show an error next to the tab if the settings on the page contain errors [#2309](https://github.com/BornToBeRoot/NETworkManager/pull/2309){:target="\_blank"}
  - SNMP options are now validates (same as group) [#2309](https://github.com/BornToBeRoot/NETworkManager/pull/2309){:target="\_blank"}
- Log program update check & errors to the log file [#2329](https://github.com/BornToBeRoot/NETworkManager/pull/2329){:target="\_blank"}

## Bugfixes

- **DNS Lookup**
  - Fixed app crash when editing DNS server profile in some rare cases [#2282](https://github.com/BornToBeRoot/NETworkManager/pull/2282){:target="\_blank"}
- **Remote Desktop**
  - `Adjust screen` via Context Menu now only adjusts the screen size if `Adjust screen automatically` or `Use the current view size as the screen size` is enabled [#2318](https://github.com/BornToBeRoot/NETworkManager/pull/2318){:target="\_blank"}
- **PuTTY**
  - Add missing baud rates for serial connection [#2337](https://github.com/BornToBeRoot/NETworkManager/pull/2337){:target="\_blank"}
- **SNTP Lookup**
  - Fixed app crash when editing SNTP server profile in some rare cases [#2282](https://github.com/BornToBeRoot/NETworkManager/pull/2282){:target="\_blank"}
- **Subnet Calculator**
  - Fixed a design issue with the calculate button in subnetting [#2230](https://github.com/BornToBeRoot/NETworkManager/pull/2230){:target="\_blank"}
- [AirSpace Fixer](https://www.nuget.org/packages/AirspaceFixer){:target="\_blank"} code optimized (only called if needed). This will also prevent a screenshot bug when the application is loading and a dialog is shown. [#2253](https://github.com/BornToBeRoot/NETworkManager/pull/2253){:target="\_blank"}
- Group dialog
  - Fix default value for Remote Desktop sreen size [#2293](https://github.com/BornToBeRoot/NETworkManager/pull/2293){:target="\_blank"}
  - Enable Remote Desktop `Use gateway credentials` only if logon method is set to `Userpass` [#2316](https://github.com/BornToBeRoot/NETworkManager/pull/2316){:target="\_blank"}
- Profile dialog
  - Fix default value for Remote Desktop sreen size [#2293](https://github.com/BornToBeRoot/NETworkManager/pull/2293){:target="\_blank"}
  - Enable Remote Desktop `Use gateway credentials` only if logon method is set to `Userpass` [#2316](https://github.com/BornToBeRoot/NETworkManager/pull/2316){:target="\_blank"}
  - Fix validation rule for TigerVNC port [#2309](https://github.com/BornToBeRoot/NETworkManager/pull/2309){:target="\_blank"}
- Settings
  - Settings view is now re-selected if the filter (search) is removed [#2325](https://github.com/BornToBeRoot/NETworkManager/pull/2325){:target="\_blank"}
  - If settings are opened again, the last selected settings view is now selected (except an application was selected that has a settings view, then the settings view of the application is selected) [#2325](https://github.com/BornToBeRoot/NETworkManager/pull/2325){:target="\_blank"}

## Deprecated

Deprecated
{: .label .label-red }

- Profile
  - Wake on LAN port, because it was inconsistent (If you select the profile, the port from the settings was used. If you right-click on the profile, the port from the profile was used) [#2220](https://github.com/BornToBeRoot/NETworkManager/pull/2220){:target="\_blank"}

## Other

- Code cleanup [#2100](https://github.com/BornToBeRoot/NETworkManager/pull/2100){:target="\_blank"} [#2172](https://github.com/BornToBeRoot/NETworkManager/pull/2172){:target="\_blank"} [#2254](https://github.com/BornToBeRoot/NETworkManager/pull/2254){:target="\_blank"} [#2325](https://github.com/BornToBeRoot/NETworkManager/pull/2325){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
- Add documentation for:
  - Application > Port Scanner [#2265](https://github.com/BornToBeRoot/NETworkManager/pull/2265){:target="\_blank"}
  - Application > Ping Monitor [#2267](https://github.com/BornToBeRoot/NETworkManager/pull/2267){:target="\_blank"}
  - Application > Traceroute [#2268](https://github.com/BornToBeRoot/NETworkManager/pull/2268){:target="\_blank"}
  - Application > DNS Lookup [#2273](https://github.com/BornToBeRoot/NETworkManager/pull/2273){:target="\_blank"}
  - Application > Remote Desktop [#2293](https://github.com/BornToBeRoot/NETworkManager/pull/2293){:target="\_blank"} [#2324](https://github.com/BornToBeRoot/NETworkManager/pull/2324){:target="\_blank"}
  - Application > PowerShell [#2249](https://github.com/BornToBeRoot/NETworkManager/pull/2249){:target="\_blank"} [#2324](https://github.com/BornToBeRoot/NETworkManager/pull/2324){:target="\_blank"}
  - Application > PuTTY [#2324](https://github.com/BornToBeRoot/NETworkManager/pull/2324){:target="\_blank"} [#2337](https://github.com/BornToBeRoot/NETworkManager/pull/2337){:target="\_blank"}
  - Application > TigerVNC [#2248](https://github.com/BornToBeRoot/NETworkManager/pull/2248){:target="\_blank"} [#2324](https://github.com/BornToBeRoot/NETworkManager/pull/2324){:target="\_blank"}
  - Application > Web Console [#2244](https://github.com/BornToBeRoot/NETworkManager/pull/2244){:target="\_blank"} [#2324](https://github.com/BornToBeRoot/NETworkManager/pull/2324){:target="\_blank"}
  - Application > SNMP [#2289](https://github.com/BornToBeRoot/NETworkManager/pull/2289){:target="\_blank"} [#2293](https://github.com/BornToBeRoot/NETworkManager/pull/2293){:target="\_blank"}
  - Application > Wake on LAN [#2242](https://github.com/BornToBeRoot/NETworkManager/pull/2242){:target="\_blank"}
  - Application > Whois [#2236](https://github.com/BornToBeRoot/NETworkManager/pull/2236){:target="\_blank"}
  - Application > Subnet Calculator [#2233](https://github.com/BornToBeRoot/NETworkManager/pull/2233){:target="\_blank"}
- Update documentation for:
  - Application > AWS Session Manager [#2299](https://github.com/BornToBeRoot/NETworkManager/pull/2299){:target="\_blank"} [#2324](https://github.com/BornToBeRoot/NETworkManager/pull/2324){:target="\_blank"}
