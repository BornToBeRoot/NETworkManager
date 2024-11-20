---
sidebar_position: 0
---

# Next Release

Version: **Next release** <br />
Release date: **xx.xx.2024**

| File | `SHA256` |
| ---- | -------- |

**System requirements**

- Windows 10 / Server x64 (22H2 or later)
- [.NET Desktop Runtime 8.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime)

## Breaking Changes

- Minimum supported Windows version increased to `22H2`. [#2912](https://github.com/BornToBeRoot/NETworkManager/pull/2912)

## What's new?

- **WiFi**
  - 6 GHz networks are not supported. [#2912](https://github.com/BornToBeRoot/NETworkManager/pull/2912) [#2928](https://github.com/BornToBeRoot/NETworkManager/pull/2928)
  - `WPA3 Personal (SAE)`, `WPA3 Enterprise` and `WPA3 Enterprise (192-bit)` are now supported. [#2912](https://github.com/BornToBeRoot/NETworkManager/pull/2912)
  - `802.11be` (`EHT`) is now supported. [#2912](https://github.com/BornToBeRoot/NETworkManager/pull/2912)

## Improvements

## Bugfixes

- Changed the Welcome dialog from `MahApps.Metro.Controls.Dialogs` to `MahApps.Metro.SimpleChildWindow`, so the main window can be dragged and resized on the first start. [#2914](https://github.com/BornToBeRoot/NETworkManager/pull/2914)

- **WiFi**
  - Fixed a bug that caused the scan process to crash when a 6 GHz network was found. [#2912](https://github.com/BornToBeRoot/NETworkManager/pull/2912)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
