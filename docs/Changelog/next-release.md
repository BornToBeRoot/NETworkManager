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
- [.NET Desktop Runtime 8.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime){:target="\_blank"}

## Breaking Changes

## What's new?

- Ping Monitor
  - UI redesigned [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
  - Allow ranges like `192.168.0.0/24` or `10.0.[0-255].1` [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}

## Experimental features

- Run command (HotKey: Ctrl+Shift+P) added. This feature allows you to open a command field to switch between applications and pass parameters to them.

{: .note}
Experimental features can be enabled in the settings under [`Settings > Update`](https://borntoberoot.net/NETworkManager/Documentation/Settings/Update#experimental-features){:target="\_blank"}.

## Improvements

- IP Scanner
  - Scan is no longer aborted if the IP of a single host in a series of hosts cannot be resolved (the host is skipped and an error is displayed) [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
- Port Scanner
  - Scan is no longer aborted if the IP of a single host in a series of hosts cannot be resolved (the host is skipped and an error is displayed) [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
  - Hostname added to group [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
- Ping Monitor
  - Scan is no longer aborted if the IP of a single host in a series of hosts cannot be resolved (the host is skipped and an error is displayed) [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
- DNS Lookup
  - Hostname of the nameserver added to group [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}

## Bugfixes

## Other

- Code cleanup & Refactoring
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
