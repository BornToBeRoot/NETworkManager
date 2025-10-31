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

**Remote Desktop**

- Flag to enable `Admin (console) session` added to the RDP connect, profile and group dialogs. This flag allows connecting to the console session of the remote computer. [#3216](https://github.com/BornToBeRoot/NETworkManager/pull/3216)

## Improvements

**Remote Desktop**

- Redesign RDP connect dialog (migrated from dialog to child window). [#3216](https://github.com/BornToBeRoot/NETworkManager/pull/3216)

## Bug Fixes

- The new profile filter popup indroduced in version `2025.10.18.0` was instantly closed when a `PuTTY`, `PowerShell` or `AWS Session Manager` session was opened and the respective application / view was selected. [#3219](https://github.com/BornToBeRoot/NETworkManager/pull/3219)

## Dependencies, Refactoring & Documentation

- Documentation updated
- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
