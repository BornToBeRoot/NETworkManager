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

- Fixed the content of a tab staying visible without its tab after the tab was dragged into another window and out again quickly. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab dropped into an empty tab bar (or far behind the last tab) being dragged out again right away. A tab now leaves the tab bar only when it is dragged out of the whole tab bar. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed the tab title (e.g. the host of a scan) not being updated when the tab is in a dragged out window that is not the active window. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- The tab headers show the focus ring of the application (keyboard navigation) instead of a dotted rectangle. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- The other tabs no longer slide to their new position while a tab is dragged if animations are disabled (Settings > Appearance > Animations). [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab (and its session, e.g. PuTTY or Remote Desktop) being lost when it was dragged out as a window after a previous drag had merged a tab into another window: the drop zone of the earlier drag was reused. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a crash while dragging a tab as a window when another tab window closed at the same time (e.g. its session ended). [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab dragged back to its original position while the other tabs were still sliding ending up at the wrong position. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab dropped into another tab bar being inserted at the wrong position with display scaling above 100 %. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed the splitter between tabs split on top of each other also reacting 3 px into the tabs on the left and right. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)

**About**

- Fixed the license of ControlzEx in the library list: it showed the license text and link of Dragablz. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Updated the license of MahApps.Metro in the library list (copyright: .NET Foundation and Contributors). [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Dragablz (tab control) is now part of the repository (`Source/3rdparty/Dragablz`, stripped to the code we use) instead of a git submodule. Existing clones: see the [README](https://github.com/BornToBeRoot/NETworkManager/tree/main/Source/3rdparty/Dragablz#existing-clones-with-the-old-submodule) of the folder. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
