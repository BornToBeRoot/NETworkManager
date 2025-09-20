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

- Profile's can now be filtered by tags. Next to the search textbox, a new filter button is available. [#3144](https://github.com/BornToBeRoot/NETworkManager/pull/3144)

## Improvements

- Redesign settings reset dialog. [#3138](https://github.com/BornToBeRoot/NETworkManager/pull/3138)

**WiFi**

- Documentation for Windows 11 24H2 location permission added. [#3148](https://github.com/BornToBeRoot/NETworkManager/pull/3148)

## Bugfixes

- **Web Console**
  - Fixed an issue where clearing the Browser cache crashed the application. [#3169](https://github.com/BornToBeRoot/NETworkManager/pull/3169)

- **Profiles**
  - Fixed an issue where only one profile was deleted in `Settings > Profiles` when multiple profiles were selected. [#3144](https://github.com/BornToBeRoot/NETworkManager/pull/3144) [#3145](https://github.com/BornToBeRoot/NETworkManager/issues/3145)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
- Gurubase Widget removed [#3059](https://github.com/BornToBeRoot/NETworkManager/pull/3059)
