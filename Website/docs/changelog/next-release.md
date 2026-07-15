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

- Windows 10 / Server x64 (22H2 or later)
- [.NET Desktop Runtime 10.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime)

## Breaking Changes

## What's new?

**Traceroute**

- New **Map** view below the hop list, visualizing each resolved hop's geolocation on an offline world map. Consecutive hops are connected with curved, directional arrows; hovering a marker shows its location, ISP/ASN, hostname, IP address and average round-trip time, while hovering an arrow shows the source and destination location of that segment. The map supports mouse-wheel zoom and drag-to-pan, and can be collapsed via a toggle button on the map itself, similar to the Profiles panel. The map is only shown if **Check IP geolocation** and the new **Show map** setting are both enabled, since hops need a resolved geolocation to be plotted. [#3520](https://github.com/BornToBeRoot/NETworkManager/pull/3520)

## Improvements

## Bug Fixes

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
