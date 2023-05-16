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

## Improvements

- TigerVNC
  - Use port from default config instead of settings if creating a new group [#2249](https://github.com/BornToBeRoot/NETworkManager/pull/2249){:target="\_blank"}
- Wake on LAN
  - Change default port from 7 to 9 [#2242](https://github.com/BornToBeRoot/NETworkManager/pull/2242){:target="\_blank"}

## Bugfixes

- Subnet Calculator
  - Fixed a design issue with the calculate button in subnetting [#2230](https://github.com/BornToBeRoot/NETworkManager/pull/2230){:target="\_blank"}
- [AirSpace Fixer](https://www.nuget.org/packages/AirspaceFixer){:target="\_blank"} code optimized (only called if needed). This will also prevent a screenshot bug when the application is loading and a dialog is shown. [#2253](https://github.com/BornToBeRoot/NETworkManager/pull/2253){:target="\_blank"}

## Deprecated

Deprecated
{: .label .label-red }

- Profiles
  - Wake on LAN port, because it was inconsistent (If you select the profile, the port from the settings was used. If you right-click on the profile, the port from the profile was used) [#2220](https://github.com/BornToBeRoot/NETworkManager/pull/2220){:target="\_blank"}

## Other

- Code cleanup [#2100](https://github.com/BornToBeRoot/NETworkManager/pull/2100){:target="\_blank"} [#2172](https://github.com/BornToBeRoot/NETworkManager/pull/2172){:target="\_blank"} [#2254](https://github.com/BornToBeRoot/NETworkManager/pull/2254){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
- Add documentation for:
  - Application > Port Scanner [#2265](https://github.com/BornToBeRoot/NETworkManager/pull/2265){:target="\_blank"}
  - Application > Ping Monitor [#2267](https://github.com/BornToBeRoot/NETworkManager/pull/2267){:target="\_blank"}
  - Application > Traceroute [#2268](https://github.com/BornToBeRoot/NETworkManager/pull/2268){:target="\_blank"}
  - Application > DNS Lookup [#2273](https://github.com/BornToBeRoot/NETworkManager/pull/2273){:target="\_blank"}
  - Application > PowerShell [#2249](https://github.com/BornToBeRoot/NETworkManager/pull/2249){:target="\_blank"}
  - Application > TigerVNC [#2248](https://github.com/BornToBeRoot/NETworkManager/pull/2248){:target="\_blank"}
  - Application > Web Console [#2244](https://github.com/BornToBeRoot/NETworkManager/pull/2244){:target="\_blank"}
  - Application > Wake on LAN [#2242](https://github.com/BornToBeRoot/NETworkManager/pull/2242){:target="\_blank"}
  - Application > Whois [#2236](https://github.com/BornToBeRoot/NETworkManager/pull/2236){:target="\_blank"}
  - Application > Subnet Calculator [#2233](https://github.com/BornToBeRoot/NETworkManager/pull/2233){:target="\_blank"}
