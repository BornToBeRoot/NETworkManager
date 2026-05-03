---
sidebar_position: 0
description: "Changelog for the next NETworkManager release — upcoming features, improvements, and bug fixes."
keywords:
  [
    NETworkManager,
    changelog,
    release notes,
    next release,
    upcoming features,
    bug fixes,
  ]
---

# Next Release

Version: **Next release** <br />
Release date: **xx.xx.2025**

| File | `SHA256` |
| ---- | -------- |

**System requirements**

- Windows 10 / Server x64 (22H2 or later)
- [.NET Desktop Runtime 10.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime)

## Breaking Changes

- **ARP Table** has been renamed to **[Neighbor Table](../application/neighbor-table.md)**. The application list entry is automatically migrated on first launch. Other view settings (auto-refresh interval, export file type/path) reset to their defaults. [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)
- **IP Scanner** export: The `ARPMACAddress` and `ARPVendor` columns have been removed from CSV, XML and JSON exports. Use `MACAddress` and `Vendor` instead, which contain the same value (ARP/NDP preferred, NetBIOS as fallback). [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)

## What's new?

**[Neighbor Table](../application/neighbor-table.md)** (formerly ARP Table)

- IPv6 (NDP) neighbor entries are now shown in addition to IPv4 (ARP). [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)
- New **Interface** and **State** columns (sortable, searchable). [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)
- Add entry dialog now accepts IPv4 and IPv6 addresses. [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)
- View is read-only when not running elevated; modifying the table requires elevated rights. [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)

**Firewall**

- New feature to quickly add, edit, enable, disable and delete NETworkManager-owned firewall rules. Managed rules are prefixed with `NETworkManager_` in the Windows Firewall. (See the [documentation](https://borntoberoot.net/NETworkManager/docs/application/firewall) for more details) [#3383](https://github.com/BornToBeRoot/NETworkManager/pull/3383)

**PowerShell**

- DPI scaling is now applied correctly when NETworkManager is moved to a monitor with a different DPI scaling factor. The embedded PowerShell (conhost) window now rescales its font automatically using the Windows Console API (`AttachConsole` + `SetCurrentConsoleFontEx`), bypassing the OS limitation that prevents `WM_DPICHANGED` from being forwarded to cross-process child windows. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

**PuTTY**

- DPI scaling is now applied correctly when NETworkManager is moved to a monitor with a different DPI scaling factor. The embedded PuTTY window now receives an explicit `WM_DPICHANGED` message with the new DPI value packed into `wParam`, since the OS does not forward this message across process boundaries after `SetParent`. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

## Improvements

**IP Scanner**

- MAC address resolution now uses ARP (IPv4) or NDP (IPv6) from the neighbor cache, with NetBIOS as fallback. The detail panel shows a single **MAC Address** section instead of separate ARP and NetBIOS entries. [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)

**Dashboard**

- Redesign Status Window to make it more compact. [#3359](https://github.com/BornToBeRoot/NETworkManager/pull/3359)

**Network Interface**

- Added Network Profile (domain, private, public) information to the Network Interface details view, if available. [#3383](https://github.com/BornToBeRoot/NETworkManager/pull/3383)

**Discovery Protocol**

- Added support for `F5` and `Enter` keys to start capturing network packets. [#3383](https://github.com/BornToBeRoot/NETworkManager/pull/3383)
- Redesigned the "restart as admin" note to be more compact and visually consistent. [#3383](https://github.com/BornToBeRoot/NETworkManager/pull/3383)

**Hosts File Editor**

- Button to open the hosts file in the default text editor added. [#3383](https://github.com/BornToBeRoot/NETworkManager/pull/3383)

## Bug Fixes

**Port Scanner**

- Fixed an app crash when double-clicking a port profile. [#3382](https://github.com/BornToBeRoot/NETworkManager/pull/3382)

**PowerShell**

- Fixed incorrect initial embedded window size on high-DPI monitors. The `WindowsFormsHost` panel now sets its initial dimensions in physical pixels using the current DPI scale factor, ensuring the PowerShell window fills the panel correctly at startup. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

**PuTTY**

- Fixed incorrect initial embedded window size on high-DPI monitors. The `WindowsFormsHost` panel now sets its initial dimensions in physical pixels using the current DPI scale factor, ensuring the PuTTY window fills the panel correctly at startup. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

**TigerVNC**

- Fixed incorrect initial embedded window size on high-DPI monitors. The `WindowsFormsHost` panel now sets its initial dimensions in physical pixels using the current DPI scale factor, ensuring the TigerVNC window fills the panel correctly at startup. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
