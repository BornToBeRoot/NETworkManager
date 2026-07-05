---
sidebar_position: 0
description: "Changelog for the next NETworkManager release — upcoming features, improvements, and bug fixes."
keywords:
  [
    NETworkManager,
    changelog,
    release notes,
    next release,
    upcoming features,
    bug fixes,
  ]
---

# Next Release

Version: **Next release** <br />
Release date: **xx.xx.2025**

| File | `SHA256` |
| ---- | -------- |

**System requirements**

- Windows 10 / Server x64 (22H2 or later)
- [.NET Desktop Runtime 10.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime)

## Breaking Changes

- **ARP Table** has been renamed to **[Neighbor Table](../application/neighbor-table.md)**. The application list entry is automatically migrated on first launch. Other view settings (auto-refresh interval, export file type/path) reset to their defaults. [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)
- **IP Scanner** export: The `ARPMACAddress` and `ARPVendor` columns have been removed from CSV, XML and JSON exports. Use `MACAddress` and `Vendor` instead, which contain the same value (ARP/NDP preferred, NetBIOS as fallback). [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)
- **IP Scanner** custom commands: The placeholder syntax has changed from `$$ipaddress$$` / `$$hostname$$` to `{{IPAddress}}` / `{{Hostname}}`. Existing custom commands are automatically migrated on first launch.

## What's new?

**Dashboard**

- New **Speed Test** widget to measure download/upload speed, latency, and jitter against [`speed.cloudflare.com`](https://speed.cloudflare.com/). The test is user-initiated and shows download (Mbps), upload (Mbps), latency (ms), jitter (ms), ISP, and server location. A privacy disclaimer is shown before use. [#3440](https://github.com/BornToBeRoot/NETworkManager/pull/3440)

**WiFi**

- Added **Channel Width** column (in MHz) to the network list. Channel bandwidth is retrieved via the native `WlanApi`, bypassing the `Windows.Devices.WiFi` API limitation. Typical values: 20, 40, 80, 160 MHz. [#3462](https://github.com/BornToBeRoot/NETworkManager/pull/3462)
- The **Channels** tab now supports 6 GHz networks and is split into two sub-tabs: **2.4 & 5 GHz** and **6 GHz**. The 6 GHz view uses separate lower (channels 1–125) and upper (channels 129–233) charts for readability. [#3462](https://github.com/BornToBeRoot/NETworkManager/pull/3462)

**PowerShell**

- DPI scaling is now applied correctly when NETworkManager is moved to a monitor with a different DPI scaling factor. The embedded PowerShell (conhost) window now rescales its font automatically using the Windows Console API (`AttachConsole` + `SetCurrentConsoleFontEx`), bypassing the OS limitation that prevents `WM_DPICHANGED` from being forwarded to cross-process child windows. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

**PuTTY**

- DPI scaling is now applied correctly when NETworkManager is moved to a monitor with a different DPI scaling factor. The embedded PuTTY window now receives an explicit `WM_DPICHANGED` message with the new DPI value packed into `wParam`, since the OS does not forward this message across process boundaries after `SetParent`. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

**Firewall**

- New feature to quickly add, edit, enable, disable and delete NETworkManager-owned firewall rules. Managed rules are prefixed with `NETworkManager_` in the Windows Firewall. (See the [documentation](https://borntoberoot.net/NETworkManager/docs/application/firewall) for more details) [#3383](https://github.com/BornToBeRoot/NETworkManager/pull/3383)

**[Neighbor Table](../application/neighbor-table.md)** (formerly ARP Table)

- IPv6 (NDP) neighbor entries are now shown in addition to IPv4 (ARP). [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)
- New **Interface** and **State** columns (sortable, searchable). [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)
- Add entry dialog now accepts IPv4 and IPv6 addresses. [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)
- View is read-only when not running elevated; modifying the table requires elevated rights. [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)

**Profiles**

- Profiles can now be imported from **Active Directory**. Search for computers by name using an AD query, select the results, assign a group, and apply connection settings (RDP, SSH, etc.) before importing. [#3368](https://github.com/BornToBeRoot/NETworkManager/pull/3368)
- Profiles can now be imported from a **CSV file**. Select a `.csv` file (drag & drop or browse) with `Name;Host` entries and an optional description column, then select the results, assign a group, and apply connection settings (RDP, SSH, etc.) before importing. The delimiter (semicolon, comma or tab) and an optional header row are detected automatically, and re-importing the same file detects already imported entries. [#3502](https://github.com/BornToBeRoot/NETworkManager/pull/3502)

**Ping Monitor**

- New **status change notifications**: when a monitored host goes up or down, a stackable popup appears in the bottom-right corner of the primary screen and/or an optional system sound is played. Configurable **success** and **failure thresholds** suppress noise from flapping hosts, the initial state is established silently, and clicking a popup brings the main window to the front. When many hosts change state at once, every host still shows its own popup but the sound is played only once. (See the [documentation](https://borntoberoot.net/NETworkManager/docs/application/ping-monitor#notifications) for more details) [#3471](https://github.com/BornToBeRoot/NETworkManager/pull/3471)

**Remote Desktop**

- New **View only** mode to monitor a session without sending input: keyboard and mouse are blocked while the screen keeps updating. It can be enabled in the connect dialog, per profile/group, and globally (inherited Global → Group → Profile), or toggled on the fly via the tab's right-click menu. An eye icon on the tab indicates when a session is view-only, and the **Fullscreen** action and all **Keyboard shortcuts** are disabled while it is active to prevent bypassing it. [#3482](https://github.com/BornToBeRoot/NETworkManager/pull/3482)
- The tab context menu's **Keyboard shortcuts** submenu now also includes **Task Manager** (`Ctrl+Shift+Esc`), **Lock** (`Win+L`), **Show Desktop** (`Win+D`), **Explorer** (`Win+E`) and **Run dialog** (`Win+R`), in addition to the existing **Ctrl+Alt+Del**. Like Ctrl+Alt+Del, these are sent directly via scan codes into the remote session, bypassing local key interception. [#3500](https://github.com/BornToBeRoot/NETworkManager/pull/3500)

**Language**

- New language Luxembourgish (`lb-LU`) has been added. [#3490](https://github.com/BornToBeRoot/NETworkManager/pull/3490)

## Improvements

**WiFi**

- Channel width is now visualized in the channel charts as a proportional band, making overlapping networks easier to identify. [#3462](https://github.com/BornToBeRoot/NETworkManager/pull/3462)
- Migrated the WiFi channel charts from LiveCharts to LiveCharts2. [#3462](https://github.com/BornToBeRoot/NETworkManager/pull/3462)

**IP Scanner**

- MAC address resolution now uses ARP (IPv4) or NDP (IPv6) from the neighbor cache, with NetBIOS as fallback. The detail panel shows a single **MAC Address** section instead of separate ARP and NetBIOS entries. [#3403](https://github.com/BornToBeRoot/NETworkManager/pull/3403)

**Profiles**

- **WebConsole** URL, **PowerShell** additional command line, and **PuTTY** additional command line now support a `{{Host}}` placeholder (e.g. `https://{{Host}}/`) that is resolved to the profile's host each time a connection is established, so it always reflects the current host even after later edits. For PowerShell and PuTTY, the placeholder also resolves in the ad-hoc **Connect** dialog, using the host entered there. A help icon next to each field shows the available placeholder. [#3511](https://github.com/BornToBeRoot/NETworkManager/pull/3511)
- New profiles pre-fill the **WebConsole** URL with `https://{{Host}}`, and profile import (CSV, Active Directory) can now also enable **WebConsole** for imported profiles. [#3511](https://github.com/BornToBeRoot/NETworkManager/pull/3511)

**Dashboard**

- Added a **Refresh** button (with animated feedback) to the **Network Connection**, **IP Geolocation**, and **DNS Resolver** widgets. The global reload button in the Status Window has been removed, as each widget now has its own. [#3447](https://github.com/BornToBeRoot/NETworkManager/pull/3447)
- Added a tooltip to the **Speed Test** widget chart showing download/upload speed values on hover. [#3449](https://github.com/BornToBeRoot/NETworkManager/pull/3449)
- Redesign Status Window to make it more compact. [#3359](https://github.com/BornToBeRoot/NETworkManager/pull/3359)

**Network Interface**

- Added Network Profile (domain, private, public) information to the Network Interface details view, if available. [#3383](https://github.com/BornToBeRoot/NETworkManager/pull/3383)
- Migrated the bandwidth chart from LiveCharts to LiveCharts2. Added a tooltip showing the download/upload speed on hover. [#3457](https://github.com/BornToBeRoot/NETworkManager/pull/3457)
- The bandwidth chart is now interactive: zoom with the mouse wheel, pan by dragging with the left mouse button, and zoom into a section by dragging with the right mouse button. While inspecting, the chart pauses auto-scrolling and a **Live** button returns it to live mode. The visible time window is now configurable via the new **Chart time** setting (default 60 seconds). [#3457](https://github.com/BornToBeRoot/NETworkManager/pull/3457)
- Reworked the network usage statistics: byte counts are now shown in a human-readable format (exact bytes on hover) and the speed is shown in bit/s (byte/s on hover). [#3457](https://github.com/BornToBeRoot/NETworkManager/pull/3457)
- Configuration commands now run in-process via `SMA.PowerShell.Create()` (using the same `netsh`/`ipconfig` commands as before) instead of spawning an elevated `powershell.exe` process per action. The **Configure** tab now shows a **Restart as Administrator** banner when the app is not elevated — all write operations are blocked until the app runs as admin. [#3499](https://github.com/BornToBeRoot/NETworkManager/pull/3499)
- A success message is now shown in the status bar after **Flush DNS cache**, **Release & Renew**, **Apply**, **Add IPv4 address**, and **Remove IPv4 address** complete successfully. [#3499](https://github.com/BornToBeRoot/NETworkManager/pull/3499)
- **Flush DNS cache** and **Release & Renew** buttons have been moved from the **Information** tab to the **Configure** tab, as they also require elevated rights. [#3499](https://github.com/BornToBeRoot/NETworkManager/pull/3499)

**Ping Monitor**

- Migrated charts from LiveCharts to LiveCharts2. Added a tooltip showing the ping time on hover. [#3449](https://github.com/BornToBeRoot/NETworkManager/pull/3449)
- The latency chart is now interactive: zoom with the mouse wheel, pan by dragging with the left mouse button, and zoom into a section by dragging with the right mouse button. While inspecting, the chart pauses auto-scrolling and a **Live** button returns it to live mode. The visible time window is now configurable via the new **Chart time** setting (default 2 minutes). [#3453](https://github.com/BornToBeRoot/NETworkManager/pull/3453)
- Export is now triggered by right-clicking a host directly (instead of the list context menu), so right-clicking the chart can be used to zoom into a section. [#3453](https://github.com/BornToBeRoot/NETworkManager/pull/3453)

**Discovery Protocol**

- Added support for `F5` and `Enter` keys to start capturing network packets. [#3383](https://github.com/BornToBeRoot/NETworkManager/pull/3383)
- Redesigned the "restart as admin" note to be more compact and visually consistent. [#3383](https://github.com/BornToBeRoot/NETworkManager/pull/3383)

**Hosts File Editor**

- Button to open the hosts file in the default text editor added. [#3383](https://github.com/BornToBeRoot/NETworkManager/pull/3383)

## Bug Fixes

**General**

- Fixed the last column of various DataGrids not resizing to fill the available view width. [#3417](https://github.com/BornToBeRoot/NETworkManager/pull/3417)
- Fixed `CancellationTokenSource` leak in `IPScanner`, `PortScanner`, `Traceroute`, `PingMonitor`, `PingMonitorHost` and `SNMP` ViewModels. The previous instance was never disposed before being overwritten on each run, leaking the underlying `WaitHandle`. [#3448](https://github.com/BornToBeRoot/NETworkManager/pull/3448)
- Fixed a `Dispatcher.ShutdownStarted` handler leak in the Dragablz tab items (PowerShell, PuTTY, TigerVNC, Remote Desktop and Web Console controls, plus the IP Scanner, Port Scanner, Traceroute, DNS Lookup, IP Geolocation, SNMP, SNTP Lookup and Whois views). The handler was subscribed in the constructor but never removed, keeping each closed tab (view and view model) alive until the application exited. It is now unsubscribed in `CloseTab()`; the Web Console additionally disposes its WebView2 instance. [#3454](https://github.com/BornToBeRoot/NETworkManager/pull/3454)

**WiFi**

- Fixed the WiFi view silently failing to load (e.g. on Windows ARM64 running the emulated x64 build) when `WiFiAdapter.RequestAccessAsync()` threw an exception. The exception is now logged and the view shows the "access not available" message with a settings button instead of an empty tab. [#3462](https://github.com/BornToBeRoot/NETworkManager/pull/3462)

**Port Scanner**

- Fixed an app crash when double-clicking a port profile. [#3382](https://github.com/BornToBeRoot/NETworkManager/pull/3382)

**PowerShell**

- Fixed incorrect initial embedded window size on high-DPI monitors. The `WindowsFormsHost` panel now sets its initial dimensions in physical pixels using the current DPI scale factor, ensuring the PowerShell window fills the panel correctly at startup. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

**PuTTY**

- Fixed incorrect initial embedded window size on high-DPI monitors. The `WindowsFormsHost` panel now sets its initial dimensions in physical pixels using the current DPI scale factor, ensuring the PuTTY window fills the panel correctly at startup. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

**Dashboard**

- Fixed the Status Window auto-close timer firing even when the window was opened manually. The `enableCloseTimer` parameter is now respected, so a manually opened Status Window stays open while the one shown automatically on a network change still closes after the configured time. [#3471](https://github.com/BornToBeRoot/NETworkManager/pull/3471)

**Ping Monitor**

- Fixed the **Status change** field showing the time of day formatted as if it were a duration (e.g. `02h 30m 25s` for 14:30:25). It now correctly shows the time of the last status change as `HH:mm:ss`. [#3471](https://github.com/BornToBeRoot/NETworkManager/pull/3471)

**Network Interface**

- Fixed `Renew6Action` incorrectly calling `ipconfig /renew` (IPv4) instead of `ipconfig /renew6` (IPv6) when renewing the IPv6 address. [#3441](https://github.com/BornToBeRoot/NETworkManager/pull/3441)
- Bandwidth measurement now includes IPv6 traffic (previously IPv4 only), derives a time-accurate speed, and no longer crashes or shows spikes on adapter errors or interface counter resets. [#3457](https://github.com/BornToBeRoot/NETworkManager/pull/3457)

**TigerVNC**

- Fixed incorrect initial embedded window size on high-DPI monitors. The `WindowsFormsHost` panel now sets its initial dimensions in physical pixels using the current DPI scale factor, ensuring the TigerVNC window fills the panel correctly at startup. [#3352](https://github.com/BornToBeRoot/NETworkManager/pull/3352)

## Dependencies, Refactoring & Documentation

- Migrated from `LiveCharts` to `LiveCharts2` (`LiveChartsCore.SkiaSharpView.WPF`) for chart rendering. [#3449](https://github.com/BornToBeRoot/NETworkManager/pull/3449) [#3457](https://github.com/BornToBeRoot/NETworkManager/pull/3457) [#3462](https://github.com/BornToBeRoot/NETworkManager/pull/3462)
- Replace fire-and-forget `.ConfigureAwait(false)` calls with explicit discard assignments (`_ = SomeAsyncOperation()`) across command handlers, startup/load paths and profile callbacks. [#3441](https://github.com/BornToBeRoot/NETworkManager/pull/3441)
- Code cleanup & refactoring
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
