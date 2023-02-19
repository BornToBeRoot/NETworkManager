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

New Feature
{: .label .label-green }

- SNTP Lookup [#1821](https://github.com/BornToBeRoot/NETworkManager/pull/1821){:target="\_blank"} (See [documentation](https://borntoberoot.net/NETworkManager/Documentation/Application/SNTPLookup){:target="\_blank"} for more details)

## Improvements
- Move .NET runtime components and dependencies into a subfolder to simplify the installation/portable folder [#1832](https://github.com/BornToBeRoot/NETworkManager/pull/1832){:target="\_blank"}
- Reduce the size of the installer, portable and archive build [#1832](https://github.com/BornToBeRoot/NETworkManager/pull/1832){:target="\_blank"}
- DataGrid Column header design improved [#1910](https://github.com/BornToBeRoot/NETworkManager/pull/1910){:target="\_blank"}
- DataGrid Columns can now be resized [#1910](https://github.com/BornToBeRoot/NETworkManager/pull/1910){:target="\_blank"}
- IP Scanner
  - Max threads changed to to 1024 [#1927](https://github.com/BornToBeRoot/NETworkManager/pull/1927){:target="\_blank"}
- Port Scanner
  - Add new port profiles / improve existing ones [#1909](https://github.com/BornToBeRoot/NETworkManager/pull/1909){:target="\_blank"}
  - Max host threads changed to to 256 [#1927](https://github.com/BornToBeRoot/NETworkManager/pull/1927){:target="\_blank"}
  - Max port threads changed to to 1024 [#1927](https://github.com/BornToBeRoot/NETworkManager/pull/1927){:target="\_blank"}
- Ping
  - Allow multiple hosts like `server-01; 1.1.1.1; example.com` as input [#1933](https://github.com/BornToBeRoot/NETworkManager/pull/1933){:target="\_blank"}
  - Use a fixed size for the view [#1933](https://github.com/BornToBeRoot/NETworkManager/pull/1933){:target="\_blank"}
- Traceroute
  - Add more validation (host, ip address) to input [#1932](https://github.com/BornToBeRoot/NETworkManager/pull/1932){:target="\_blank"}  
- Add documentation for: [#265](https://github.com/BornToBeRoot/NETworkManager/pull/265){:target="\_blank"}
  - [Dashboard](https://borntoberoot.net/NETworkManager/Documentation/Application/Dashboard){:target="\_blank"}
  - [Network Interface](https://borntoberoot.net/NETworkManager/Documentation/Application/NetworkInterface){:target="\_blank"
  - [WiFi](https://borntoberoot.net/NETworkManager/Documentation/Application/WiFi){:target="\_blank"}
  - [IP Scanner](https://borntoberoot.net/NETworkManager/Documentation/Application/IPScanner){:target="\_blank"}

## Bugfixes
AWS Session Manager
  - Use UTF-8 encoding for embedded PowerShell console window [#1832](https://github.com/BornToBeRoot/NETworkManager/pull/1832){:target="\_blank"}

## Other
- Code cleanup [#1932](https://github.com/BornToBeRoot/NETworkManager/pull/1932){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
