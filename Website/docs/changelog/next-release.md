---
sidebar_position: 0
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

- Firewall application has been added for adding NETworkManager controlled Windows firewall rules with profile support. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)

## Improvements

- Reuse existing validators and converters where applicable. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Support commands exceeding the commandline limit in PowershellHelper. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Fix warnings in NetworkInterfaceView. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Add various converters and validators [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Allow to click validation errors out of the way. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Add validation error template on checkboxes. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Allow style changes when ViewModels recognize configuration errors. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)

## Bug Fixes

- Fix null dereferences in various validators and converters. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
- Add code documentation in various places. [#3353](https://github.com/BornToBeRoot/NETworkManager/pull/3353)
- Refactor ListHelper.Modify as generic method.
