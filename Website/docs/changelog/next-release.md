---
sidebar_position: 0
description: "Changelog for the next NETworkManager release — upcoming features, improvements, and bug fixes."
keywords: [NETworkManager, changelog, release notes, next release, upcoming features, bug fixes]
---

# Next Release

Version: **Next release** <br />
Release date: **xx.xx.2026**

| File | `SHA256` |
| ---- | -------- |

**System requirements**

- Windows 10 / Server x64 (22H2 or later)
- [.NET Desktop Runtime 10.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime)

## Breaking Changes

## What's new?

**Firewall**

- Firewall application has been added for adding NETworkManager controlled Windows firewall rules with profile support. Special thanks to [@labre-rdc](https://github.com/labre-rdc) [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)

**PowerShell**

- DPI scaling is now applied correctly when NETworkManager is moved to a monitor with a different DPI scaling factor. The embedded PowerShell (conhost) window now rescales its font automatically using the Windows Console API (`AttachConsole` + `SetCurrentConsoleFontEx`), bypassing the OS limitation that prevents `WM_DPICHANGED` from being forwarded to cross-process child windows. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

**PuTTY**

- DPI scaling is now applied correctly when NETworkManager is moved to a monitor with a different DPI scaling factor. The embedded PuTTY window now receives an explicit `WM_DPICHANGED` message with the new DPI value packed into `wParam`, since the OS does not forward this message across process boundaries after `SetParent`. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

## Improvements

- Reuse existing validators and converters where applicable. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Support commands exceeding the commandline limit in PowershellHelper. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Fix warnings in NetworkInterfaceView. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Add various converters and validators [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Allow to click validation errors out of the way. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Add validation error template on checkboxes. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Allow style changes when ViewModels recognize configuration errors. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)

## Bug Fixes

**General**

- Fix null dereferences in various validators and converters. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)

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
- Add code documentation in various places. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Refactor ListHelper.Modify as generic method.
