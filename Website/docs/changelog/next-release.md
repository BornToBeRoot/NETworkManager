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

## Improvements

**PowerShell**

- Find `pwsh.exe` and `powershell.exe` executable by path, similar to `where.exe`. [#2962](https://github.com/BornToBeRoot/NETworkManager/pull/2962)

**PuTTY**

- Find `putty.exe` executable by path, similar to `where.exe`. [#2962](https://github.com/BornToBeRoot/NETworkManager/pull/2962)

**AWS Session Manager**

- Find `pwsh.exe` and `powershell.exe` executable by path, similar to `where.exe`. [#2962](https://github.com/BornToBeRoot/NETworkManager/pull/2962)

**Profiles**

- Changed the unlock dialog from `MahApps.Metro.Controls.Dialogs` to `MahApps.Metro.SimpleChildWindow`, so the main window can be dragged and resized [#3010](https://github.com/BornToBeRoot/NETworkManager/pull/3010)

## Bugfixes

**Network Interface**

- Re-select the network interface after a network change or configuration update. Thanks to [@Ghislain1](https://github.com/Ghislain1) [#3004](https://github.com/BornToBeRoot/NETworkManager/pull/3004) [#2962](https://github.com/BornToBeRoot/NETworkManager/pull/2962)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
