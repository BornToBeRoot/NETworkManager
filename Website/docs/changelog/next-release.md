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

**RDAP**

- New [**RDAP**](../application/rdap.md) tool to look up registration data via the Registration Data Access Protocol, the standardized, JSON-based successor of Whois. Supports domains, top-level domains, IP addresses/CIDR prefixes, AS numbers and entity handles. The RDAP server is determined automatically using the IANA bootstrap files, which are cached locally and can be updated in the new **Settings > RDAP** page. Results are shown in a structured view (details, status, events, nameservers, DNSSEC, contacts, notices) or as raw JSON, and can be exported as JSON or text. For domains (e.g. `.com`/`.net`), the referral to the registrar's RDAP server is followed as well (can be disabled in **Settings > RDAP**). Includes profile support. If no RDAP server is known for a domain, it can be opened in **Whois** with a single click. [#3607](https://github.com/BornToBeRoot/NETworkManager/pull/3607)

**Traceroute**

- New **Map** view below the hop list, visualizing each resolved hop's geolocation on an offline world map. Consecutive hops are connected with curved, directional arrows; hovering a marker shows its location, ISP/ASN, hostname, IP address and average round-trip time, while hovering an arrow shows the source and destination location of that segment. The map supports mouse-wheel zoom and drag-to-pan, and can be collapsed via a toggle button on the map itself, similar to the Profiles panel. The map is only shown if **Check IP geolocation** and the new **Show map** setting are both enabled, since hops need a resolved geolocation to be plotted. [#3520](https://github.com/BornToBeRoot/NETworkManager/pull/3520)

**Settings**

- New global **DNS suffix** options in [Settings > Network](../settings/network.md): **Add DNS suffix (primary) to hostname** appends the Windows primary (or, if none is set, the active network adapter's connection-specific) DNS suffix to a bare hostname before resolving it, and **Use custom DNS suffix** lets you override it with a suffix of your own. Previously only available in DNS Lookup, this now also applies to every other tool that resolves hostnames through NETworkManager's shared DNS resolver (e.g. Ping, Traceroute, Port Scanner, IP Scanner, SNTP Lookup) — DNS Lookup stays independent. [#3591](https://github.com/BornToBeRoot/NETworkManager/pull/3591)
- **DNS server(s)** in [Settings > Network](../settings/network.md) is now a list of servers (IP address + port), configured via an **Edit DNS server** dialog, replacing the previous single semicolon-separated, port-53-only text field. [#3591](https://github.com/BornToBeRoot/NETworkManager/pull/3591)

## Improvements

- Links (e.g. in **About**, **Web Console** and the notices in **RDAP**) are now keyboard accessible: they can be reached with `Tab` and opened with `Enter`. [#3607](https://github.com/BornToBeRoot/NETworkManager/pull/3607)
- Actions that require elevated rights now show a tooltip with the reason when they are disabled because the application is not running as administrator (e.g. **Apply**, **Flush DNS cache** and **Release & Renew** in **Network Interface**, the context menu and **Add entry...** in **Hosts File Editor**, **Firewall** and **Neighbor Table**, and **Capture** in **Discovery Protocol**). [#3626](https://github.com/BornToBeRoot/NETworkManager/pull/3626)
- The collapsed/expanded state of profile groups (e.g. **linux-server**) is now remembered per profile file and shared across all tools, instead of resetting every time you switch tools or restart the application. [#3539](https://github.com/BornToBeRoot/NETworkManager/pull/3539)

**Network Interface**

- Members of the **Network Configuration Operators** group can now change the network configuration (IP address, DNS servers, release/renew) without being an administrator. NETworkManager has to run elevated (**Restart as Administrator**, confirm with your own credentials). The hint is now also shown as tooltip on **Apply** in the profile context menu. [#3606](https://github.com/BornToBeRoot/NETworkManager/issues/3606)

**Whois**

- Added an **Export...** button below the result, like in the other tools. [#3607](https://github.com/BornToBeRoot/NETworkManager/pull/3607)

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

- Added a live count of hosts up/down (and paused, if any) to each group header. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Added **Start** and **Pause** actions to each group header (next to the existing **Close**), to start every paused host in the group or pause every running one at once. Click/tap the **⋮** icon to open the group's action menu. [#3575](https://github.com/BornToBeRoot/NETworkManager/pull/3575)
- Reworked each monitored host's card: the collapsed quick info now shows labeled values (`Received: X · Lost: Y · Packet loss: Z%`) instead of unlabeled numbers, the expanded view shows only the latency chart (with more room, since Hostname/IP address are already visible in the header) and the last status change time moved to a tooltip on the connectivity icon. The **Status change** field was also renamed to **Last status change**. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Host input now accepts newline-separated hosts (one per line, e.g. a column pasted from Excel), converted automatically to the semicolon-separated form. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- Host input now supports shorthand IPv4 ranges like `192.168.0.1-100`, in addition to the existing `192.168.0.0-192.168.0.100` and `192.168.[0-100].1` range formats. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)

**Hosts File Editor**

- Added **Copy as...** to the context menu to create a new entry based on an existing one. The copy is added directly below the original entry. [0544698](https://github.com/BornToBeRoot/NETworkManager/commit/0544698aefc325ab36ca4c8e80250e1fe0b62d4a)
- Multiple entries can now be selected to enable, disable or delete them at once, or to change their comment via **Edit entries...** (`F2`). The edit dialog switches to a bulk mode in which only the comment can be changed. [#3624](https://github.com/BornToBeRoot/NETworkManager/pull/3624)
- Entries that don't change (e.g. enabling an already enabled entry, or confirming the edit dialog without changes) are no longer rewritten in the `hosts` file, so custom formatting such as tabs or alignment is kept. [#3624](https://github.com/BornToBeRoot/NETworkManager/pull/3624)

**Firewall**

- Added **Copy as...** to the context menu to create a new rule based on an existing one (name suffixed with ` - Copy`). [0544698](https://github.com/BornToBeRoot/NETworkManager/commit/0544698aefc325ab36ca4c8e80250e1fe0b62d4a)
- Multiple rules can now be selected to enable, disable or delete them at once. If one of the selected rules was removed in the meantime, nothing is changed. [#3627](https://github.com/BornToBeRoot/NETworkManager/pull/3627)
- A loading indicator is shown while the rules are loaded initially, and a message is shown if no rules are found. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- A rule can now be edited by double-clicking it. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Added a **Program** column that shows the file name of the program (full path as tooltip). [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- New **Settings > Firewall** page. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Added the **Show all rules** option (**Settings > Firewall**) to show all active Windows Firewall rules (including rules from Group Policy / MDM) instead of only the rules managed by NETworkManager. A new column with an icon shows whether a rule is managed by NETworkManager, not managed, or managed by a policy. Rules managed by a policy can only be copied. Rules not managed by NETworkManager can be enabled, disabled, deleted (after confirming a warning) or used as a template via **Copy as...** (with a warning if the rule uses properties that are not copied, e.g. ICMP types, a service, an app package, specific network adapters or users / required authentication), but editing them is not supported (yet). Rules with values that are not supported by NETworkManager (e.g. port keywords like `RPC`, a combination of interface types, a protocol like `IGMP` or the program `System`) are shown correctly, but can't be edited or copied. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- The search now also finds rules by program, ports, addresses and description, and by the translated direction, action and protocol. The list is filtered after a short delay while typing, so typing stays smooth with many rules.
- Sorting by the rule source column (**Show all rules**) now groups rules managed by NETworkManager, rules not managed by NETworkManager and rules from Group Policy / MDM / Windows, instead of mixing the last two.
- The **Program** field in the add / edit / copy rule dialog now shows an example path of a program instead of the path to PowerShell.

## Bug Fixes

- Fixed changes to lists in the settings (e.g. query histories, port profiles, custom commands) not being saved if no other setting was changed in the same session. This happened since the settings are stored as JSON. [#3607](https://github.com/BornToBeRoot/NETworkManager/pull/3607)

**Firewall**

- Fixed `F5` not refreshing the rule list, although it is documented as shortcut.
- Fixed a horizontal scrollbar appearing in the add / edit / copy rule dialog when the name or description is very long. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed port keywords (e.g. `RPC`) and combinations of interface types being shown as **Any**. Editing a rule that was changed outside of NETworkManager to use such values no longer silently removes them (the rule can't be edited in this case). [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed the program of a rule not being shown in the row details and the edit dialog. Saving the edit dialog then removed the program from the rule, so the rule applied to all programs. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed editing a rule deleting and re-creating it, which removed properties that are not supported by NETworkManager (e.g. ICMP types or a service set in the Windows Firewall management console). Only the changed properties are updated now (ICMP types are reset if the protocol is changed). [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed program paths with environment variables (e.g. `%SystemRoot%\System32\svchost.exe`) being rejected as non-existent in the add / edit / copy rule dialog. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)

**Ping Monitor**

- Fixed long error messages (e.g. from a failed host resolution) in the tooltip of the error icon not being wrapped. [#3626](https://github.com/BornToBeRoot/NETworkManager/pull/3626)

**Network Interface**

- Fixed errors when changing the network configuration (e.g. missing permissions) not being shown — the change was reported as successful although it failed. The error message of Windows is now shown. [#3606](https://github.com/BornToBeRoot/NETworkManager/issues/3606)

**Dashboard**

- Fixed the **Network Connection** widget running a full connection check on every application startup, even if the Status Window or Dashboard was never opened. It now only checks when actually shown - via the tray icon, on a network change, or when the Dashboard tab is opened. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Fixed the DNS status (Computer/Router/Internet) in the **Network Connection** widget showing as an error when no PTR record exists for the address, which is common and expected for private IP ranges. This is now shown as informational instead of critical. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Fixed a race condition in the **Network Connection** widget where results from a superseded check could overwrite the results of a newer, still-running check after quickly reopening the widget. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)

**DNS Lookup**

- Fixed **Add DNS suffix (primary) to hostname** silently appending no suffix at all on machines without a Windows primary DNS suffix configured (e.g. not domain-joined), even though a usable suffix was available via the active network adapter's connection-specific DNS suffix (shown under **DNS Suffix Search List** in `ipconfig /all`). It now falls back to that adapter's suffix in this case. [#3591](https://github.com/BornToBeRoot/NETworkManager/pull/3591)

**IP Scanner**

- Fixed NetBIOS lookups (computer name, domain/workgroup, user name) not starting until a host's entire port scan had finished, since the port scan wasn't actually running asynchronously despite being awaited alongside it. Ping, port scan, and NetBIOS resolution now genuinely run concurrently for every host. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Fixed the application becoming unresponsive (including the window not reacting to input) during a large scan (e.g. a /24). Scan results and progress were both being pushed to the UI one item/update at a time via a dispatcher call per host/port, which could flood the UI thread's message queue on large scans. Both are now batched and flushed periodically (every 150ms) instead, for **IP Scanner** and **Port Scanner** alike. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)

**IP Geolocation**

- Fixed the query history of **IP Geolocation** being written to the history of **Whois** instead of its own. [#3607](https://github.com/BornToBeRoot/NETworkManager/pull/3607)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Converted the **IP Scanner** and **Port Scanner** engines from `Parallel.ForEach` with blocking calls to genuinely asynchronous `Parallel.ForEachAsync`, removing the risk of exhausting the application's ThreadPool at high concurrency settings. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Refactored the **Network Connection** widget's check logic to reduce duplicated code and share the local IP address detection between the Computer and Router checks instead of detecting it twice. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Consolidated the duplicated **Profiles** side panel (search, tag filter, grouped list, context menu, add/edit/copy/delete) used across 15 tool views into a single shared control, reducing code duplication. As part of this, the profile panel's expanded/width state is now shared across all tools instead of being tracked per tool. [#3537](https://github.com/BornToBeRoot/NETworkManager/pull/3537)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
