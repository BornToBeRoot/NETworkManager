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

**Profiles**

- Added the ability to filter profiles by tags using the filter button next to the search field. [#3144](https://github.com/BornToBeRoot/NETworkManager/pull/3144) [#3187](https://github.com/BornToBeRoot/NETworkManager/pull/3187) [#3194](https://github.com/BornToBeRoot/NETworkManager/pull/3194)
- Introduced a context menu for profile groups to collapse or expand all groups at once. [#3171](https://github.com/BornToBeRoot/NETworkManager/pull/3171)

## Improvements

- Improved light theme for enhanced readability and contrast. [#3191](https://github.com/BornToBeRoot/NETworkManager/pull/3191)
- Redesigned the settings reset dialog for a clearer and more user-friendly experience. [#3138](https://github.com/BornToBeRoot/NETworkManager/pull/3138)

**WiFi**

- Added documentation for Windows 11 24H2 location permission requirements. [#3148](https://github.com/BornToBeRoot/NETworkManager/pull/3148)

## Bug Fixes

- Fixed an issue where the status window was out of screen when a display scale other than 100% was set. [#3185](https://github.com/BornToBeRoot/NETworkManager/pull/3185)

**Web Console**

- Fixed a crash that occurred when clearing the browser cache. [#3169](https://github.com/BornToBeRoot/NETworkManager/pull/3169)

**Profiles**

- Fixed an issue where only one profile was deleted in `Settings > Profiles` when multiple profiles were selected. [#3144](https://github.com/BornToBeRoot/NETworkManager/pull/3144) [#3145](https://github.com/BornToBeRoot/NETworkManager/issues/3145)

## Dependencies, Refactoring & Documentation

- Documentation updated [#3187](https://github.com/BornToBeRoot/NETworkManager/pull/3187)
- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot) [#3190](https://github.com/BornToBeRoot/NETworkManager/pull/3190)
- Gurubase Widget removed [#3059](https://github.com/BornToBeRoot/NETworkManager/pull/3059)
