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
- [.NET Desktop Runtime 10.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime)

## Breaking Changes

- Migrated from .NET 8.0 (LTS) to .NET 10.0 (LTS).
  Upgrade your [.NET Desktop Runtime to version 10.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime) before you install this version. [#3229](https://github.com/BornToBeRoot/NETworkManager/pull/3229)

## What's new?

**Remote Desktop**

- Flag to enable `Admin (console) session` added to the RDP connect, profile and group dialogs. This flag allows connecting to the console session of the remote computer. [#3216](https://github.com/BornToBeRoot/NETworkManager/pull/3216)

## Improvements

**Profiles**

- Profile file creation flow improved — when adding a new profile you are now prompted to enable profile-file encryption to protect stored credentials and settings. [#3227](https://github.com/BornToBeRoot/NETworkManager/pull/3227)
- Profile file dialog migrated to a child window to improve usability. [#3227](https://github.com/BornToBeRoot/NETworkManager/pull/3227)
- Credential dialogs migrated to child windows to improve usability. [#3231](https://github.com/BornToBeRoot/NETworkManager/pull/3231)

**Remote Desktop**

- Redesign RDP connect dialog (migrated from dialog to child window). [#3216](https://github.com/BornToBeRoot/NETworkManager/pull/3216)

**PowerShell**

- Redesign PowerShell connect dialog (migrated from dialog to child window). [#3234](https://github.com/BornToBeRoot/NETworkManager/pull/3234)

**PuTTY**

- Redesign PuTTY connect dialog (migrated from dialog to child window). [#3234](https://github.com/BornToBeRoot/NETworkManager/pull/3234)

**Web Console**

- Redesign Web Console connect dialog (migrated from dialog to child window). [#3234](https://github.com/BornToBeRoot/NETworkManager/pull/3234)

## Bug Fixes

- The new profile filter popup introduced in version `2025.10.18.0` was instantly closed when a `PuTTY`, `PowerShell` or `AWS Session Manager` session was opened and the respective application / view was selected. [#3219](https://github.com/BornToBeRoot/NETworkManager/pull/3219)

## Dependencies, Refactoring & Documentation

- Documentation updated
- Code cleanup & refactoring
  - Introduced a new DialogHelper utility to centralize creation of `OK` and `OK/Cancel` dialogs. This reduces duplication and enforces a consistent layout. [#3231](https://github.com/BornToBeRoot/NETworkManager/pull/3231)
- Migrated existing credential and profile dialogs to use DialogHelper, improving usability, accessibility and maintainability across the app. [#3231](https://github.com/BornToBeRoot/NETworkManager/pull/3231)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
