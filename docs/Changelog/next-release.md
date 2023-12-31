---
layout: default
title: Next release
parent: Changelog
nav_order: 1
description: "Changelog for next release"
permalink: /Changelog/next-release
---

Version: **Next release** | Release date: **xx.xx.2023**

**System requirements**

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 8.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime){:target="\_blank"}

New Feature
{: .label .label-green }

## Breaking Changes

## What's new?

- Run Command
  - Run command (HotKey: Ctrl+Shift+P) added. This feature allows you to open a command field to switch between applications (and pass parameters to them _in the future_) [#2577](https://github.com/BornToBeRoot/NETworkManager/pull/2577){:target="\_blank"}
- IP Scanner
  - NetBIOS resolver added to enhance the detection of hosts (e.g. if there is no ICMP response or TCP port is open) but netbios responds. It will also display the Computername if no DNS hostname is available. [#2590](https://github.com/BornToBeRoot/NETworkManager/pull/2590){:target="\_blank"}
- Ping Monitor
  - UI redesigned [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
  - Allow ranges like `192.168.0.0/24` or `10.0.[0-255].1` [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
- Connections
  - Resolve remote hostname, process id, process name and process path (similar to `netstat`) [#2587](https://github.com/BornToBeRoot/NETworkManager/pull/2587){:target="\_blank"}

## Improvements

- IP Scanner
  - Scan is no longer aborted if the IP of a single host in a series of hosts cannot be resolved (the host is skipped and an error is displayed) [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
  - Sort by IP address & MAC address improved / fixed [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"}
- Port Scanner
  - Scan is no longer aborted if the IP of a single host in a series of hosts cannot be resolved (the host is skipped and an error is displayed) [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
  - Hostname added to group [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
  - Sort improved [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"}
  - Port profile column sort fixed [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"}
- Ping Monitor
  - Scan is no longer aborted if the IP of a single host in a series of hosts cannot be resolved (the host is skipped and an error is displayed) [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
- DNS Lookup
  - Hostname of the nameserver added to group [#2573](https://github.com/BornToBeRoot/NETworkManager/pull/2573){:target="\_blank"}
  - Sort improved [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"}
- SNMP
  OID profile column sort fixed [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"}
- Connections
  - Sort by IP address improved / fixed [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"}
- Listeners
  - Sort by IP address improved / fixed [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"}
- ARP Table
  - Sort by IP address & MAC address improved / fixed [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"}

## Bugfixes

- SNMP
  - Detect EndOfMibView in v1 and v2c walk [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"}
- Lookup
  - Fix sort by MAC address [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"}

## Other

- Code cleanup & Refactoring [#2583](https://github.com/BornToBeRoot/NETworkManager/pull/2583){:target="\_blank"} [#2586](https://github.com/BornToBeRoot/NETworkManager/pull/2586){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
