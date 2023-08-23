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

Breaking Changes
{: .label .label-red }

New Feature
{: .label .label-green }

## Breaking Changes

- Profiles and settings migration (indroduced in `2023.3.7.0`) from `%AppData%\NETworkManager` to `%UserProfile%\Documents\NETworkManager` removed. If you use a version before `2023.3.7.0` you have to install and start version `2023.6.27.1` before you install this version. [#2380](https://github.com/BornToBeRoot/NETworkManager/pull/2380){:target="\_blank"}
- Remove profile migration script to migrate from `2021.11.30.0` and before to a later version [#2388](https://github.com/BornToBeRoot/NETworkManager/pull/2388){:target="\_blank"}

## What's new?

- Dashboard
  - Check IP geolocation added [#2392](https://github.com/BornToBeRoot/NETworkManager/pull/2392){:target="\_blank"}
  - Check DNS resolver added [#2392](https://github.com/BornToBeRoot/NETworkManager/pull/2392){:target="\_blank"}

## Improvements

## Bugfixes

- Network Interface
  - Add missing scrollviewer in configure tab [#2410](https://github.com/BornToBeRoot/NETworkManager/pull/2410){:target="\_blank"}
- Profiles
  - Prevent the application from crashing if a profile file cannot be loaded (profile management is then blocked) [#2464](https://github.com/BornToBeRoot/NETworkManager/pull/2464){:target="\_blank"}

## Other

- Code cleanup [#2388](https://github.com/BornToBeRoot/NETworkManager/pull/2388){:target="\_blank"} [#2392](https://github.com/BornToBeRoot/NETworkManager/pull/2392){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
