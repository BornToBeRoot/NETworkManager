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

- If you are upgrading from a version older than or equal to `2022.12.20.0`, first install [`2024.11.11.0`](https://github.com/BornToBeRoot/NETworkManager/releases/tag/2024.11.11.0) to upgrade the settings. [#2962](https://github.com/BornToBeRoot/NETworkManager/pull/2962).
- Minimum supported Windows version increased to `22H2` to support:
  - WiFi 6 GHz, WPA3, 802.11be [#2912](https://github.com/BornToBeRoot/NETworkManager/pull/2912)
  - Remote Desktop high DPI, scaling and fast resizing [#2968](https://github.com/BornToBeRoot/NETworkManager/pull/2968)

## What's new?

- **WiFi**

  - 6 GHz networks are now supported. [#2912](https://github.com/BornToBeRoot/NETworkManager/pull/2912) [#2928](https://github.com/BornToBeRoot/NETworkManager/pull/2928)
  - `WPA3 Personal (SAE)`, `WPA3 Enterprise` and `WPA3 Enterprise (192-bit)` are now supported. [#2912](https://github.com/BornToBeRoot/NETworkManager/pull/2912)
  - `802.11be` (`EHT`) is now supported. [#2912](https://github.com/BornToBeRoot/NETworkManager/pull/2912)

- **Remote Desktop**

  - Scale rdp session and control to support high DPI (e.g. per Monitor DPI like 125%, 150%, etc.). [#2968](https://github.com/BornToBeRoot/NETworkManager/pull/2968)
  - Resizing now uses [`IMsRdpClient9::UpdateSessionDisplaySettings`](<https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/mt703457(v=vs.85)>) instead of [`IMsRdpClient::Reconnect`](https://learn.microsoft.com/en-us/windows/win32/termserv/imsrdpclient8-reconnect) to support scaling and faster resizing (without the need of reconnecting). [#2968](https://github.com/BornToBeRoot/NETworkManager/pull/2968).

  :::warning

  The new features for high DPI, scaling and resizing may cause issues or doesn't work with legacy servers/clients. Please report any issues you find here: [#2911](https://github.com/BornToBeRoot/NETworkManager/issues/2911)

  :::

## Improvements

- Improve ToolTips (e.g. migrate from Twitter to X, etc.), Buttons, etc. [#2955](https://github.com/BornToBeRoot/NETworkManager/pull/2955)
- **WiFi**
  - Improve search, cleanup/remove some converters to make the code more readable and faster. [#2940](https://github.com/BornToBeRoot/NETworkManager/pull/2940)

## Bugfixes

- Horizontal scrollbar fixed for some views. [#2945](https://github.com/BornToBeRoot/NETworkManager/pull/2945)
- Fixed an issue with DPI scaling where the application was blurry if a second monitor had a different dpi setting than the main monitor. [#2941](https://github.com/BornToBeRoot/NETworkManager/pull/2941)
- Changed the Welcome dialog from `MahApps.Metro.Controls.Dialogs` to `MahApps.Metro.SimpleChildWindow`, so the main window can be dragged and resized on the first start. [#2914](https://github.com/BornToBeRoot/NETworkManager/pull/2914)

- **WiFi**

  - Fixed a bug that caused the scan process to crash when a 6 GHz network was found. [#2912](https://github.com/BornToBeRoot/NETworkManager/pull/2912)

- **IP Scanner**

  - Fixed two `NullReferenceException` in ICMP & NETBIOS for some IP addresses. [#2964](https://github.com/BornToBeRoot/NETworkManager/pull/2964)

## Dependencies, Refactoring & Documentation

- Migrated code for some loading indicators from the library [LoadingIndicators.WPF] (https://github.com/zeluisping/LoadingIndicators.WPF) to the NETworkManager repo, as the original repo looks unmaintained and has problems with MahApps.Metro version 2 and later. [#2963](https://github.com/BornToBeRoot/NETworkManager/pull/2963)
- Code cleanup & refactoring [#2940](https://github.com/BornToBeRoot/NETworkManager/pull/2940) [#2976](https://github.com/BornToBeRoot/NETworkManager/pull/2976)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
