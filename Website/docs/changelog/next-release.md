---
sidebar_position: 0
description: "Changelog for the next NETworkManager release — upcoming features, improvements, and bug fixes."
keywords:  [NETworkManager, changelog, release notes, next release, upcoming features, bug fixes]
---

# Next Release

Version: **Next release** <br />
Release date: **xx.xx.2026**

| File | `SHA256` |
| ---- | -------- |

**System requirements**

- Windows 10 / Server x64/ARM64 (22H2 or later)
- [.NET Desktop Runtime 10.0 (LTS) - x64/ARM64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime)

## Breaking Changes

## What's new?

## Improvements

## Bug Fixes

**Tabs**

- Fixed the tab title (e.g. the host of a scan) not being updated when the tab is in a dragged out window that is not the active window. [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)
- The tab headers show the focus ring of the application (keyboard navigation) instead of a dotted rectangle. [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)
- The other tabs no longer slide to their new position while a tab is dragged if animations are disabled (Settings > Appearance > Animations). [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)
- Fixed a tab (and its session, e.g. PuTTY or Remote Desktop) being lost when it was dragged out as a window after a previous drag had merged a tab into another window: the drop zone of the earlier drag was reused. [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)
- Fixed a crash while dragging a tab as a window when another tab window closed at the same time (e.g. its session ended). [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)
- Fixed a tab dragged back to its original position while the other tabs were still sliding ending up at the wrong position. [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)
- Fixed a tab dropped into another tab bar being inserted at the wrong position with display scaling above 100 %. [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)
- Fixed the splitter between tabs split on top of each other also reacting 3 px into the tabs on the left and right. [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)

**About**

- Fixed the license of ControlzEx in the library list: it showed the license text and link of Dragablz. [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)
- Updated the license of MahApps.Metro in the library list (copyright: .NET Foundation and Contributors). [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Dragablz (tab control) is now part of the repository (`Source/3rdparty/Dragablz`, stripped to the code we use) instead of a git submodule. Existing clones: see the [README](https://github.com/BornToBeRoot/NETworkManager/tree/main/Source/3rdparty/Dragablz#existing-clones-with-the-old-submodule) of the folder. [#xxxx](https://github.com/BornToBeRoot/NETworkManager/pull/xxxx)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
