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

**Dashboard**

- A custom public IP address API (**Settings > Dashboard**) must return the IP address as plain text or in a JSON object (e.g. `{"ip":"xx.xx.xx.xx"}`). An address inside other text or an HTML page (e.g. `checkip.dyndns.org`) is no longer used, because the page of a hotspot / captive portal could be shown as the public IP address. See the [examples](../application/dashboard.md#use-custom-ipv4-address-api). [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)

## What's new?

**Network Interface**

- Network Interface profiles can be bound to a network interface (**Bind to network interface** in the profile): the profile is always applied to this network interface, selecting the profile also selects it. If the network interface is not found on this computer, a dialog shows its name and ID and offers to apply the profile to the selected network interface instead. [#146](https://github.com/BornToBeRoot/NETworkManager/issues/146)

**mDNS Discovery**

- Added a dedicated browser for services advertised on the local network, including printers, scanners, web servers and media devices. Select an interface and service types or service type profiles (e.g. Printers, editable in the settings, with history) or discover all types, inspect hostnames, ports, IPv4 / IPv6 addresses and TXT records, filter, copy and export the results. **Raw DNS records** shows the individual DNS records instead, with each TXT entry on its own row; other and unknown record types are preserved as raw data. Includes the `mdns` / `dnssd` run commands (e.g. `mdns printers 30`) with suggestions of the service type profiles and durations. Thanks to [@MovGP0](https://github.com/MovGP0) [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**Dashboard**

- The widgets of the dashboard (Network connection, Speed Test, IP geolocation, DNS resolver) can be hidden in the settings (**Settings > Dashboard > Widgets**). A hidden widget doesn't check anything (a running speed test is canceled). [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)

## Improvements

**Network Interface**

- The tooltip of a profile shows the bound network interface and the IP address / subnet mask (or **Obtain an IP address automatically**) instead of the host, which is not used by **Network Interface**. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- **Add profile...** on the **Configure** tab creates a profile with the shown configuration, bound to the selected network interface, e.g. to save the current configuration and apply it again later. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- **Reset** (icon next to the **Configure** header) shows the current configuration of the selected network adapter again, e.g. after a profile was selected. Static values of a previously selected adapter are no longer shown for an adapter that uses DHCP. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Double-click a profile (or press `Enter`) to fill in its values on the **Configure** tab again, e.g. after **Reset** or if the profile was selected automatically. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- The run command accepts a profile: `networkinterface @profile` selects the profile and fills in its values on the **Configure** tab (like a double-click on the profile), it is applied with **Apply**. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Screenshot mode (`--screenshot`): sample network adapters and a filled bandwidth chart instead of your own adapters and a measurement. Configuration actions only show their result, nothing is changed. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- **Refresh** button next to **Network connections...** when no network adapter was found, like in **WiFi** (e.g. after an adapter was enabled). [2f8a5d5](https://github.com/BornToBeRoot/NETworkManager/commit/2f8a5d597e2c06205e4f214867c759891537fe0a)

**WiFi**

- **Network connections...** button next to **Refresh** when no WiFi adapter was found (e.g. to enable a disabled adapter), like in **Network Interface**. [daa27ab](https://github.com/BornToBeRoot/NETworkManager/commit/daa27ab438be4680ceb5b264b4a53d6e8ab4570b)

**LLDP / CDP Capture**

- Renamed "Discovery Protocol" to "LLDP / CDP Capture", so it is clear what the tool does (and it is not confused with mDNS Discovery). The run command is now `lldpcdpcapture` with the aliases `lldp` and `cdp`, the documentation moved to [LLDP / CDP Capture](../application/lldp-cdp-capture.md). The selected protocol and duration are reset to the defaults. [#3721](https://github.com/BornToBeRoot/NETworkManager/issues/3721)

**Dashboard**

- Switching back to the dashboard checks the network connection only if the last check is at least 60 seconds old (or the network / a setting of the network connection changed), instead of on every switch between the tools. **F5** and the refresh button always check. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- The network connection check is faster (also at startup and in the status window): the local IP address and the gateway are read directly from the network adapters instead of querying all adapter details incl. PowerShell up to four times per check. This also speeds up the detection of the local IP address in the **IP Scanner** when no route is available. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- The network connection shows a warning instead of OK if the computer has only a link-local IPv4 address (`169.254.x.x`, no address from a DHCP server) or no route to the internet. Without an IPv6 connection (e.g. the internet provider doesn't support IPv6), the IPv6 values are shown as information (gray) instead of failed (red), and the IPv6 address of the router and the internet are not checked. Hover over an icon to see the reason. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- The DNS name (PTR) of the IPv6 address is no longer requested after the DNS server failed for the IPv4 address (the same server would time out again), so the network connection check finishes faster without a working DNS server. Without an IP address to resolve, the DNS value is shown as information instead of failed. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- The gateway / router is now pinged (2 attempts, 2 seconds timeout) instead of only checking that a gateway is configured. No reply is shown as a warning (some routers block ICMP). A network adapter without a gateway (e.g. a VPN connection) is shown as a warning instead of failed if the internet is routed via it. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- The internet is now checked with a ping of the public IPv4 / IPv6 address (settings, `1.1.1.1` / `2606:4700:4700::1111`), the public IP address API and the internet connectivity reported by Windows. A hotspot / captive portal that requires a sign-in, a blocked public IP address API (e.g. a proxy) or blocked ICMP are shown as a warning instead of the internet being not reachable. With **Check public IP address** disabled, the internet is still checked (ping and Windows) instead of showing "Check is disabled!". [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- The values of the network connection (dashboard and status window) can be opened in other tools via the context menu: Port Scanner, Ping Monitor, Traceroute and DNS Lookup, the gateway / router also in the Web Console, the public IP address also in RDAP and IP Geolocation. **Copy** copies the whole value. Opening a tool from the status window shows the main window. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- A value of the network connection that is cut off in a narrow window (e.g. a long IPv6 address or DNS name) is shown completely in the tooltip. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- The network connection shows the time of the last check ("Reloaded at ...") next to the refresh button, like other tools. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- New **Copy all** button in the network connection: copies all values with the reasons of the states as text, e.g. for a support request. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- The speed test measures like the official [cloudflare/speedtest](https://github.com/cloudflare/speedtest) library, so the result is comparable with [speed.cloudflare.com](https://speed.cloudflare.com/): a download / upload direction ends after a whole set of requests in which even the fastest request took longer than one second (instead of after the first request of one second, e.g. after TCP slow start, which skipped the larger and more accurate sizes), and a few latency probes run between the sets (two instead of one at the start). The download includes an estimate of the HTTP headers (0.5 %, like the upload). The server time is read from the request duration (`cfRequestDuration`) of the `server-timing` header instead of the first duration of any metric, without it no server time is subtracted (instead of 10 ms). [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- The speed test shows below its header when it was canceled or failed (translated, e.g. no response within 15 seconds or the HTTP status code) instead of replacing the measured values with the English error text of .NET: the values measured until then stay visible. The phase of the running test is no longer shown in the accent color. [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- The speed test shows the time of the test ("Last test at ...") next to its buttons, like the network connection, so an old result is recognized (the tooltip shows the date). A canceled or failed test shows no time. [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- New **Copy all** button in the speed test: copies download, upload, latency, jitter, ISP, server and the time of the test as text, like the network connection. [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- The IP geolocation widget shows when ip-api.com can be requested again after its rate limit was reached ("Rate limit reached! Try again in ... seconds", like the IP Geolocation tool), as a warning below its header. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)

**Status window**

- The countdown of the status window moves smoothly (like the progress of the IP Scanner) instead of in small steps, and the window no longer grows and moves right after it was opened on a network change. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)

**Notifications**

- The countdown of the notifications (e.g. Ping Monitor, IP Scanner) moves smoothly, like the progress of the IP Scanner, instead of in small steps. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)

## Bug Fixes

**Tabs**

- Fixed the embedded windows (e.g. PuTTY, Remote Desktop) covering the drop zones again while a tab was dragged if another window of the same tool closed during the drag. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed the content of a tab staying visible without its tab after the tab was dragged into another window and out again quickly. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab dropped into an empty tab bar (or far behind the last tab) being dragged out again right away. A tab now leaves the tab bar only when it is dragged out of the whole tab bar. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed the tab title (e.g. the host of a scan) not being updated when the tab is in a dragged out window that is not the active window. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- The tab headers show the focus ring of the application (keyboard navigation) instead of a dotted rectangle. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- The other tabs no longer slide to their new position while a tab is dragged if animations are disabled (Settings > Appearance > Animations). [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab (and its session, e.g. PuTTY or Remote Desktop) being lost when it was dragged out as a window after a previous drag had merged a tab into another window: the drop zone of the earlier drag was reused. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a crash while dragging a tab as a window when another tab window closed at the same time (e.g. its session ended). [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab dragged back to its original position while the other tabs were still sliding ending up at the wrong position. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed a tab dropped into another tab bar being inserted at the wrong position with display scaling above 100 %. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Fixed the splitter between tabs split on top of each other also reacting 3 px into the tabs on the left and right. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)

**Network Interface**

- Fixed **Apply** failing when the IPv4 address or the subnet mask on the **Configure** tab (or in **Add IPv4 address...**) started or ended with a space. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed **Apply** and **Add IPv4 address...** failing when the CIDR was entered without a slash (e.g. `24` instead of `/24`), although it was accepted as valid. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed values of a profile (IP address, subnet mask, gateway, DNS servers) being passed to `netsh` without checking them again. A manipulated or shared profile file could run other commands with elevated rights when the profile was applied. Invalid values are now reported and nothing is changed. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed **Apply** failing for a static IP address without a default gateway. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed the configuration being blocked (progress bar running until a restart) when **Apply** was used after the selected network adapter was removed (e.g. a USB or VPN adapter). The first adapter is selected instead. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed **Apply** in the profile context menu being available while another change was running or the selected network adapter was not connected (the **Apply** button was already disabled). [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed the **Bandwidth** chart and statistics no longer updating after the network configuration changed (e.g. after **Apply**, a DHCP renewal or connecting a VPN). [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed the **Bandwidth** measurement being restarted and the values entered on the **Configure** tab being replaced whenever the network configuration of the computer changed (e.g. a new IPv6 address or a VPN connection), although the selected network adapter stayed the same. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed the values entered on the **Configure** tab being replaced with the values of a profile after switching to another tool and back, after any profile was changed, or when searching / filtering the profiles. Only selecting a profile fills in its values. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed **Use the following DNS server addresses** being selected on the **Configure** tab when switching to **Obtain an IP address automatically**. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed profiles with a static IP address and **Obtain DNS server address automatically** (e.g. from older profile files) showing an option that could not be changed in the profile dialog. **Use the following DNS server addresses** is selected instead, as for every static IP address. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed the reload of the network adapters being blocked (or the application crashing) if reading the network adapters failed, e.g. because an adapter was removed at that moment. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed **IP Scanner** on the **Information** tab being enabled for a network adapter without IPv4 address (e.g. disconnected) and doing nothing when clicked. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed the profile dialog showing validation errors on the disabled IP address and DNS server fields (e.g. for a new profile with **Obtain an IP address automatically**). [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed the **Information** tab possibly showing the old configuration after **Apply**, **Release & Renew** or adding / removing an IP address, if the network adapters were reloaded because of a network change at the same time. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed the selected network adapter not being selected again when it is back after it was missing (e.g. a VPN or USB adapter), also after a restart. A network adapter that was configured meanwhile (e.g. **Apply**, **Release & Renew**) stays selected. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed the **Information** tab not showing the changes made before an error, e.g. when the IP address was set but setting the DNS servers failed. The network adapters are now reloaded after an error too. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed **Add IPv4 address...** and **Remove IPv4 address...** changing another network adapter (or the application crashing if no adapter was left) when the network adapter was removed while the dialog was open (e.g. a USB or VPN adapter). Nothing is changed in this case. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed **DNS autoconfiguration** on the **Information** tab (and in the export) showing **Yes** for a network adapter with a static IP address and no DNS servers (DNS servers can only be obtained automatically with DHCP), and for a network adapter without IPv4 with static IPv6 DNS servers. [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)
- Fixed **No enabled network adapters found** being shown for a moment while the network adapters were loaded (e.g. when the tool was opened). A loading indicator is shown instead, like in **WiFi**. [ee1693f](https://github.com/BornToBeRoot/NETworkManager/commit/ee1693fd3a0ab09ce35d06b49d89ae48b08be249)

**DNS Lookup**

- Fixed the last column of the DNS servers (settings) not filling the remaining width. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)
- Fixed a DNS server name with a leading / trailing space not being detected as a duplicate (settings). [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**SNTP Lookup**

- Fixed the last column of the SNTP servers (settings) not filling the remaining width. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)
- Fixed an SNTP server name with a leading / trailing space not being detected as a duplicate (settings). [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**Hosts File Editor**

- Fixed the last column (comment) not filling the remaining width. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**Settings**

- Fixed the profile files (Settings > Profiles) not filling the width of the grid: the name column fills the remaining width. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)
- Fixed a custom DNS server name (Settings > Network) with a leading / trailing space not being detected as a duplicate. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**Profiles**

- Fixed the profile list not filling the width of the grid: the name column fills the remaining width. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)
- Fixed the first profile being selected after a profile was edited, instead of the edited profile (profile list of the tools and **Settings > Profiles**). [#3728](https://github.com/BornToBeRoot/NETworkManager/pull/3728)

**Port Scanner**

- Fixed a port profile name with a leading / trailing space (e.g. `Webserver `) not being detected as a duplicate: the ports input always used the first port profile with the name. [#3719](https://github.com/BornToBeRoot/NETworkManager/pull/3719)

**About**

- Fixed the license of ControlzEx in the library list: it showed the license text and link of Dragablz. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Updated the license of MahApps.Metro in the library list (copyright: .NET Foundation and Contributors). [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)

**Dashboard**

- Fixed the network connection check getting stuck (all values loading until the next refresh) when it was started several times while a check was running (e.g. the refresh and a network change). The refresh is disabled while a check runs, a network change during a check repeats the check once it has finished. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- Fixed the network connection widget (dashboard and status window) not updating when **Check public IP address** was changed in the settings until the application was restarted: an old public IP address could be shown as current. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- Fixed the network connection not being checked again when the network changed while the dashboard was open (e.g. a cable unplugged or a VPN connected), although the documentation says so. If the dashboard is not open (or the window is in the tray / minimized, the status window checks meanwhile), it is checked when it is shown again. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- Fixed the gateway / router not being found (shown as failed although the internet works) for VPN connections whose adapter is not an Ethernet or Wi-Fi adapter (e.g. the Windows VPN client with IKEv2 / SSTP). A VPN adapter without a gateway is no longer shown with the gateway `0.0.0.0`. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- Fixed an IP address on an error page or the page of a hotspot / captive portal (e.g. hotel Wi-Fi) being shown as the public IP address. The answer of the API must have a success status code, a private IP address is shown as a warning, and an API name that can't be resolved points to the DNS server. The public IPv4 and IPv6 addresses are requested at the same time. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- Fixed custom public IPv6 address APIs that answer with a JSON object (e.g. `{"ip":"2001:db8::1"}`) not being recognized. A custom API may answer with the IP address as plain text or in a JSON object; other answers (e.g. the HTML page of a captive portal) are no longer searched for an IP address. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- Fixed the speed test failing completely when the ISP / Cloudflare server could not be requested (`speed.cloudflare.com/meta`, e.g. an error of the server or a network without an ASN). The test continues, ISP and server are shown as `-/-`. [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- Fixed the download speed of the speed test jumping above the result while the test runs (e.g. a short burst of data) and dropping at the end. The value shown is the result of the completed requests, only the first request is shown while it runs. [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- Fixed the speed test running endlessly ("Measuring download speed...") when the connection was lost during the test (e.g. Wi-Fi disconnected). A request now fails after 15 seconds without a response or data. [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- Fixed `Enter` on the dashboard starting a speed test (up to about 1 GB of data on a fast connection) and `Esc` stopping it, e.g. after clicking a value of another widget. The speed test is only started and stopped with its button. [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- Fixed one failed request (e.g. a short interruption of the connection or a proxy that rejects a large upload) ending the whole speed test. Like the official library, a failed request is skipped (a warning shows how many) and a request rate limited by Cloudflare (HTTP 429) is repeated up to three times. If all requests of a size fail, the larger sizes of the direction are skipped; the test only ends after three requests in a row without a response. [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- Fixed the server of the speed test being empty at the start of a test or shown as `-/-, -/- (-/-)` when Cloudflare did not report it. It is shown as `-/-` like the ISP, with the full name in the tooltip. [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- Fixed the sparkline of the speed test showing the first request twice (one point more than requests, the same value twice in the tooltip). [#3740](https://github.com/BornToBeRoot/NETworkManager/pull/3740)
- Fixed the IP geolocation widget showing no reason when ip-api.com could not be reached (e.g. no network connection), and the DNS resolver widget showing the English error text of .NET. Both show a translated message below their header (e.g. no response within 10 seconds or the HTTP status code), like the speed test, and a request fails after 10 seconds without a response instead of 100 seconds. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed the refresh button of the IP geolocation and DNS resolver widgets staying disabled after a check until the mouse was moved or a key was pressed. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed the IP geolocation and DNS resolver widgets showing old values after the network changed (e.g. a VPN connected: the network connection showed the new public IP address, the IP geolocation the old one). They are checked again with the network connection: after a network change, and when the dashboard is shown again and the last check is at least 60 seconds old. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed the IP geolocation and DNS resolver widgets showing an empty result (and the error texts on top of it) after their check was enabled in the settings, until the refresh button was clicked. They are checked when the dashboard is shown again, also after the widget was shown again. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed the IP geolocation widget failing when a proxy removed the rate limit headers of ip-api.com. The result is shown. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed the DNS resolver widget showing "Object reference not set to an instance of an object" when edns.ip-api.com answered with an empty response (e.g. a proxy). An empty response of ip-api.com is shown as an error in both widgets. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed the offset of the IP geolocation widget being shown as `-/-` for locations in UTC+0 (e.g. London in winter) and in seconds (e.g. `3600`). It is shown as `UTC+01:00`. A region without a code is shown without empty brackets. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)

**Status window**

- The status window now opens after a network change once the network has settled (5 seconds without further changes, e.g. after the DHCP address was assigned, at most 20 seconds after the first change) instead of 5 seconds after the first change, so it no longer shows an intermediate state. An error while updating the DNS configuration after a network change no longer stops the detection of further network changes. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- The status window that opens on a network change no longer takes the keyboard focus from the application you are working in. Clicking into it keeps it open (the countdown stops). [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- Fixed the status window being hidden right after it was opened from the tray icon when the countdown of a previous network change had just ended. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- Fixed the status window being cut off on the left on small screens (e.g. 1280 x 720 with 150 % scaling). It is no longer wider than the screen. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)
- Fixed the status window opened by the user (e.g. from the tray icon) closing itself after a network change. It stays open and checks the network connection again. [#3734](https://github.com/BornToBeRoot/NETworkManager/pull/3734)

**IP Geolocation**

- Fixed the tab getting stuck (query disabled, input read-only, progress bar running) after an error or when the rate limit of ip-api.com was reached, until the tab was closed. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed a query waiting up to 100 seconds when ip-api.com did not respond (e.g. a firewall that drops the request). It fails after 10 seconds. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed "Rate limit reached! Try again in ... seconds" always showing the whole time window of ip-api.com (e.g. 60 seconds) instead of the time that is left. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed every query failing with "The rate limit values couldn't be extracted from the http header..." when a proxy removed the rate limit headers of ip-api.com. The result is shown, the rate limit is still detected by ip-api.com's answer (HTTP 429). [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed an empty response of ip-api.com (e.g. from a proxy) being shown as an empty result instead of an error. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed the offset being shown as `-/-` for locations in UTC+0 (e.g. London in winter) and in seconds (e.g. `3600`). It is shown as `UTC+01:00`. A region without a code is shown without empty brackets. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)

**Traceroute**

- Fixed the traceroute waiting up to 100 seconds per hop when the IP geolocation is enabled and ip-api.com did not respond (e.g. a firewall that drops the request). The geolocation of a hop fails after 10 seconds. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed the IP geolocation of every hop failing with "The rate limit values couldn't be extracted from the http header..." when a proxy removed the rate limit headers of ip-api.com. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)
- Fixed an empty response of ip-api.com (e.g. from a proxy) being shown as an empty IP geolocation of the hop instead of an error. [#3741](https://github.com/BornToBeRoot/NETworkManager/pull/3741)

## Dependencies, Refactoring & Documentation

- Code cleanup & refactoring
- Dragablz (tab control) is now part of the repository (`Source/3rdparty/Dragablz`, stripped to the code we use) instead of a git submodule. Existing clones: see the [README](https://github.com/BornToBeRoot/NETworkManager/tree/main/Source/3rdparty/Dragablz#existing-clones-with-the-old-submodule) of the folder. [#3717](https://github.com/BornToBeRoot/NETworkManager/pull/3717)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
