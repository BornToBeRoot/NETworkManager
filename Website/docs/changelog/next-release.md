---
sidebar_position: 0
---

# Next Release

Version: **Next release** <br />
Release date: **xx.xx.2023**

| File | `SHA256` |
| ---- | -------- |

**System requirements**

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 8.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime)

## Breaking Changes

## What's new?

**Network Interfaces**

- Adding an additional IPv4 address to the network adapter will enable the `netsh` option `dhcpstaticipcoexistence` if the network adapter is configured to use DHCP [#2656](https://github.com/BornToBeRoot/NETworkManager/pull/2656)

## Improvements

- **Network Interfaces**

  - Export to CSV, XML and JSON added [#2626](https://github.com/BornToBeRoot/NETworkManager/pull/2626)

- **Ping Monitor**

  - Grouping of hosts added. Hosts are now grouped based on the profile or added to the default group [#2645](https://github.com/BornToBeRoot/NETworkManager/pull/2645)

- **Discovery Protocol**

  - Export to CSV, XML and JSON added [#2626](https://github.com/BornToBeRoot/NETworkManager/pull/2626)

- **IP Geolocation**

  - Export to CSV, XML and JSON added [#2626](https://github.com/BornToBeRoot/NETworkManager/pull/2626)

- **Bit Calculator**

  - Export to CSV, XML and JSON added [#2626](https://github.com/BornToBeRoot/NETworkManager/pull/2626)

## Bugfixes

- **General**

  - Fix some design issues for the `Run command` button. The button is now in the right-hand title bar instead of in the center until a better solution is found [#2651](https://github.com/BornToBeRoot/NETworkManager/pull/2651)

- **IP Scanner**

  - `Add profile...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)
  - `Export...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)
  - Copy MAC address to clipboard fixed [#2644](https://github.com/BornToBeRoot/NETworkManager/pull/2644)

- **Port Scanner**

  - `Open port profiles...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)
  - `Export...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)

- **Traceroute**

  - `Export...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)

- **DNS Lookup**

  - `Export...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)

- **SNMP**

  - `Open OID profiles...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)
  - `Export...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)

- **SNTP Lookup**

  - `Export...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)

- **Whois**

  `Export...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)

- **IP Geolocation**

  - `Export...` in dragged out tab fixed [#2661](https://github.com/BornToBeRoot/NETworkManager/pull/2661)

## Dependencies, Refactoring & Documentation

- Code cleanup & Refactoring
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
