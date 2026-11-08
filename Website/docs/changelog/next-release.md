---
sidebar_position: 0
description: "Changelog for the next NETworkManager release — upcoming features, improvements, and bug fixes."
keywords:  [NETworkManager, changelog, release notes, next release, upcoming features, bug fixes]
---

# Next Release

Version: **Next release** <br />
Release date: **xx.xx.2026**

| File | `SHA256` |
| ---- | -------- |

**System requirements**

- Windows 10 / Server x64/ARM64 (22H2 or later)
- [.NET Desktop Runtime 10.0 (LTS) - x64/ARM64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime)

## Breaking Changes

## What's new?

**mDNS Discovery**

- Added a dedicated browser for services advertised on the local network, including printers, scanners, web servers and media devices. Select an interface and service types or service type profiles (e.g. Printers, editable in the settings, with history) or discover all types, inspect hostnames, ports, IPv4 / IPv6 addresses and TXT records, filter, copy and export the results. **Raw DNS records** shows the individual DNS records instead, with each TXT entry on its own row; other and unknown record types are preserved as raw data. Includes the `mdns` / `dnssd` run commands (e.g. `mdns printers 30`) with suggestions of the service type profiles and durations. Thanks to [@MovGP0](https://github.com/MovGP0) [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

## Improvements

**LLDP / CDP Capture**

- Renamed "Discovery Protocol" to "LLDP / CDP Capture", so it is clear what the tool does (and it is not confused with mDNS Discovery). The run command is now `lldpcdpcapture` with the aliases `lldp` and `cdp`, the documentation moved to [LLDP / CDP Capture](../application/lldp-cdp-capture.md). The selected protocol and duration are reset to the defaults. [#3721](https://github.com/BornToBeRoot/NETworkManager/issues/3721)

## Bug Fixes

**Tabs**

- Fixed the embedded windows (e.g. PuTTY, Remote Desktop) covering the drop zones again while a tab was dragged if another window of the same tool closed during the drag. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed the content of a tab staying visible without its tab after the tab was dragged into another window and out again quickly. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab dropped into an empty tab bar (or far behind the last tab) being dragged out again right away. A tab now leaves the tab bar only when it is dragged out of the whole tab bar. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed the tab title (e.g. the host of a scan) not being updated when the tab is in a dragged out window that is not the active window. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- The tab headers show the focus ring of the application (keyboard navigation) instead of a dotted rectangle. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- The other tabs no longer slide to their new position while a tab is dragged if animations are disabled (Settings > Appearance > Animations). [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab (and its session, e.g. PuTTY or Remote Desktop) being lost when it was dragged out as a window after a previous drag had merged a tab into another window: the drop zone of the earlier drag was reused. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a crash while dragging a tab as a window when another tab window closed at the same time (e.g. its session ended). [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab dragged back to its original position while the other tabs were still sliding ending up at the wrong position. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab dropped into another tab bar being inserted at the wrong position with display scaling above 100 %. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed the splitter between tabs split on top of each other also reacting 3 px into the tabs on the left and right. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)

**Network Interface**

- Fixed **Apply** failing when the IPv4 address or the subnet mask on the **Configure** tab (or in **Add IPv4 address...**) started or ended with a space. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed **Apply** and **Add IPv4 address...** failing when the CIDR was entered without a slash (e.g. `24` instead of `/24`), although it was accepted as valid. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed values of a profile (IP address, subnet mask, gateway, DNS servers) being passed to `netsh` without checking them again. A manipulated or shared profile file could run other commands with elevated rights when the profile was applied. Invalid values are now reported and nothing is changed. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed **Apply** failing for a static IP address without a default gateway. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed the configuration being blocked (progress bar running until a restart) when **Apply** was used after the selected network adapter was removed (e.g. a USB or VPN adapter). The first adapter is selected instead. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed **Apply** in the profile context menu being available while another change was running or the selected network adapter was not connected (the **Apply** button was already disabled). [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed the **Bandwidth** chart and statistics no longer updating after the network configuration changed (e.g. after **Apply**, a DHCP renewal or connecting a VPN). [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)

**DNS Lookup**

- Fixed the last column of the DNS servers (settings) not filling the remaining width. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)
- Fixed a DNS server name with a leading / trailing space not being detected as a duplicate (settings). [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**SNTP Lookup**

- Fixed the last column of the SNTP servers (settings) not filling the remaining width. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)
- Fixed an SNTP server name with a leading / trailing space not being detected as a duplicate (settings). [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**Hosts File Editor**

- Fixed the last column (comment) not filling the remaining width. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**Settings**

- Fixed the profile files (Settings > Profiles) not filling the width of the grid: the name column fills the remaining width. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)
- Fixed a custom DNS server name (Settings > Network) with a leading / trailing space not being detected as a duplicate. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**Profiles**

- Fixed the profile list not filling the width of the grid: the name column fills the remaining width. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**Port Scanner**

- Fixed a port profile name with a leading / trailing space (e.g. `Webserver `) not being detected as a duplicate: the ports input always used the first port profile with the name. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**About**

- Fixed the license of ControlzEx in the library list: it showed the license text and link of Dragablz. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Updated the license of MahApps.Metro in the library list (copyright: .NET Foundation and Contributors). [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Dragablz (tab control) is now part of the repository (`Source/3rdparty/Dragablz`, stripped to the code we use) instead of a git submodule. Existing clones: see the [README](https://github.com/BornToBeRoot/NETworkManager/tree/main/Source/3rdparty/Dragablz#existing-clones-with-the-old-submodule) of the folder. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
