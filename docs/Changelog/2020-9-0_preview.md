---
layout: default
title: 2020.9.0 [Preview]
parent: Changelog
nav_order: 994
description: "Changelog for version 2020.5.0 preview"
permalink: /Changelog/2020-9-0-preview
---

Version: **2020.9.0 - Preview** <br />
Release date: **05.09.2020**

| File                                                                                                                       | Checksum [SHA256]                                                  |
| -------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| [Setup](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.9.0/NETworkManager_2020.9.0_Setup.exe)       | `0EC8C1371C4D62947AE0DC70DBAA91FE105CACECD32DF4A4451C95F71D085014` |
| [Portable](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.9.0/NETworkManager_2020.9.0_Portable.zip) | `E8B60E677E65959BB11F05884F6EA9EC99F09F24ECE869A240B8361F958B4A66` |
| [Archive](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.9.0/NETworkManager_2020.9.0_Archive.zip)   | `86D13E4C139132BCBA86B6F72ADA4D679C4DD303EA08643D276AE1A5DFD47C47` |

**New system requirements**

- Windows 10 x64
- [.NET Desktop Runtime 5.0.0-preview.8](https://dotnet.microsoft.com/download/dotnet/5.0){:target="\_blank"}
- [Microsoft Edge Canary](https://www.microsoftedgeinsider.com/en-us/download){:target="\_blank"}

**Kown issues**

- Application crash on fullscreen [#325](http://github.com/BornToBeRoot/NETworkManager/issues/325){:target="\_blank"}
- [More...](https://github.com/BornToBeRoot/NETworkManager/issues?q=label%3A.NET5.0-feedback+){:target="\_blank"}

## What's new?

- Migration to ~~.NET Core 3.1~~ .NET 5.0!!! (requires [.NET Desktop Runtime 5.0.0-repview.8](https://dotnet.microsoft.com/download/dotnet/5.0)) [#309](http://github.com/BornToBeRoot/NETworkManager/issues/309){:target="\_blank"}
- Migration to MahApps.Metro Version 2.x
- Migration to WebView2 (requires [Microsoft Edge Canary](https://www.microsoftedgeinsider.com/en-us/download)) [#252](http://github.com/BornToBeRoot/NETworkManager/issues/252){:target="\_blank"}

## Improvements

- WebConsole
  - Untrusted SSL certificates can now be accepted [#266](http://github.com/BornToBeRoot/NETworkManager/issues/266){:target="\_blank"}

## Bugfixes

- Subnet Calculator
  - Subnetting - App crash fixed if subnetmask was used [#319](http://github.com/BornToBeRoot/NETworkManager/issues/319){:target="\_blank"}

## Other

- Libraries updated
