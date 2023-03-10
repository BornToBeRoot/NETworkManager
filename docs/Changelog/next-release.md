---
layout: default
title: Next release
parent: Changelog
nav_order: 1
description: "Changelog for next release"
permalink: /Changelog/next-release
---

Version: **Next release** <br />
Release date: **xx.xx.2023**

**System requirements**

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="\_blank"}

## What's new?

## Improvements
- (Re)loading of the profiles improved. Code has been optimized and user interface update calls have been reduced. WPF virtualizing explicitly activated. The scrollbar in the profile list is now always visible, because the size is calculated by WPF each time on mouse over. [#2014](https://github.com/BornToBeRoot/NETworkManager/pull/2014){:target="\_blank"}
- Reselect profile or select first profile after (re)loading or search [#2014](https://github.com/BornToBeRoot/NETworkManager/pull/2014){:target="\_blank"}
- Some designs have been improved (Error & Warning icons). [#2014](https://github.com/BornToBeRoot/NETworkManager/pull/2014){:target="\_blank"}

## Bugfixes
- Group name check is now case insensitive and a group name can only be used once. If you create a group named `Test`, you cannot create a group named `test`. [#2014](https://github.com/BornToBeRoot/NETworkManager/pull/2014){:target="\_blank"}
- Profile dialog
  - Validate AWS Session Manager input (instance ID, profile, region) [#2025](https://github.com/BornToBeRoot/NETworkManager/pull/2025){:target="\_blank"}

## Other
- Code cleanup [#2024](https://github.com/BornToBeRoot/NETworkManager/pull/2024){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
