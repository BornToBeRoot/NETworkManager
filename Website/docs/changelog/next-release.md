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

**Run command**

- The [**Run command**](../run-command.md) (`Ctrl+Shift+P`) now runs a feature directly, e.g. `ping 10.0.0.1`, `dns borntoberoot.net 1.1.1.1 aaaa`, `rdap 1.10 asn` or `rdp @server-01` (profile). It opens in a dialog instead of a flyout, with short aliases (e.g. `ping`, `dns`, `rdp`, `ssh`, `geo`, `wol`), suggestions for recent commands, profiles of the feature and types (`Up`/`Down` to select, `Tab` to complete, `Enter` to run, `Esc` to close), and validates the input like the feature does. Only visible features are offered. [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)
- New run command targets for the **Subnet Calculator** (`subnet 10.0.0.0/24`, `subnet 10.0.0.0/16 /24` for subnetting, `subnet 10.0.0.0/24 10.0.1.0/24` for the wide subnet) and the **Lookup** (`lookup oui 00:11:22`, `lookup port 22; ssh`). [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- New global hotkey to bring the window to the foreground and open the run command (default `Ctrl+Alt+P`, disabled by default), see [Settings > HotKeys](../settings/hotkeys.md). [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- Run command: the **Port Scanner** needs ports, which can also be the name of a port profile (e.g. `portscan 10.0.0.1 webserver`, suggested after the host). Own history size for the run command (default 20, [Settings > General](../settings/general.md#number-of-stored-entries-run-command)). `Tab` also completes profiles and port profiles, `PageUp`/`PageDown` move the selection by 5 suggestions. [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- Run command dialog redesigned: the icon of the recognized feature, the syntax with the part being typed highlighted, the values as they were understood (e.g. `Hosts: borntoberoot.net`, `Type: AAAA`), validation while typing, grouped suggestions with the typed text highlighted, examples and key hints (can be hidden in the settings). The dialog uses more space in a large window. The title bar shows a **Run command...** box in the center (like the command center of VS Code) instead of the icon. Type only a target (e.g. `10.0.0.1`, `server-01`, `00:11:22:33:44:55`, `10.0.0.0/24`) to get the commands that accept it, recent commands are also suggested while typing (e.g. a host); optionally, a target in the clipboard is suggested ([Settings > General](../settings/general.md#suggest-the-clipboard-content-as-target)). `Ctrl+Enter` runs and keeps the dialog open, `Shift+Enter` fills in the input without starting, `Shift+Del` or the remove button removes a recent command. [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)

**RDAP**

- New [**RDAP**](../application/rdap.md) tool to look up registration data via the Registration Data Access Protocol, the standardized, JSON-based successor of Whois. Supports domains, nameservers, top-level domains, IP addresses/CIDR prefixes, AS numbers and entity handles; the type is detected automatically or can be selected. The RDAP server is determined automatically using the IANA bootstrap files, which are cached locally and can be updated in the new **Settings > RDAP** page. Results are shown in a structured view (details, status, events, nameservers, DNSSEC, networks and AS numbers of an organization, contacts, notices) or as raw JSON, and can be exported as JSON or text. For domains (e.g. `.com`/`.net`), the referral to the registrar's RDAP server is followed as well (can be disabled in **Settings > RDAP**). Includes profile support. If no RDAP server is known for a domain, it can be opened in **Whois** with a single click. IP addresses and hostnames can be queried from the context menu of **IP Scanner** and **Traceroute**, AS numbers from **Traceroute**; for a hostname (e.g. `www.borntoberoot.net`), the registered domain is queried (determined with the included [Public Suffix List](https://publicsuffix.org/)). [#3607](https://github.com/BornToBeRoot/NETworkManager/pull/3607) [#3642](https://github.com/BornToBeRoot/NETworkManager/pull/3642)

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
- Faster scans with new defaults: all ports of a host are now scanned at the same time (**Max. concurrent port threads** `5` → `16`, max. `32`) and the timeouts are lower (**ICMP**, **port scan** and **NetBIOS** timeout `4000` → `2000` ms), so an unreachable host is finished after about 4 seconds (2 ICMP attempts) instead of up to 12 seconds. **Max. concurrent host threads** is lowered from `256` to `128` (max. `256` instead of `512`) to put less simultaneous load on the network. An empty `/24` network is scanned in about 8 seconds instead of about 50. Settings that are still on the old default are updated. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564) [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- The neighbor table (ARP/NDP) is read once and cached during a scan instead of once per reachable host to resolve the MAC address. The MAC address of the own IPv6 addresses and of IPv6 link-local neighbors is resolved as well. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- **Resolve MAC address and vendor** is enabled by default for new installations (as documented). [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- The export runs in the background, so the window stays responsive while a large result is exported (also in the **Port Scanner**). [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Hosts are started one after another (every 10 ms, shorter with short timeouts and many threads) instead of all at the same moment, the **Port Scanner** does the same with ports (every 1 ms). This avoids bursts of more than a thousand connection requests within a few milliseconds and makes the progress smoother. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Added **RDAP...** to the context menu to query the registration data of the IP address or hostname of a host in the [**RDAP**](../application/rdap.md) tool. [#3642](https://github.com/BornToBeRoot/NETworkManager/pull/3642)

**Port Scanner**

- Added a port status icon column to the results, matching the port icon used in the IP Scanner's extended port info, to indicate at a glance whether a port is open (green) or closed (red). [#3558](https://github.com/BornToBeRoot/NETworkManager/issues/3558)
- Added a live count of ports open/closed next to the **Result** header. Stays visible after the scan finishes. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Host and Ports input fields now accept newline-separated entries (one per line, e.g. a column pasted from Excel), converted automatically to the semicolon-separated form. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- Host input now supports shorthand IPv4 ranges like `192.168.0.1-100`, in addition to the existing `192.168.0.0-192.168.0.100` and `192.168.[0-100].1` range formats. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- New defaults: **Timeout** `4000` → `2000` ms (still allows one retransmission of a lost connection request), **Max. concurrent host threads** `5` → `4` and **Max. concurrent port threads** `256` → `128`, which puts less simultaneous load on the scanned hosts (up to 512 instead of 1280 connections at the same time). Settings that are still on the old default are updated. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564) [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Added a new **Well-known ports** (`1-1024`) default port profile. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)

**Ping Monitor**

- Added a live count of hosts up/down (and paused, if any) to each group header. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Added **Start** and **Pause** actions to each group header (next to the existing **Close**), to start every paused host in the group or pause every running one at once. Click/tap the **⋮** icon to open the group's action menu. [#3575](https://github.com/BornToBeRoot/NETworkManager/pull/3575)
- Reworked each monitored host's card: the collapsed quick info now shows labeled values (`Received: X · Lost: Y · Packet loss: Z%`) instead of unlabeled numbers, the expanded view shows only the latency chart (with more room, since Hostname/IP address are already visible in the header) and the last status change time moved to a tooltip on the connectivity icon. The **Status change** field was also renamed to **Last status change**. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Host input now accepts newline-separated hosts (one per line, e.g. a column pasted from Excel), converted automatically to the semicolon-separated form. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- Host input now supports shorthand IPv4 ranges like `192.168.0.1-100`, in addition to the existing `192.168.0.0-192.168.0.100` and `192.168.[0-100].1` range formats. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)

**Traceroute**

- Added **RDAP...** to the context menu to query the registration data of the IP address, hostname or AS number of a hop in the [**RDAP**](../application/rdap.md) tool. [#3642](https://github.com/BornToBeRoot/NETworkManager/pull/3642)

**DNS Lookup**

- IP addresses opened from another feature (e.g. **IP Scanner** > **DNS lookup** > **IP address**), from a profile or with the run command are looked up with `PTR` instead of the selected record type. [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)

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
- The search now also finds rules by program, ports, addresses and description, and by the translated direction, action and protocol. The list is filtered after a short delay while typing, so typing stays smooth with many rules. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- Sorting by the rule source column (**Show all rules**) now groups rules managed by NETworkManager, rules not managed by NETworkManager and rules from Group Policy / MDM / Windows, instead of mixing the last two. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- The **Program** field in the add / edit / copy rule dialog now shows an example path of a program instead of the path to PowerShell. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- The program of a rule can now be selected with a file dialog in the add / edit / copy rule dialog. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- The selected rule stays selected and visible when the list is reloaded (e.g. after enabling, disabling or editing it), instead of jumping back to the top of the list. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- The row details now show the source of a rule (local, Group Policy, MDM or Windows) and the unsupported properties it uses (e.g. ICMP types or a service), which are not copied via **Copy as...**. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- The export now contains whether a rule is managed by NETworkManager and its source. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- Faster loading of the rules managed by NETworkManager if there are many of them (the ports, addresses, programs etc. of all rules are queried at once instead of per rule). [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)

## Bug Fixes

- Fixed changes to lists in the settings (e.g. query histories, port profiles, custom commands) not being saved if no other setting was changed in the same session. This happened since the settings are stored as JSON. [#3607](https://github.com/BornToBeRoot/NETworkManager/pull/3607)
- Fixed the profile search (profile list in the tools, **Profiles** settings and import result) filtering the profiles while still typing. The profiles are now filtered once after the user stopped typing, instead of every 750 ms after the first key press. [#3638](https://github.com/BornToBeRoot/NETworkManager/pull/3638)
- Fixed the application list not scrolling to the selected application if it was opened from somewhere else (e.g. from the **Dashboard**, another application or the run command). [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)

**Firewall**

- Fixed `F5` not refreshing the rule list, although it is documented as shortcut. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- Fixed an empty rule list with "Reloaded at ..." being shown if the rules could not be loaded (e.g. the Windows Firewall service is not running). An error is shown now, and a message if the rules could only be loaded partially. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- Fixed the Firewall not working until the application was restarted if loading the PowerShell module `NetSecurity` failed once. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- Fixed keywords like `LocalSubnet` or `DNS` being accepted for **Local addresses** in the add / edit / copy rule dialog, although Windows only accepts `Any` there (saving the rule failed). The same applies to `Any4` / `Any6` for local and remote addresses. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- Fixed a horizontal scrollbar appearing in the add / edit / copy rule dialog when the name or description is very long. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed port keywords (e.g. `RPC`) and combinations of interface types being shown as **Any**. Editing a rule that was changed outside of NETworkManager to use such values no longer silently removes them (the rule can't be edited in this case). [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed the program of a rule not being shown in the row details and the edit dialog. Saving the edit dialog then removed the program from the rule, so the rule applied to all programs. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed editing a rule deleting and re-creating it, which removed properties that are not supported by NETworkManager (e.g. ICMP types or a service set in the Windows Firewall management console). Only the changed properties are updated now (ICMP types are reset if the protocol is changed). [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed program paths with environment variables (e.g. `%SystemRoot%\System32\svchost.exe`) being rejected as non-existent in the add / edit / copy rule dialog. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)

**Ping Monitor**

- Fixed long error messages (e.g. from a failed host resolution) in the tooltip of the error icon not being wrapped. [#3626](https://github.com/BornToBeRoot/NETworkManager/pull/3626)

**Network Interface**

- Fixed errors when changing the network configuration (e.g. missing permissions) not being shown — the change was reported as successful although it failed. The error message of Windows is now shown. [#3606](https://github.com/BornToBeRoot/NETworkManager/issues/3606)

**Connections**

- Fixed the auto refresh continuing in the background after switching to another tool, or after it was disabled, if this happened while a refresh was running. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)

**Listeners**

- Fixed the auto refresh continuing in the background after switching to another tool, or after it was disabled, if this happened while a refresh was running. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)

**Neighbor Table**

- Fixed adding and deleting entries not working until the application was restarted if loading the PowerShell module `NetTCPIP` failed once. [cc05937](https://github.com/BornToBeRoot/NETworkManager/commit/cc05937a7de2bf1247621845f47077f3796b9bb2)
- Fixed the auto refresh continuing in the background after switching to another tool, or after it was disabled, if this happened while a refresh was running. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)

**WiFi**

- Fixed the auto refresh continuing in the background after switching to another tool, or after it was disabled, if this happened while a refresh was running. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)
- Fixed the auto refresh not being resumed after switching back from another tool. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)

**Dashboard**

- Fixed the **Network Connection** widget running a full connection check on every application startup, even if the Status Window or Dashboard was never opened. It now only checks when actually shown - via the tray icon, on a network change, or when the Dashboard tab is opened. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Fixed the DNS status (Computer/Router/Internet) in the **Network Connection** widget showing as an error when no PTR record exists for the address, which is common and expected for private IP ranges. This is now shown as informational instead of critical. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Fixed a race condition in the **Network Connection** widget where results from a superseded check could overwrite the results of a newer, still-running check after quickly reopening the widget. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Fixed the Status Window closing by itself after being opened from the tray icon (or with the hotkey), if it was opened automatically on a network change shortly before. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)

**DNS Lookup**

- Fixed **Add DNS suffix (primary) to hostname** silently appending no suffix at all on machines without a Windows primary DNS suffix configured (e.g. not domain-joined), even though a usable suffix was available via the active network adapter's connection-specific DNS suffix (shown under **DNS Suffix Search List** in `ipconfig /all`). It now falls back to that adapter's suffix in this case. [#3591](https://github.com/BornToBeRoot/NETworkManager/pull/3591)

**IP Scanner**

- Fixed NetBIOS lookups (computer name, domain/workgroup, user name) not starting until a host's entire port scan had finished, since the port scan wasn't actually running asynchronously despite being awaited alongside it. Ping, port scan, and NetBIOS resolution now genuinely run concurrently for every host. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Fixed the application becoming unresponsive (including the window not reacting to input) during a large scan (e.g. a /24). Scan results and progress were both being pushed to the UI one item/update at a time via a dispatcher call per host/port, which could flood the UI thread's message queue on large scans. Both are now batched and flushed periodically (every 150ms) instead, for **IP Scanner** and **Port Scanner** alike. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Fixed every host being shown as unreachable when the ICMP **Buffer** was set to more than `65500` bytes (Windows limit), the same applies to **Ping Monitor** and **Traceroute**. The maximum is now `65500`, higher values are lowered to `65500`. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed the ICMP **Attempts** setting being ignored: a host was shown as unreachable after the first lost ping. Another ping is now sent if the previous one timed out. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)

**IP Scanner** / **Port Scanner**

- Fixed the scan getting stuck (button disabled, scan never ends) when it was canceled right before the scan started, and when an unexpected error occurred. Errors are now shown as status message. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed the message "Canceled by user" being replaced by "No reachable hosts found" / "No open ports found" when the scan was canceled before the first result, and a new scan started right after canceling showing no progress or stopping too early. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed the message "The following hostnames could not be resolved" being replaced by "No reachable hosts found" / "No open ports found". Both messages are shown now. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed the scan getting stuck when an error occurred while the hosts or ports were prepared, and the **IP Scanner** subnet detection button staying disabled after an error. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed ports being shown as closed when there was no answer at all (e.g. host or network unreachable). Closed now means the host refused the connection, otherwise the port is timed out. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed large scans using up the local (ephemeral) ports: every open port left a connection in the `TIME_WAIT` state for several minutes, so later connections failed (also in other applications). Connections are now closed with a reset. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- **IP Scanner**: Fixed hosts that block ICMP and refuse the connection on the scanned ports (TCP reset) being shown as unreachable. A refused connection is an answer from the host, so it is now shown as reachable. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed IP addresses being scanned twice if the inputs overlap (e.g. `10.0.0.1; 10.0.0.0/24`), the same applies to **Ping Monitor**. If an IP address is also entered as hostname, the hostname is kept. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed the application running out of memory (and the scan getting stuck) for very large inputs like `10.0.0.0/1`. A scan of more than `65,536` IP addresses (**Port Scanner**: hosts × ports) must now be confirmed, more than `16,777,216` are refused with a message. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- **Port Scanner**: Fixed ports being scanned twice if they are part of multiple inputs (e.g. `80; 1-100` or multiple port profiles). [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Fixed IP ranges across `128.0.0.0` (e.g. `127.255.255.250 - 128.0.0.5`) being rejected as invalid. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- **Port Scanner**: Fixed the application crashing when the results of multiple hosts were sorted by IP address. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- **Port Scanner**: Results are sorted by IP address and port (before, in the order the ports were finished), and the hostname is resolved while the ports are scanned instead of before. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- **IP Scanner**: Fixed sorting by **NetBIOS reachable** not working and the column being shown with NetBIOS disabled. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- **IP Scanner**: Fixed **Copy** > **Hostname** / **MAC address** / **Vendor** being hidden in the context menu while the column was shown (resolved via NetBIOS only), and **Copy** > **Status** copying `True` / `False` instead of `Up` / `Down`. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- **IP Scanner**: Fixed a hostname received from the network (DNS PTR record or NetBIOS name) with special characters like `&` being able to run additional commands through a custom command with `{{Hostname}}` (e.g. `cmd.exe /c ...`). Such a hostname is now replaced with an empty value. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- **IP Scanner**: Fixed **Edit** (`F2`) and **Delete** (`Del`) of a custom command in the settings being available without a selected command. The delete confirmation shows the name of the command. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)

**IP Geolocation**

- Fixed the query history of **IP Geolocation** being written to the history of **Whois** instead of its own. [#3607](https://github.com/BornToBeRoot/NETworkManager/pull/3607)

**WiFi**

- Fixed the connection status message (e.g. "Connecting to ...") being hidden too early, while still connecting, if a previous message (e.g. after connecting or disconnecting) was shown shortly before. Each message is now shown for 15 seconds. [#3638](https://github.com/BornToBeRoot/NETworkManager/pull/3638)

**History**

- Fixed adding an entry to the history of an input (e.g. hosts, ports, domains) failing when the number of stored entries is set to `0` in [Settings > General](../settings/general.md#number-of-stored-entries). With `0`, no entries are stored. Removing an entry from the history (also the oldest one, shared by several tabs) no longer clears the input that shows it. [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)

**Input validation**

- Fixed host names being accepted with a hyphen at the start or end of a label other than the first (e.g. `server.-bad-.com`) and with only digits (e.g. `10.0.0.` or `192.168.1`, which are an incomplete IPv4 address). The same applies to the domain in **Whois**. IPv4 addresses are still accepted in all host fields, now checked explicitly in the **Remote Desktop** host field. [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- Fixed MAC addresses and their first 3 bytes (**Wake on LAN**, **Lookup** > **OUI**, profiles) being accepted with any character instead of a dot in the format `0000.0000.0000` (e.g. `0011 2233 4455`). [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- Fixed IPv6 subnets in the **Subnet Calculator** being accepted with a leading `s` (e.g. `sss2001:db8::/32`) and rejected with an embedded IPv4 address (e.g. `::ffff:192.168.0.1/96`). [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- Fixed the port of the **Remote Desktop** host field (`host:port`) being accepted empty or as `0`. [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- Fixed the application freezing when validating an invalid domain with many labels in **Whois** (exponential backtracking of the regular expression). [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- Fixed IPv6 subnets with whitespace or a zone index (e.g. `fe80::1%eth0/64`) being accepted in the **Subnet Calculator**, which can't calculate them. [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- Fixed an empty entry in **Lookup** > **OUI** / **Port** (e.g. `22;;80`) finding every vendor or service. [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- Fixed **Lookup** > **Port** searching for a port with an upper-case protocol (e.g. `22/TCP`) as a service instead of the port and protocol. [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)

**Subnet Calculator**

- Fixed **Subnetting** getting stuck when the new subnet mask is smaller than the subnet mask of the subnet (e.g. `10.0.0.0/24` with `/16`) or too large for the address family (e.g. `/40` for IPv4). An error is shown instead. [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Converted the **IP Scanner** and **Port Scanner** engines from `Parallel.ForEach` with blocking calls to genuinely asynchronous `Parallel.ForEachAsync`, removing the risk of exhausting the application's ThreadPool at high concurrency settings. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- The TCP port probe (**IP Scanner**, **Port Scanner**) and the NetBIOS request (**IP Scanner**) report a refused connection, a timeout or no answer as result instead of throwing an exception for each of them (thousands per scan). Faster, and the application no longer lags while scanning with a debugger attached. [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)
- Refactored the **Network Connection** widget's check logic to reduce duplicated code and share the local IP address detection between the Computer and Router checks instead of detecting it twice. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Consolidated the duplicated **Profiles** side panel (search, tag filter, grouped list, context menu, add/edit/copy/delete) used across 15 tool views into a single shared control, reducing code duplication. As part of this, the profile panel's expanded/width state is now shared across all tools instead of being tracked per tool. [#3537](https://github.com/BornToBeRoot/NETworkManager/pull/3537)
- Replaced the internal event system for opening a feature with data (e.g. **Open in RDAP** from the **IP Scanner**) with typed requests, which can also carry options (e.g. DNS server and record type) and profiles. Features no longer fail to open from another feature while the application list is filtered by a search. [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)
- Input histories (e.g. hosts, domains, ports) are updated in place instead of being cleared and refilled, which avoids clearing the text of the input field and unnecessary updates. [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)
- Added a unit test project (xUnit) with tests for the run command parser and the history lists. [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
