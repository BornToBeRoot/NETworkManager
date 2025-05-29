---
sidebar_position: 0
---

# Next Release

Version: **Next release** <br />
Release date: **xx.xx.2025**

| File | `SHA256` |
| ---- | -------- |

**System requirements**

- Windows 10 / Server x64 (22H2 or later)
- [.NET Desktop Runtime 8.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime)

## Breaking Changes

## What's new?

- New language Japanese (`ja-JP`) has been added. Thanks to [@coolvitto](https://github.com/coolvitto) [#3044](https://github.com/BornToBeRoot/NETworkManager/pull/3044) [#3030](https://github.com/BornToBeRoot/NETworkManager/pull/3030) [#3035](https://github.com/BornToBeRoot/NETworkManager/pull/3035)

**Hosts File Editor**

- New feature to display (and edit) the `hosts` file. See [Hosts File Editor]() for more details.

  :::info

  This feature is currently in read-only mode. Editing will be enabled / implemented in a future release. Please report any issues you encounter on the [GitHub issue tracker](https://github.com/BornToBeRoot/NETworkManager/issues)

  :::

**DNS Lookup**

- `DNSKEY` records are now supported. [#3060](https://github.com/BornToBeRoot/NETworkManager/pull/3060)
- `SRV` records are now supported. [#3060](https://github.com/BornToBeRoot/NETworkManager/pull/3060)
- `CAA` records are now supported. [#3012](https://github.com/BornToBeRoot/NETworkManager/pull/3012)

## Improvements

**WiFi**

- Redesign refresh button/view. [#3012](https://github.com/BornToBeRoot/NETworkManager/pull/3012)

**DNS Lookup**

- Record types that are not implemented are now hidden in the user interface. [#3012](https://github.com/BornToBeRoot/NETworkManager/pull/3012)

**PowerShell**

- Find `pwsh.exe` and `powershell.exe` executable by path, similar to `where.exe`. [#2962](https://github.com/BornToBeRoot/NETworkManager/pull/2962)

**PuTTY**

- Find `putty.exe` executable by path, similar to `where.exe`. [#2962](https://github.com/BornToBeRoot/NETworkManager/pull/2962)

**AWS Session Manager**

- Find `pwsh.exe` and `powershell.exe` executable by path, similar to `where.exe`. [#2962](https://github.com/BornToBeRoot/NETworkManager/pull/2962)

**Connections**

- Redesign refresh button/view. [#3012](https://github.com/BornToBeRoot/NETworkManager/pull/3012)

**Listeners**

- Redesign refresh button/view. [#3012](https://github.com/BornToBeRoot/NETworkManager/pull/3012)

**ARP**

- Redesign refresh button/view. [#3012](https://github.com/BornToBeRoot/NETworkManager/pull/3012)

**Profiles**

- Changed the unlock dialog from `MahApps.Metro.Controls.Dialogs` to `MahApps.Metro.SimpleChildWindow`, so the main window can be dragged and resized [#3010](https://github.com/BornToBeRoot/NETworkManager/pull/3010)

## Bugfixes

**Network Interface**

- Re-select the network interface after a network change or configuration update. Thanks to [@Ghislain1](https://github.com/Ghislain1) [#3004](https://github.com/BornToBeRoot/NETworkManager/pull/3004) [#2962](https://github.com/BornToBeRoot/NETworkManager/pull/2962)

**WiFi**

- Fix filter by frequency if search is empty. [#3012](https://github.com/BornToBeRoot/NETworkManager/pull/3012)
- Reload animation fixed in some cases. [#3012](https://github.com/BornToBeRoot/NETworkManager/pull/3012)

**PowerShell**

- Set PowerShell console color for correct path... [#3023](https://github.com/BornToBeRoot/NETworkManager/pull/3023)

**SNMP**

- Fixed an issue where the ConsoleTextBox is not displayed correctly. [#3012](https://github.com/BornToBeRoot/NETworkManager/pull/3012)

**Whois**

- Fixed an issue where the ConsoleTextBox is not displayed correctly. [#3012](https://github.com/BornToBeRoot/NETworkManager/pull/3012)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
- Gurubase Widget removed [#3059](https://github.com/BornToBeRoot/NETworkManager/pull/3059)
