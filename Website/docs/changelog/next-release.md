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

- Removed the **ThreadPool additional min. threads** setting (Settings > General) and the application-wide `ThreadPool.SetMinThreads` workaround it configured for the IP Scanner and Port Scanner. It's no longer needed now that both scan engines use non-blocking async concurrency instead of blocking calls. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)

## What's new?

- ARM64 builds are now available. [#3538](https://github.com/BornToBeRoot/NETworkManager/issues/3538)

**Traceroute**

- New **Map** view below the hop list, visualizing each resolved hop's geolocation on an offline world map. Consecutive hops are connected with curved, directional arrows; hovering a marker shows its location, ISP/ASN, hostname, IP address and average round-trip time, while hovering an arrow shows the source and destination location of that segment. The map supports mouse-wheel zoom and drag-to-pan, and can be collapsed via a toggle button on the map itself, similar to the Profiles panel. The map is only shown if **Check IP geolocation** and the new **Show map** setting are both enabled, since hops need a resolved geolocation to be plotted. [#3520](https://github.com/BornToBeRoot/NETworkManager/pull/3520)

## Improvements

- The collapsed/expanded state of profile groups (e.g. **linux-server**) is now remembered per profile file and shared across all tools, instead of resetting every time you switch tools or restart the application. [#3539](https://github.com/BornToBeRoot/NETworkManager/pull/3539)

**IP Scanner**

- Added a live count of hosts up/down next to the **Result** header. Stays visible after the scan finishes. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Host input now accepts newline-separated hosts (one per line, e.g. a column pasted from Excel), converted automatically to the semicolon-separated form. Thanks to [@dearmb](https://github.com/dearmb) [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- Host input now supports shorthand IPv4 ranges like `192.168.0.1-100`, in addition to the existing `192.168.0.0-192.168.0.100` and `192.168.[0-100].1` range formats. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- Added `135` (RPC) and `9100` (raw printing) to the default **Ports** list used to detect if a host is reachable. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Reduced the default **Max. concurrent port threads** from `5` to `4`. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Reduced the default **Max. concurrent host threads** from `256` to `64`, a more conservative default that puts less simultaneous load on the scanned network. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)

**Port Scanner**

- Added a port status icon column to the results, matching the port icon used in the IP Scanner's extended port info, to indicate at a glance whether a port is open (green) or closed (red). [#3558](https://github.com/BornToBeRoot/NETworkManager/issues/3558)
- Added a live count of ports open/closed next to the **Result** header. Stays visible after the scan finishes. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Host and Ports input fields now accept newline-separated entries (one per line, e.g. a column pasted from Excel), converted automatically to the semicolon-separated form. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- Host input now supports shorthand IPv4 ranges like `192.168.0.1-100`, in addition to the existing `192.168.0.0-192.168.0.100` and `192.168.[0-100].1` range formats. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- Reduced the default **Max. concurrent host threads** from `5` to `4`. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Reduced the default **Max. concurrent port threads** from `256` to `64`, a more conservative default that puts less simultaneous load on the scanned host. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Added a new **Well-known ports** (`1-1024`) default port profile. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)

**Ping Monitor**

- Added a live count of hosts up/down (and paused, if any) per group, next to the group's close button. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Added **Start** and **Pause** buttons to each group header (shown next to the close button on mouse-over), to start every paused host in the group or pause every running one at once. [#XXXX](https://github.com/BornToBeRoot/NETworkManager/pull/XXXX)
- Reworked each monitored host's card: the collapsed quick info now shows labeled values (`Received: X · Lost: Y · Packet loss: Z%`) instead of unlabeled numbers, the expanded view shows only the latency chart (with more room, since Hostname/IP address are already visible in the header) and the last status change time moved to a tooltip on the connectivity icon. The **Status change** field was also renamed to **Last status change**. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Host input now accepts newline-separated hosts (one per line, e.g. a column pasted from Excel), converted automatically to the semicolon-separated form. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- Host input now supports shorthand IPv4 ranges like `192.168.0.1-100`, in addition to the existing `192.168.0.0-192.168.0.100` and `192.168.[0-100].1` range formats. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)

## Bug Fixes

**Dashboard**

- Fixed the **Network Connection** widget running a full connection check on every application startup, even if the Status Window or Dashboard was never opened. It now only checks when actually shown - via the tray icon, on a network change, or when the Dashboard tab is opened. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Fixed the DNS status (Computer/Router/Internet) in the **Network Connection** widget showing as an error when no PTR record exists for the address, which is common and expected for private IP ranges. This is now shown as informational instead of critical. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Fixed a race condition in the **Network Connection** widget where results from a superseded check could overwrite the results of a newer, still-running check after quickly reopening the widget. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)

**IP Scanner**

- Fixed NetBIOS lookups (computer name, domain/workgroup, user name) not starting until a host's entire port scan had finished, since the port scan wasn't actually running asynchronously despite being awaited alongside it. Ping, port scan, and NetBIOS resolution now genuinely run concurrently for every host. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Fixed the application becoming unresponsive (including the window not reacting to input) during a large scan (e.g. a /24). Scan results and progress were both being pushed to the UI one item/update at a time via a dispatcher call per host/port, which could flood the UI thread's message queue on large scans. Both are now batched and flushed periodically (every 150ms) instead, for **IP Scanner** and **Port Scanner** alike. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Converted the **IP Scanner** and **Port Scanner** engines from `Parallel.ForEach` with blocking calls to genuinely asynchronous `Parallel.ForEachAsync`, removing the risk of exhausting the application's ThreadPool at high concurrency settings. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Refactored the **Network Connection** widget's check logic to reduce duplicated code and share the local IP address detection between the Computer and Router checks instead of detecting it twice. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Consolidated the duplicated **Profiles** side panel (search, tag filter, grouped list, context menu, add/edit/copy/delete) used across 15 tool views into a single shared control, reducing code duplication. As part of this, the profile panel's expanded/width state is now shared across all tools instead of being tracked per tool. [#3537](https://github.com/BornToBeRoot/NETworkManager/pull/3537)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
