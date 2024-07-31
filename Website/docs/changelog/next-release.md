---
sidebar_position: 0
---

# Next Release

Version: **Next release** <br />
Release date: **xx.xx.2024**

| File | `SHA256` |
| ---- | -------- |

**System requirements**

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 8.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime)

## Breaking Changes

## What's new?

- Applications can now be sorted via drag & drop in the application sidebar or in the settings under `Settings > General > Applications`. [#2781](https://github.com/BornToBeRoot/NETworkManager/pull/2781)

## Improvements

- Applications settings under `Settings > General > Applications` redesigned and context menu added (Right-click) [#2781](https://github.com/BornToBeRoot/NETworkManager/pull/2781)
  - `Set default` added (Set the startup application - available if not set)
  - `Show` added (Shows the application in the main window - available if hidden)
  - `Hide` added (Hides the application from the main window - available if shown)

## Bugfixes

- TextBox content not centered because of ScrollViewer issue. [#2763](https://github.com/BornToBeRoot/NETworkManager/pull/2763)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
