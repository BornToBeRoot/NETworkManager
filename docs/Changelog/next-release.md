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

- IP Scanner
  - Scan common ports (`22; 53; 80; 139; 389; 636; 443; 445; 3389`) to test if a host is reachable. The feature is enabled by default and can be configured and disabled in the settings. [#2026](https://github.com/BornToBeRoot/NETworkManager/pull/2026){:target="\_blank"}

## Improvements

- (Re)loading of the profiles improved. Code has been optimized and user interface update calls have been reduced. WPF virtualizing explicitly activated. The scrollbar in the profile list is now always visible, because the size is calculated by WPF each time on mouse over. [#2014](https://github.com/BornToBeRoot/NETworkManager/pull/2014){:target="\_blank"}
- Reselect profile or select first profile after (re)loading or search [#2014](https://github.com/BornToBeRoot/NETworkManager/pull/2014){:target="\_blank"}
- Some designs have been improved (Error & Warning icons). [#2014](https://github.com/BornToBeRoot/NETworkManager/pull/2014){:target="\_blank"}
- You can now configure the application wide [ThreadPool](https://learn.microsoft.com/en-us/dotnet/standard/threading/the-managed-thread-pool) under `Settings > General > Multithreading`, which is used for the IP scanner and the port scanner. The default value for min. threads are CPU threads + 512. Depending on the hardware, this can improve the performance of the scan. [#2026](https://github.com/BornToBeRoot/NETworkManager/pull/2026){:target="\_blank"}
- **Network Interface**
  - Add a button to redirect the the IPv4 address and subnetmask of the selected network interface to the IP scanner [#2046](https://github.com/BornToBeRoot/NETworkManager/pull/2046){:target="\_blank"}
- **IP Scanner**
  - Option added to limit the number of concurrent threads per host scan (256) & port scan (5). Increasing the values can speed up the scan, but can also lead to resource problems. [#2026](https://github.com/BornToBeRoot/NETworkManager/pull/2026){:target="\_blank"}
- **Port Scanner**
  - Option added to limit the number of concurrent threads per host scan (5) & port scan (256). Increasing the values can speed up the scan, but can also lead to resource problems. [#2026](https://github.com/BornToBeRoot/NETworkManager/pull/2026){:target="\_blank"}

## Bugfixes

- Show error message when redirecting to another application, but the application is hidden in the settings or somehow invalid [#2046](https://github.com/BornToBeRoot/NETworkManager/pull/2046){:target="\_blank"}
- Group name check is now case insensitive and a group name can only be used once. If you create a group named `Test`, you cannot create a group named `test`. [#2014](https://github.com/BornToBeRoot/NETworkManager/pull/2014){:target="\_blank"}
- Profile dialog
  - Validate AWS Session Manager input (instance ID, profile, region) [#2025](https://github.com/BornToBeRoot/NETworkManager/pull/2025){:target="\_blank"}
- **IP Scanner**
  - Export to CSV fixed if `Vendor` contains a comma [#2026](https://github.com/BornToBeRoot/NETworkManager/pull/2026){:target="\_blank"}
- **Port Scanner**
  - Show port and protocol if service (and desciption) is not available [#2026](https://github.com/BornToBeRoot/NETworkManager/pull/2026){:target="\_blank"}
  - Export to CSV fixed if `Description` contains a comma [#2026](https://github.com/BornToBeRoot/NETworkManager/pull/2026){:target="\_blank"}
- **PowerShell**
  - ContextMenu in TabControl in a dragged out window was closed automatically due to a focus problem right after opening it [#2064](https://github.com/BornToBeRoot/NETworkManager/pull/2064){:target="\_blank"}
  - Resize command in TabControl context menu in a dragged out window not working [#2060](https://github.com/BornToBeRoot/NETworkManager/pull/2060){:target="\_blank"}
- **PuTTY**
  - ContextMenu in TabControl in a dragged out window was closed automatically due to a focus problem right after opening it [#2064](https://github.com/BornToBeRoot/NETworkManager/pull/2064){:target="\_blank"}
- **AWS Session Manager**
  - ContextMenu in TabControl in a dragged out window was closed automatically due to a focus problem right after opening it [#2064](https://github.com/BornToBeRoot/NETworkManager/pull/2064){:target="\_blank"}
  - Resize command in TabControl context menu in a dragged out window not working [#2060](https://github.com/BornToBeRoot/NETworkManager/pull/2060){:target="\_blank"}
- **Web Console**
  - Reload command in TabControl context menu in main window not working [#2060](https://github.com/BornToBeRoot/NETworkManager/pull/2060){:target="\_blank"}
- **Lookup - OUI**
  - Export to CSV fixed if `Vendor` contains a comma [#2026](https://github.com/BornToBeRoot/NETworkManager/pull/2026){:target="\_blank"}
- **Lookup - Port**
  - Export to CSV fixed if `Description` contains a comma [#2026](https://github.com/BornToBeRoot/NETworkManager/pull/2026){:target="\_blank"}

## Other

- Code cleanup [#2024](https://github.com/BornToBeRoot/NETworkManager/pull/2024){:target="\_blank"} [#2026](https://github.com/BornToBeRoot/NETworkManager/pull/2026){:target="\_blank"} [#2060](https://github.com/BornToBeRoot/NETworkManager/pull/2060){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
