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
- **Port Scanner**: the JSON export uses the key `State` for the port state (like CSV and XML) instead of `Status`. Scripts that read `Status` have to be adjusted. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

## What's new?

- ARM64 builds are now available. [#3538](https://github.com/BornToBeRoot/NETworkManager/issues/3538)

**Run command**

- The [**Run command**](../run-command.md) (`Ctrl+Shift+P`) now runs a feature directly, e.g. `ping 10.0.0.1`, `dns borntoberoot.net 1.1.1.1 aaaa`, `rdap 1.10 asn` or `rdp @server-01` (profile). It opens in a dialog instead of a flyout, with short aliases (e.g. `ping`, `dns`, `rdp`, `ssh`, `geo`, `wol`), suggestions for recent commands, profiles of the feature and types (`Up`/`Down` to select, `Tab` to complete, `Enter` to run, `Esc` to close), and validates the input like the feature does. Only visible features are offered. [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)
- New run command targets for the **Subnet Calculator** (`subnet 10.0.0.0/24`, `subnet 10.0.0.0/16 /24` for subnetting, `subnet 10.0.0.0/24 10.0.1.0/24` for the wide subnet) and the **Lookup** (`lookup oui 00:11:22`, `lookup port 22; ssh`). [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- New global hotkey to bring the window to the foreground and open the run command (default `Ctrl+Alt+P`, disabled by default), see [Settings > HotKeys](../settings/hotkeys.md). [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
- Run command: the **Port Scanner** needs ports, which can also be names of port profiles, also several and together with ports (e.g. `portscan 10.0.0.1 webserver` or `portscan 10.0.0.1 "DNS (via TCP)";webserver;22`, suggested after the host and after a `;`). Own history size for the run command (default 20, [Settings > General](../settings/general.md#number-of-stored-entries-run-command)). `Tab` also completes profiles and port profiles, `PageUp`/`PageDown` move the selection by 5 suggestions. [#3646](https://github.com/BornToBeRoot/NETworkManager/pull/3646)
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
- Added `21` (FTP, e.g. NAS), `23` (Telnet, e.g. older switches), `135` (RPC), `554` (RTSP, e.g. IP cameras), `631` (IPP, printers), `5900` (VNC), `5985` (WinRM), `7000` (AirPlay), `8009` (Chromecast), `8080` / `8443` (web interfaces of devices and appliances), `9100` (raw printing) and `62078` (iPhone / iPad) to the default **Ports** list used to detect if a host is reachable, e.g. hosts that block ICMP. An unreachable host is still finished after about 4 seconds (22 ports with **Max. concurrent port threads** `16`, like 2 ICMP attempts). Settings that are still on the old default are updated. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564) [#3667](https://github.com/BornToBeRoot/NETworkManager/pull/3667)
- Added a button next to **Ports** in the settings to restore the default ports. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- New default custom commands **Windows Explorer (shares)**, **Computer Management**, **Event Viewer** and **System Information** (for new settings; existing custom commands are not changed). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Faster scans with new defaults: all ports of a host are now scanned at the same time (**Max. concurrent port threads** `5` → `16`, max. `32`) and the timeouts are lower (**ICMP**, **port scan** and **NetBIOS** timeout `4000` → `2000` ms), so an unreachable host is finished after about 4 seconds (2 ICMP attempts) instead of up to 12 seconds. **Max. concurrent host threads** is lowered from `256` to `128` (max. `256` instead of `512`) to put less simultaneous load on the network. An empty `/24` network is scanned in about 8 seconds instead of about 50. Settings that are still on the old default are updated. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564) [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- The neighbor table (ARP/NDP) is read once and cached during a scan instead of once per reachable host to resolve the MAC address. The MAC address of the own IPv6 addresses and of IPv6 link-local neighbors is resolved as well. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- **Resolve MAC address and vendor** is enabled by default for new installations (as documented). [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- The export runs in the background, so the window stays responsive while a large result is exported, and uses the order shown in the result (sorting) instead of the order in which the hosts were scanned. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- The DNS hostname and the MAC address of a host are resolved at the same time instead of one after the other. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- New **Expand host view** setting to show the details of every host in the result (also hosts added while the scan is running), like in the **Ping Monitor**. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- The up / down counter next to the **Result** header is translated as one text, so translations can put the number in a different position. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- **Rescan** in the context menu scans the selected host(s) again with the settings of the shown scan and updates their rows, e.g. to check if a host is up again. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Added **Web Console** (https / http depending on the open ports) and **Wake on LAN** (with the MAC address) to the context menu. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Added a search box above the result to filter the hosts by IP address, hostname, MAC address, vendor and open ports, also while the scan is running. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- **Copy** > **IP address** / **Hostname** / **MAC address** is also available if multiple results are selected (one value per line, in the order shown). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Added a **Time** column with the round trip time of the ping. Sorted by time, hosts without a ping reply are at the end. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- A host is shown as soon as it answers (ping reply, open or refused port or NetBIOS answer) instead of after all probes and lookups are finished (up to the port scan / NetBIOS timeout, plus the DNS lookup). Values of probes that are still running are shown as `...`, the other results, the DNS hostname and the MAC address / vendor are filled in when they arrive. The DNS hostname and the MAC address are resolved right after the first answer. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- The CSV and XML export contain the DNS hostname (`DNSHostname`) in addition to the hostname (DNS, or NetBIOS as fallback), like the JSON export. It is added as last column (CSV) / element (XML), so the position of the other columns doesn't change. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Hosts are started one after another (every 10 ms, shorter with short timeouts and many threads) instead of all at the same moment. This avoids bursts of more than a thousand connection requests within a few milliseconds and makes the progress smoother. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Closed ports are reported immediately. Windows repeated the connection request after the host refused it and reported a closed port only after about 2 seconds. The connection request is sent once. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- New **Attempts** setting for the port scan (default `1`): another connection request is sent if there was no answer, e.g. on an unreliable network (Wi-Fi, VPN) or for hosts on the internet. Open and closed ports are not repeated. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Added **RDAP...** to the context menu to query the registration data of the IP address or hostname of a host in the [**RDAP**](../application/rdap.md) tool. [#3642](https://github.com/BornToBeRoot/NETworkManager/pull/3642)
- The probes a host answered (**Ping**, **TCP**, **NetBIOS**) are shown in the tooltip of the status icon and as **Detected by** in the row details, e.g. to see why a host is up. The export contains them as `DetectedBy` (last column / element). [#PR](https://github.com/BornToBeRoot/NETworkManager/pull/PR)

**Port Scanner**

- Added a port status icon column to the results, matching the port icon used in the IP Scanner's extended port info, to indicate at a glance whether a port is open (green) or closed (red). [#3558](https://github.com/BornToBeRoot/NETworkManager/issues/3558)
- Added a live count of ports open/closed next to the **Result** header. Stays visible after the scan finishes. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Host and Ports input fields now accept newline-separated entries (one per line, e.g. a column pasted from Excel), converted automatically to the semicolon-separated form. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- Host input now supports shorthand IPv4 ranges like `192.168.0.1-100`, in addition to the existing `192.168.0.0-192.168.0.100` and `192.168.[0-100].1` range formats. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- New defaults: **Timeout** `4000` → `2000` ms (still allows one retransmission of a lost connection request), **Max. concurrent host threads** `5` → `4` and **Max. concurrent port threads** `256` → `128`, which puts less simultaneous load on the scanned hosts (up to 512 instead of 1280 connections at the same time). Settings that are still on the old default are updated. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564) [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Closed ports are reported immediately. Windows repeated the connection request after the host refused it and reported a closed port only after about 2 seconds, so a scan of a host that refuses most ports is much faster now (e.g. `1-1024`: about 1 second instead of about 16 seconds). The connection request is sent once. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- New **Attempts** setting (default `1`): another connection request is sent if there was no answer, e.g. on an unreliable network (Wi-Fi, VPN) or for hosts on the internet. Open and closed ports are not repeated. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- The counter next to the **Result** header shows timed out ports (no answer) separately: `Open · Closed · Timed out`. Before, they were counted as closed. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- The open / closed counter next to the **Result** header is translated as one text, so translations can put the number in a different position. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- **Rescan** in the context menu scans the selected port(s) again and updates their state in place; **Rescan host** in the context menu of a host (group header) scans the host again with all ports of the shown scan and replaces its results. The context menu of a host also opens it in other tools, adds it as a profile and copies its IP address, hostname or open ports (e.g. `22; 80; 443`). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Added **Web Console** to the context menu of an open web port (`80`, `443`, `8080`, `8443`). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Added **Ping Monitor**, **Traceroute**, **DNS lookup...**, **RDAP...**, **Remote Desktop**, **PowerShell**, **PuTTY**, **TigerVNC** and **SNMP** to the context menu to open the host (hostname, otherwise the IP address) in the tool, like in the **IP Scanner**. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- The header of each host shows the number of open ports (e.g. `server (10.0.0.1) · 3 Open`), also while the scan is running. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- The **Ports** input lists the history (a single port profile is not added to it) and the port profiles (name, with the ports below), like the DNS servers in **DNS Lookup**: select a port profile, or type ports and port profile names. Replaces the **Select port profile** button and its dialog. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Profiles: the **Ports** of the Port Scanner can contain port profiles (selectable in the list like in the tool), which are referenced by the profile - changes to a port profile apply, also after a rename. The ports are saved with the profile as well and used, with a note, if the port profile can't be found (e.g. a profile file shared from another computer, port profiles are saved in the settings). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Port profiles can be entered by name in the **Ports** input, also several and together with ports (e.g. `Webserver; Database; 8080`, case-insensitive). `Tab` completes the name that is being typed (e.g. `webserver; dat` → `webserver; Database`), also in the profile dialog. The name of a port profile must be unique, must not contain `;` (replaced by `,` when updating) and must not be a port or port range. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Added **Add profile...** to the context menu: creates a profile with the host (IP address), the hostname as name and the ports of the shown scan. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Several port profiles can be selected in the settings and deleted at once (context menu or `Del`), the confirmation lists them. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Added a search box above the result to filter the ports by IP address, hostname, port, protocol, service, description and status, also while the scan is running. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- **Copy** > **IP address** / **Hostname** / **Port** / **IP address:Port** is available if multiple results are selected (one value per line, in the order shown). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- New setting **Ping hosts before the scan** (default off): only hosts that answer a ping are scanned, the number of skipped hosts is shown after the scan. The ping has its own **Attempts** (default `2`), **Timeout** (default `2000` ms) and **Buffer** (default `32` bytes) settings, like the IP Scanner. Faster for ranges with many unused IP addresses; hosts that block ICMP are skipped. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660) [#3667](https://github.com/BornToBeRoot/NETworkManager/pull/3667)
- New setting **Adjust threads automatically** (default on): the number of hosts and ports scanned at the same time is adjusted to the scan (up to `512` connections, at most `128` hosts and `128` ports per host). A scan with few ports is much faster, e.g. a `/24` with one port (e.g. `3389`) takes about 4 seconds instead of about 2 minutes; `1-1024` is unchanged. **Max. concurrent host threads** (now up to `256`) and **Max. concurrent port threads** are used if it is disabled, with at most `8,192` connections at the same time. It is disabled when updating if the threads were changed. The hostname (DNS `PTR`) of a host is resolved when its first port is scanned, so the lookups are spread over time like the ports. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Added a new **Well-known ports** (`1-1024`) default port profile. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Groups: new **Ports** setting (ports, port ranges and port profiles) for the Port Scanner, used by the profiles of the group that don't override them (new **Ports** checkbox in the profile, like the other settings that can be set in the group). Existing profiles with ports keep them (the checkbox is enabled when the profile file is loaded). [#3667](https://github.com/BornToBeRoot/NETworkManager/pull/3667)
- The export runs in the background, so the window stays responsive while a large result is exported, and uses the order shown in the result (sorting, grouping) instead of the order in which the ports were scanned. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Ports are started one after another (every 1 ms, shorter with short timeouts and many threads) instead of all at the same moment. This avoids bursts of hundreds of connection requests within a few milliseconds and makes the progress smoother. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)

**Ping Monitor**

- Added a live count of hosts up/down (and paused, if any) to each group header. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- The up / down / paused counters of the group header are translated as one text, so translations can put the number in a different position. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Added **Start** and **Pause** actions to each group header (next to the existing **Close**), to start every paused host in the group or pause every running one at once. Click/tap the **⋮** icon to open the group's action menu. [#3575](https://github.com/BornToBeRoot/NETworkManager/pull/3575)
- Reworked each monitored host's card: the collapsed quick info now shows labeled values (`Received: X · Lost: Y · Packet loss: Z%`) instead of unlabeled numbers, the expanded view shows only the latency chart (with more room, since Hostname/IP address are already visible in the header) and the last status change time moved to a tooltip on the connectivity icon. The **Status change** field was also renamed to **Last status change**. [#3572](https://github.com/BornToBeRoot/NETworkManager/pull/3572)
- Host input now accepts newline-separated hosts (one per line, e.g. a column pasted from Excel), converted automatically to the semicolon-separated form. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)
- Host input now supports shorthand IPv4 ranges like `192.168.0.1-100`, in addition to the existing `192.168.0.0-192.168.0.100` and `192.168.[0-100].1` range formats. [#3568](https://github.com/BornToBeRoot/NETworkManager/pull/3568)

**Traceroute**

- Added **RDAP...** to the context menu to query the registration data of the IP address, hostname or AS number of a hop in the [**RDAP**](../application/rdap.md) tool. [#3642](https://github.com/BornToBeRoot/NETworkManager/pull/3642)

**DNS Lookup**

- Profiles: new option **DNS server** to use a DNS server of the profile instead of the one selected in the DNS lookup, with the same list and `Tab` completion. It can also be set in the group (used by the profiles of the group that don't override it). New option **Type** (record type) in the profile; without it, a profile is looked up with `PTR` for IP addresses and `A` for hostnames instead of the last selected record type. Configured DNS servers are referenced by the profile (changes apply, also after a rename), their IP addresses are saved as well and used, with a note, if the DNS server can't be found (e.g. a profile file shared from another computer). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660) [#3667](https://github.com/BornToBeRoot/NETworkManager/pull/3667)
- Several DNS servers can be entered, also names of configured DNS servers together with custom DNS servers (e.g. `Google Public DNS; Cloudflare; 9.9.9.9`), every DNS server is queried once; `[Windows DNS]` in a list adds the DNS servers of Windows. The list of the **DNS server** input shows the history and the configured DNS servers (name, with the IP addresses below), `Tab` completes the name of a DNS server that is being typed (e.g. `google; cl` → `google; Cloudflare`). The name of a DNS server must not contain `;` (replaced by `,` when updating). Also with the run command (`dns borntoberoot.net "Google Public DNS";cloudflare`). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- IP addresses opened from another feature (e.g. **IP Scanner** > **DNS lookup** > **IP address**), from a profile or with the run command are looked up with `PTR` instead of the selected record type. [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)

**Hosts File Editor**

- Added **Copy as...** to the context menu to create a new entry based on an existing one. The copy is added directly below the original entry. [0544698](https://github.com/BornToBeRoot/NETworkManager/commit/0544698aefc325ab36ca4c8e80250e1fe0b62d4a)
- Multiple entries can now be selected to enable, disable or delete them at once, or to change their comment via **Edit entries...** (`F2`). The edit dialog switches to a bulk mode in which only the comment can be changed. [#3624](https://github.com/BornToBeRoot/NETworkManager/pull/3624)
- Entries that don't change (e.g. enabling an already enabled entry, or confirming the edit dialog without changes) are no longer rewritten in the `hosts` file, so custom formatting such as tabs or alignment is kept. [#3624](https://github.com/BornToBeRoot/NETworkManager/pull/3624)
- The search filters after a short delay while typing instead of on every key press (smoother with many entries), `Enter` filters right away. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

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
- `Enter` in the search box filters the rules right away instead of after the delay while typing. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

**Profiles**

- **Settings > Profiles**: `Enter` in the search box filters the profiles right away instead of after the delay while typing. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

**Connections**

- The search filters after a short delay while typing instead of on every key press (smoother with many connections), `Enter` filters right away. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

**Listeners**

- The search filters after a short delay while typing instead of on every key press (smoother with many listeners), `Enter` filters right away. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

**Neighbor Table**

- The search filters after a short delay while typing instead of on every key press (smoother with many entries), `Enter` filters right away. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

**WiFi**

- The search filters after a short delay while typing instead of on every key press, `Enter` filters right away. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

## Bug Fixes

- Fixed changes to lists in the settings (e.g. query histories, port profiles, custom commands) not being saved if no other setting was changed in the same session. This happened since the settings are stored as JSON. [#3607](https://github.com/BornToBeRoot/NETworkManager/pull/3607)
- Fixed the profile search (profile list in the tools, **Profiles** settings and import result) filtering the profiles while still typing. The profiles are now filtered once after the user stopped typing, instead of every 750 ms after the first key press. [#3638](https://github.com/BornToBeRoot/NETworkManager/pull/3638)
- Fixed the application list not scrolling to the selected application if it was opened from somewhere else (e.g. from the **Dashboard**, another application or the run command). [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)
- Fixed `Enter` in the application search (title bar) being able to trigger the default button of the shown tool (e.g. start a scan in the **IP Scanner**). It only filters the applications now. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper) in **About** and the DNS server profiles in **Settings > Network**. The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)
- Fixed the screenshot that is shown in place of embedded windows (**Remote Desktop**, **PowerShell**, **PuTTY**, **TigerVNC**, **Web Console**) while a dialog is open being cropped / stretched with a display scaling above 100% or on a monitor with a different scaling, and being stretched / scaled if the window is resized while the dialog is open (it keeps its size now). The `AirspacePanel` of [AirspaceFixer](https://github.com/chris84948/AirspaceFixer) is now part of NETworkManager, based on the fork [AirspaceFixerDotNet](https://github.com/ellipticbit/AirspaceFixerDotNet) with per monitor DPI support, which also fixes a GDI handle leak. [#3681](https://github.com/BornToBeRoot/NETworkManager/pull/3681)

**Firewall**

- Fixed the name of the feature not being translatable in the list of features, the settings and the run command (missing translation key). [#3667](https://github.com/BornToBeRoot/NETworkManager/pull/3667)
- Fixed `F5` not refreshing the rule list, although it is documented as shortcut. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- Fixed an empty rule list with "Reloaded at ..." being shown if the rules could not be loaded (e.g. the Windows Firewall service is not running). An error is shown now, and a message if the rules could only be loaded partially. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- Fixed the Firewall not working until the application was restarted if loading the PowerShell module `NetSecurity` failed once. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- Fixed keywords like `LocalSubnet` or `DNS` being accepted for **Local addresses** in the add / edit / copy rule dialog, although Windows only accepts `Any` there (saving the rule failed). The same applies to `Any4` / `Any6` for local and remote addresses. [#3637](https://github.com/BornToBeRoot/NETworkManager/pull/3637)
- Fixed a horizontal scrollbar appearing in the add / edit / copy rule dialog when the name or description is very long. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed port keywords (e.g. `RPC`) and combinations of interface types being shown as **Any**. Editing a rule that was changed outside of NETworkManager to use such values no longer silently removes them (the rule can't be edited in this case). [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed the program of a rule not being shown in the row details and the edit dialog. Saving the edit dialog then removed the program from the rule, so the rule applied to all programs. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed editing a rule deleting and re-creating it, which removed properties that are not supported by NETworkManager (e.g. ICMP types or a service set in the Windows Firewall management console). Only the changed properties are updated now (ICMP types are reset if the protocol is changed). [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed program paths with environment variables (e.g. `%SystemRoot%\System32\svchost.exe`) being rejected as non-existent in the add / edit / copy rule dialog. [#3630](https://github.com/BornToBeRoot/NETworkManager/pull/3630)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper). The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**Ping Monitor**

- Fixed long error messages (e.g. from a failed host resolution) in the tooltip of the error icon not being wrapped. [#3626](https://github.com/BornToBeRoot/NETworkManager/pull/3626)
- Fixed the ping not working when the ICMP **Buffer** was set to more than `65500` bytes (Windows limit). The maximum is now `65500`, higher values are lowered to `65500`. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed hosts being added twice if the inputs overlap (e.g. `10.0.0.1; 10.0.0.0/24`). If an IP address is also entered as hostname, the hostname is kept; different hostnames of the same IP address (e.g. two names of one server) are all kept. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed **Copy** > **Hostname** copying a hostname resolved via DNS (`PTR`) with a trailing dot (e.g. `server.example.com.`). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

**Traceroute**

- Fixed the trace not working when the ICMP **Buffer** was set to more than `65500` bytes (Windows limit). The maximum is now `65500`, higher values are lowered to `65500`. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed hostnames of the hops resolved via DNS (`PTR`) being shown with a trailing dot (e.g. `server.example.com.`), also in the export and when opened in another feature (e.g. **RDAP**). A single-label name (e.g. `gateway.`) keeps the dot, so the DNS suffix isn't appended, except for **Remote Desktop**, **PuTTY**, **TigerVNC** and **PowerShell**, which resolve it via Windows. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper), e.g. to read a long hostname. The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**Network Interface**

- Fixed errors when changing the network configuration (e.g. missing permissions) not being shown — the change was reported as successful although it failed. The error message of Windows is now shown. [#3606](https://github.com/BornToBeRoot/NETworkManager/issues/3606)

**Connections**

- Fixed the auto refresh continuing in the background after switching to another tool, or after it was disabled, if this happened while a refresh was running. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)
- Fixed the hostname of the remote address (DNS `PTR`) being shown with a trailing dot (e.g. `server.example.com.`), also in the export. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper). The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**Listeners**

- Fixed the auto refresh continuing in the background after switching to another tool, or after it was disabled, if this happened while a refresh was running. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper). The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**Neighbor Table**

- Fixed adding and deleting entries not working until the application was restarted if loading the PowerShell module `NetTCPIP` failed once. [cc05937](https://github.com/BornToBeRoot/NETworkManager/commit/cc05937a7de2bf1247621845f47077f3796b9bb2)
- Fixed the auto refresh continuing in the background after switching to another tool, or after it was disabled, if this happened while a refresh was running. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper). The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**WiFi**

- Fixed the auto refresh continuing in the background after switching to another tool, or after it was disabled, if this happened while a refresh was running. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)
- Fixed the auto refresh not being resumed after switching back from another tool. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper). The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**Dashboard**

- Fixed the **Network Connection** widget running a full connection check on every application startup, even if the Status Window or Dashboard was never opened. It now only checks when actually shown - via the tray icon, on a network change, or when the Dashboard tab is opened. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Fixed the DNS status (Computer/Router/Internet) in the **Network Connection** widget showing as an error when no PTR record exists for the address, which is common and expected for private IP ranges. This is now shown as informational instead of critical. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Fixed a race condition in the **Network Connection** widget where results from a superseded check could overwrite the results of a newer, still-running check after quickly reopening the widget. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Fixed the Status Window closing by itself after being opened from the tray icon (or with the hotkey), if it was opened automatically on a network change shortly before. [#3639](https://github.com/BornToBeRoot/NETworkManager/pull/3639)
- Fixed the DNS hostname (`PTR`) in the **Network Connection** widget being shown with a trailing dot (e.g. `router.example.com.`). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

**DNS Lookup**

- Fixed **Inherit host from general** of a profile being saved with the value of the **Traceroute**, so the host of the DNS lookup could be replaced with the host of the profile. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed a DNS server opened with the run command in another case than configured (e.g. `cloudflare` for `Cloudflare`) being looked up as hostname instead of using the configured DNS server. Names of DNS servers are case-insensitive now. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed **Add DNS suffix (primary) to hostname** silently appending no suffix at all on machines without a Windows primary DNS suffix configured (e.g. not domain-joined), even though a usable suffix was available via the active network adapter's connection-specific DNS suffix (shown under **DNS Suffix Search List** in `ipconfig /all`). It now falls back to that adapter's suffix in this case. [#3591](https://github.com/BornToBeRoot/NETworkManager/pull/3591)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper) in the result and the DNS server profiles in the settings. The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)
- Fixed a technical error (*An invalid IP address was specified*) being shown for a hostname looked up with `PTR`. A message is shown and the hostname is skipped, the other hosts are looked up. [#3667](https://github.com/BornToBeRoot/NETworkManager/pull/3667)

**IP Scanner**

- Fixed NetBIOS lookups (computer name, domain/workgroup, user name) not starting until a host's entire port scan had finished, since the port scan wasn't actually running asynchronously despite being awaited alongside it. Ping, port scan, and NetBIOS resolution now genuinely run concurrently for every host. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Fixed the application becoming unresponsive (including the window not reacting to input) during a large scan (e.g. a /24). Scan results and progress were both being pushed to the UI one item/update at a time via a dispatcher call per host, which could flood the UI thread's message queue on large scans. Both are now batched and flushed periodically (every 150ms) instead. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Fixed every host being shown as unreachable when the ICMP **Buffer** was set to more than `65500` bytes (Windows limit). The maximum is now `65500`, higher values are lowered to `65500`. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the ICMP **Attempts** setting being ignored: a host was shown as unreachable after the first lost ping. Another ping is now sent if the previous one timed out. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the scan getting stuck (button disabled, scan never ends) when it was canceled right before the scan started, and when an unexpected error occurred. Errors are now shown as status message. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the message "Canceled by user" being replaced by "No reachable hosts found" when the scan was canceled before the first result, and a new scan started right after canceling showing no progress or stopping too early. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the message "The following hostnames could not be resolved" being replaced by "No reachable hosts found". Both messages are shown now if other hosts were scanned; if no host is left, the scan is not started. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the scan getting stuck when an error occurred while the hosts or ports were prepared, and the subnet detection button staying disabled after an error. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed ports being shown as closed when there was no answer at all (e.g. host or network unreachable). Closed now means the host refused the connection, otherwise the port is timed out. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed large scans using up the local (ephemeral) ports: every open port left a connection in the `TIME_WAIT` state for several minutes, so later connections failed (also in other applications). Connections are now closed with a reset. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed hosts that block ICMP and refuse the connection on the scanned ports (TCP reset) being shown as unreachable. A refused connection is an answer from the host, so it is now shown as reachable. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed IP addresses being scanned twice if the inputs overlap (e.g. `10.0.0.1; 10.0.0.0/24`) or the same IP address is entered with different hostnames (e.g. two names of one server). [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653) [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed the application running out of memory (and the scan getting stuck) for very large inputs like `10.0.0.0/1`. A scan of more than `65,536` IP addresses must now be confirmed, more than `16,777,216` are refused with a message. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed IP ranges across `128.0.0.0` (e.g. `127.255.255.250 - 128.0.0.5`) being rejected as invalid. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed sorting by **NetBIOS reachable** not working and the column being shown with NetBIOS disabled. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed an expanded host (row details) that was only partly visible not being scrollable into view: the results scrolled by whole rows, now by pixel. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed **Copy** > **Hostname** / **MAC address** / **Vendor** being hidden in the context menu while the column was shown (resolved via NetBIOS only), and **Copy** > **Status** copying `True` / `False` instead of `Up` / `Down`. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed a hostname received from the network (DNS PTR record or NetBIOS name) with special characters like `&` being able to run additional commands through a custom command with `{{Hostname}}` (e.g. `cmd.exe /c ...`). Such a hostname is now replaced with an empty value. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed **Edit** (`F2`) and **Delete** (`Del`) of a custom command in the settings being available without a selected command. The delete confirmation shows the name of the command. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed several custom commands being selectable in the settings while **Edit** and **Delete** only used one of them. Several custom commands can now be deleted at once (context menu or `Del`), the confirmation lists them; **Edit** needs exactly one selected command. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed columns, **Copy** menu entries and row details being shown or hidden according to the current settings instead of the settings the shown scan was started with (e.g. **MAC address** column shown, but empty, after enabling **Resolve MAC address and vendor** without a new scan). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed hostnames resolved via DNS (`PTR`) being shown with a trailing dot (e.g. `server.example.com.`), also in the export and when opened in another feature (e.g. **DNS Lookup**, **Remote Desktop**). A single-label name (e.g. `gateway.`) keeps the dot, so the DNS suffix isn't appended, except for **Remote Desktop**, **PuTTY**, **TigerVNC** and **PowerShell**, which resolve it via Windows. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed a NetBIOS name (shown as hostname if there is no DNS hostname) being passed to other features (e.g. **Remote Desktop**, **Port Scanner**, **DNS Lookup**, **RDAP**), where it may not be resolvable via DNS. The DNS hostname is used, otherwise the IP address; **DNS lookup...** / **RDAP...** > **Hostname** need a DNS hostname. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed the progress and the up / down counters possibly staying below the final values (e.g. 255 of 256 hosts scanned) when the results of several hosts arrived at the same time. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed the application crashing when the results were sorted by **MAC address** and a host had no MAC address (e.g. a host behind a router, or while the MAC address was still resolved). Hosts without a MAC address are shown at the end. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed the sort arrow of the previously sorted column still being shown after sorting by **IP address**, **MAC address** or **Time**. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed **Copy** > **Ports** being shown in the context menu with the port scan disabled (it copied nothing). [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed IPv6 hosts waiting for the NetBIOS timeout (NetBIOS doesn't exist over IPv6). NetBIOS is only requested from IPv4 addresses. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed columns that couldn't be made wider (by dragging or double-clicking the gripper) if the window was not wide enough, e.g. to see a long hostname. The last column still fills the remaining width, other columns can now grow beyond the window with a horizontal scrollbar. This also applies to the custom commands in the settings. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**Port Scanner**

- Fixed the application becoming unresponsive (including the window not reacting to input) during a large scan. Scan results and progress were both being pushed to the UI one item/update at a time via a dispatcher call per port, which could flood the UI thread's message queue on large scans. Both are now batched and flushed periodically (every 150ms) instead. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- Fixed the scan getting stuck (button disabled, scan never ends) when it was canceled right before the scan started, and when an unexpected error occurred. Errors are now shown as status message. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the message "Canceled by user" being replaced by "No open ports found" when the scan was canceled before the first result, and a new scan started right after canceling showing no progress or stopping too early. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the message "The following hostnames could not be resolved" being replaced by "No open ports found". Both messages are shown now if other hosts were scanned; if no host is left, the scan is not started. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the scan getting stuck when an error occurred while the hosts or ports were prepared. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed ports being shown as closed when there was no answer at all (e.g. host or network unreachable). Closed now means the host refused the connection, otherwise the port is timed out. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed large scans using up the local (ephemeral) ports: every open port left a connection in the `TIME_WAIT` state for several minutes, so later connections failed (also in other applications). Connections are now closed with a reset. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed IP addresses being scanned twice if the inputs overlap (e.g. `10.0.0.1; 10.0.0.0/24`) or the same IP address is entered with different hostnames (e.g. two names of one server). [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653) [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed the application running out of memory (and the scan getting stuck) for very large inputs like `10.0.0.0/1`. A scan of more than `65,536` ports (hosts × ports) must now be confirmed, more than `16,777,216` are refused with a message. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed ports being scanned twice if they are part of multiple inputs (e.g. `80; 1-100` or multiple port profiles). [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed IP ranges across `128.0.0.0` (e.g. `127.255.255.250 - 128.0.0.5`) being rejected as invalid. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the application crashing when the results of multiple hosts were sorted by IP address. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the application crashing ("Width and Height must be non-negative") when hosts were collapsed or expanded while a scan was running. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Results are sorted by IP address and port (before, in the order the ports were finished), and the hostname is resolved while the ports are scanned instead of before. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Fixed the **Hostname** column and **Copy** > **Hostname** being shown or hidden according to the current settings instead of the settings the shown scan was started with. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed hostnames resolved via DNS (`PTR`) being shown with a trailing dot (e.g. `server.example.com.`), also in the group header, the export and when opened in another feature (e.g. **DNS Lookup**, **Remote Desktop**). A single-label name (e.g. `gateway.`) keeps the dot, so the DNS suffix isn't appended, except for **Remote Desktop**, **PuTTY**, **TigerVNC** and **PowerShell**, which resolve it via Windows. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed the progress and the open / closed counters possibly staying below the final values (e.g. 1023 of 1024 ports scanned) when the results of several ports arrived at the same time. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper) in the result and the port profiles (settings). The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)
- Fixed the ports of different hosts with the same hostname (DNS `PTR`, e.g. a generic name of a provider or a server with two IP addresses) being shown in one group. The results are grouped per IP address, the group header shows the hostname and the IP address. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed **Edit** (`F2`, double-click) and **Delete** (`Del`) of a port profile in the settings being available without a selected profile (edit created a new profile). The delete confirmation shows the name of the profile. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)

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
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper) in the subnetting result. The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**Profiles**

- Fixed `Enter` in the search box of the import result importing the profiles (default button). It filters the found profiles right away now, instead of after the delay while typing. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper) in the groups (Settings > Profiles) and the import result. The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)
- Fixed the names of the features in the group dialog shown without spaces (e.g. `RemoteDesktop` instead of **Remote Desktop**). [#3667](https://github.com/BornToBeRoot/NETworkManager/pull/3667)
- Fixed the profiles of a group renamed only by case (e.g. `servers` → `Servers`) keeping the old group name. Opening them in a feature with group settings (e.g. **Remote Desktop**, **PuTTY**) failed, and editing one could create a second group with the old name. Profile files with such profiles are repaired when they are loaded. [#3667](https://github.com/BornToBeRoot/NETworkManager/pull/3667)

**SNMP**

- Fixed `Enter` in the search box of the OID profiles dialog closing the dialog with **OK**. It only filters the OID profiles now. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper) in the result and the OID profiles (dialog, settings). The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**SNTP Lookup**

- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper) in the result and the SNTP server profiles in the settings. The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**RDAP**

- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper) in the result and the bootstrap files in the settings. The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**Lookup**

- Fixed columns that couldn't be made wider than the window (by dragging or double-clicking the gripper) in the OUI and port lookup. The grid scrolls horizontally now, the last (or main) column still fills the remaining width. [#3557](https://github.com/BornToBeRoot/NETworkManager/issues/3557)

**Web Console**

- Fixed no message being shown after **Delete browsing data** in the settings (whether the browsing data was deleted or an error occurred), and an error (e.g. the WebView2 runtime is not installed) not being handled.

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Dependabot updates the NuGet packages again (no updates since February 2025). The Windows SDK version is now set via `TargetPlatformVersion` instead of in the target framework (`net10.0-windows10.0.22621.0` → `net10.0-windows`), which Dependabot can't handle ([dependabot-core#13278](https://github.com/dependabot/dependabot-core/issues/13278)). The build output moved from `bin\...\net10.0-windows10.0.22621.0\` to `bin\...\net10.0-windows\`. [1ce8310](https://github.com/BornToBeRoot/NETworkManager/commit/1ce8310a876f0d5bf87d0b2e098bcb22d15dd5a9)
- Context menus of lists (results, profiles, settings) get the view model in XAML instead of when they are opened. Their items, and menu items that are created outside of a menu (e.g. the extra items of the profile context menu), no longer cause XAML binding errors every time a list is filled, which hid real binding errors.
- Converted the **IP Scanner** and **Port Scanner** engines from `Parallel.ForEach` with blocking calls to genuinely asynchronous `Parallel.ForEachAsync`, removing the risk of exhausting the application's ThreadPool at high concurrency settings. [#3564](https://github.com/BornToBeRoot/NETworkManager/pull/3564)
- The TCP port probe (**IP Scanner**, **Port Scanner**) and the NetBIOS request (**IP Scanner**) report a refused connection, a timeout or no answer as result instead of throwing an exception for each of them (thousands per scan). Faster, and the application no longer lags while scanning with a debugger attached. [#3653](https://github.com/BornToBeRoot/NETworkManager/pull/3653)
- Refactored the **Network Connection** widget's check logic to reduce duplicated code and share the local IP address detection between the Computer and Router checks instead of detecting it twice. [#3553](https://github.com/BornToBeRoot/NETworkManager/pull/3553)
- Consolidated the duplicated **Profiles** side panel (search, tag filter, grouped list, context menu, add/edit/copy/delete) used across 15 tool views into a single shared control, reducing code duplication. As part of this, the profile panel's expanded/width state is now shared across all tools instead of being tracked per tool. [#3537](https://github.com/BornToBeRoot/NETworkManager/pull/3537)
- Replaced the internal event system for opening a feature with data (e.g. **Open in RDAP** from the **IP Scanner**) with typed requests, which can also carry options (e.g. DNS server and record type) and profiles. Features no longer fail to open from another feature while the application list is filtered by a search. [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)
- Input histories (e.g. hosts, domains, ports) are updated in place instead of being cleared and refilled, which avoids clearing the text of the input field and unnecessary updates. [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)
- Added a unit test project (xUnit) with tests for the run command parser and the history lists. [#3644](https://github.com/BornToBeRoot/NETworkManager/pull/3644)
- Reworked the GitHub issue templates as forms (bug report, feature request, question) that ask for the relevant information (e.g. affected feature, version, installation, Windows version, log), with links to the documentation, discussions, translations and security advisories. Simplified the pull request template and removed the Copilot summary section. [#3660](https://github.com/BornToBeRoot/NETworkManager/pull/3660)
- Faster CI: AppVeyor no longer builds a branch a second time when a PR is open, skips changes that only affect the documentation / website, builds ARM64 only for releases and now runs the unit tests. The website is only built for PRs that change it and builds about 3x faster (Docusaurus Faster). Dependabot also updates the GitHub Actions, and CodeQL also scans the workflows. Website changes are only merged automatically if the website builds. Dependabot major updates are no longer merged automatically. Removed the stale bot. [#3675](https://github.com/BornToBeRoot/NETworkManager/pull/3675)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
