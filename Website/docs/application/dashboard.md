---
sidebar_position: 0
description: "View the status of your network connection at a glance. The NETworkManager Dashboard monitors adapter changes and checks router and internet connectivity."
keywords: [NETworkManager, dashboard, network status, network connection, network monitoring, connectivity check]
---

# Dashboard

The **Dashboard** shows the status of your computer's current network connection to provide a quick overview of the most important information. Whenever the local network adapter changes state (e.g. an Ethernet cable is plugged in, Wi-Fi or VPN connects), the dashboard checks connectivity to the router and the internet. The [IP geolocation](#ip-geolocation) and the [DNS resolver](#dns-resolver) are checked again together with the network connection (the public IP address may have changed, e.g. a VPN connected). When you switch back to the dashboard (or show the window again from the tray / taskbar), the widgets are only checked again if the last check is at least 60 seconds old (e.g. the public IP address may have changed) or the network or a setting of a check (e.g. the public IP address) has changed in the meantime, so switching between the tools doesn't send a request each time. **F5** or the refresh button of a widget always checks it again. The [Internet latency](#internet-latency) widget is updated live while the dashboard is shown.

![Dashboard](../img/dashboard.png)

## Network connection

The **Network connection** widget (also shown in the status window) checks the connection of the computer, the gateway / router and the internet, each for IPv4, IPv6 and DNS. The gateway is pinged (2 attempts with a timeout of 2 seconds, like the IP Scanner). The internet is checked with a ping of the [Public IPv4 address](#public-ipv4-address) / [Public IPv6 address](#public-ipv6-address), the public IP address from [`api.ipify.org`](https://www.ipify.org/) (if [Check public IP address](#check-public-ip-address) is enabled) and the internet connectivity reported by Windows (detects a hotspot / captive portal that requires a sign-in). The value is the public IP address, or the pinged address with the round-trip time if the public IP address is not requested. The icon in front of each value shows the result, hover over it to see the reason. **DNS** is the DNS name (PTR record) of the IPv4 address, or of the IPv6 address if the IPv4 address has none:

| Icon | Meaning |
|------|---------|
| Green | OK |
| Yellow | Works partly or may not work, e.g. the computer has only a link-local IPv4 address (`169.254.x.x`, no address from a DHCP server) or no route to the internet (the address of the first active network adapter is shown), the gateway did not reply to the ping (it may block ICMP), the network adapter has no gateway (e.g. a VPN connection), the public IP address API returned a private IP address (e.g. the page of a hotspot / captive portal or a proxy), Windows reports a hotspot / captive portal (IPv4, unless the public IP address API returned a public IP address), the ping works but the public IP address API doesn't, or the ping and the API fail but Windows reports internet access (e.g. both are blocked by a firewall or only reachable via a proxy) |
| Red | Failed, e.g. the computer has no IPv4 address, the DNS server did not answer, or the internet is not reachable |
| Gray | Information, e.g. no IPv6 connection: the network or the internet provider may not support IPv6 (the IPv6 checks of the router and the internet are skipped), or no DNS name (PTR record) for the IP address, which many networks don't have |

Right-click a value to open it in another tool: an IP address in the **Port Scanner**, **Ping Monitor**, **Traceroute** or **DNS Lookup**, the gateway / router also in the **Web Console** (e.g. its web interface, not for a link-local IPv6 address like `fe80::1%12`), the public IP address also in **RDAP** and **IP Geolocation**, and a DNS name in the **DNS Lookup**. **Copy** copies the value. The **Copy all** button next to the refresh button copies all values with the reasons of the states as text (e.g. for a support request).

### Keyboard shortcuts

| Key | Action |
|-----|--------|
| `F5` | Check the network connection again |

:::note

`F5` only works while the widget has the focus (e.g. click into it first). It checks the **Network connection**, **IP geolocation** or **DNS resolver** widget again; the speed test is only started with its button.

:::

## Run command

Open the tool with `dashboard`. The command accepts no target and no profile, see [Run Command](../run-command.md).

## Settings

The settings are grouped by widget, in the order of the dashboard. **Show widget** shows or hides a widget; the IP geolocation and DNS resolver widgets also have a switch for their check (they are shown by default, but only send a request to `ip-api.com` after the check was enabled).

### Network connection {#settings-network-connection}

The network connection widget and the [status window](../settings/status.md) use these settings to check the internet. They also apply while the widget is hidden.

#### Show widget {#show-network-connection-widget}

Show or hide the **Network connection** widget on the dashboard. A hidden widget doesn't check anything; the status window always shows the network connection.

**Type:** `Boolean`

**Default:** `Enabled`

#### Public IPv4 address

Public IPv4 address (not the one of your computer) that is pinged to check the internet connection. The route to this address is used to detect the local IPv4 address (no packet is sent for this), so it should be routed via the network adapter that is used for the internet.

**Type:** `String`

**Default:** `1.1.1.1`

#### Public IPv6 address

Public IPv6 address (not the one of your computer) that is pinged to check the internet connection. The route to this address is used to detect the local IPv6 address (no packet is sent for this), so it should be routed via the network adapter that is used for the internet.

**Type:** `String`

**Default:** `2606:4700:4700::1111`

#### Check public IP address

Enables or disables the resolution of the public IP address via [`api.ipify.org`](https://www.ipify.org/) and [`api6.ipify.org`](https://www.ipify.org/). If disabled, the internet is only checked with the ping of the [Public IPv4 address](#public-ipv4-address) / [Public IPv6 address](#public-ipv6-address) and the connectivity reported by Windows.

**Type:** `Boolean`

**Default:** `Enabled`

#### Use custom IPv4 address API

Override the default IPv4 address API to resolve the public IP address. The API should return only a plain text IPv4 address like `xx.xx.xx.xx`, or a JSON object with the address as a string value like `{"ip":"xx.xx.xx.xx"}`. Other answers (e.g. an HTML page) are not accepted.

**Type:** `Boolean | String`

**Default:** `Disabled | Empty`

**Example:**

| API | Answer |
|-----|--------|
| [`https://api.ipify.org`](https://api.ipify.org/) | Plain text |
| [`https://api.ipify.org?format=json`](https://api.ipify.org?format=json) | JSON `{"ip":"xx.xx.xx.xx"}` |
| [`https://checkip.amazonaws.com`](https://checkip.amazonaws.com/) | Plain text |
| [`https://ifconfig.co/ip`](https://ifconfig.co/ip) | Plain text |
| [`https://ipinfo.io/ip`](https://ipinfo.io/ip) | Plain text |
| [`http://ip-api.com/line/?fields=query`](http://ip-api.com/line/?fields=query) | Plain text (`http` only, 45 requests per minute, non-commercial use only) |

Many APIs also return JSON with more information (e.g. `https://ipinfo.io/json`, `https://ifconfig.co/json`, `http://ip-api.com/json`), the first string value that is an IP address is used. An API that returns `key=value` lines (e.g. `https://1.1.1.1/cdn-cgi/trace`) is not supported.

#### Use custom IPv6 address API

Override the default IPv6 address API to resolve the public IP address. The API should return only a plain text IPv6 address like `xxxx:xx:xxx::xx`, or a JSON object with the address as a string value like `{"ip":"xxxx:xx:xxx::xx"}`. Other answers (e.g. an HTML page) are not accepted.

**Type:** `Boolean | String`

**Default:** `Disabled | Empty`

**Example:**

| API | Answer |
|-----|--------|
| [`https://api6.ipify.org`](https://api6.ipify.org/) | Plain text |
| [`https://api64.ipify.org?format=json`](https://api64.ipify.org?format=json) | JSON `{"ip":"xxxx:xx:xxx::xx"}` (IPv6 if available, otherwise IPv4 - then the answer is not accepted) |

### Internet latency {#settings-internet-latency}

#### Show widget {#show-internet-latency-widget}

Show or hide the **Internet latency** widget on the dashboard. A hidden widget doesn't ping anything. A shown widget whose check is disabled shows what it does and an **Enable** button.

**Type:** `Boolean`

**Default:** `Enabled`

#### Position

Where the widget is shown on the dashboard:

- **Next to the speed test**: to the right of the speed test (1/3 of the width), the targets one below the other. If the dashboard is narrower than about 900 pixels (or the speed test is hidden), the widget gets its own row below the speed test.
- **Own row**: below the network connection, the targets side by side.

The targets are shown one below the other whenever the widget is narrower than about 500 pixels.

**Type:** `NETworkManager.Models.Network.InternetLatencyPosition`

**Default:** `NextToSpeedTest`

**Possible values:**

- `NextToSpeedTest`
- `OwnRow`

#### Check internet latency

Enables or disables the pings of the targets. The targets are only pinged while the dashboard is shown (not in the background, not while the window is minimized or in the tray). The check can also be enabled with the **Enable** button of the widget. The switch is disabled while the widget is hidden (the value is kept).

**Type:** `Boolean`

**Default:** `Disabled`

#### Interval (seconds)

Time between the reply (or the timeout of 2 seconds) of a ping and the next ping of a target.

**Type:** `Integer` [Min `1`, Max `10`]

**Default:** `2`

#### Targets

Name and host (IP address or host name) of up to three targets. The name is shown in the widget (the host if it is empty). A target without a host is not shown, the other targets use the space.

**Type:** `String` (name and host of each target)

**Default:**

| Target | Name | Host |
|--------|------|------|
| 1 | `Cloudflare` | `1.1.1.1` |
| 2 | `Google` | `8.8.8.8` |
| 3 | `Quad9` | `9.9.9.9` |

Use IP addresses if possible: the widget then still works when the DNS server doesn't (that's when you want to see whether the internet works).

### Speed Test {#settings-speed-test}

#### Show widget {#show-speed-test-widget}

Show or hide the **Speed Test** widget on the dashboard. A speed test that is running when the widget is hidden is canceled.

**Type:** `Boolean`

**Default:** `Enabled`

### IP geolocation {#settings-ip-geolocation}

#### Show widget {#show-ip-geolocation-widget}

Show or hide the **IP geolocation** widget on the dashboard. A hidden widget sends no request to `ip-api.com`, the [DNS resolver](#dns-resolver) widget then uses the whole width. In a narrow window (less than about 1200 pixels for the dashboard, the DNS resolver needs about 400 pixels for its header) both widgets are shown one below the other. A shown widget whose check is disabled shows what it requests and an **Enable** button.

**Type:** `Boolean`

**Default:** `Enabled`

#### Check IP geolocation

Enables or disables the resolution of the IP geolocation via [`ip-api.com`](https://ip-api.com/). The check can also be enabled in the welcome dialog or with the **Enable** button of the widget. The switch is disabled while the widget is hidden (the value is kept).

:::note

The free API endpoint is limited to 45 requests per minute, supports only the `http` protocol and is available for non-commercial use only.

:::

**Type:** `Boolean`

**Default:** `Disabled`

### DNS resolver {#settings-dns-resolver}

#### Show widget {#show-dns-resolver-widget}

Show or hide the **DNS resolver** widget on the dashboard. A hidden widget sends no request to `ip-api.com`, the [IP geolocation](#ip-geolocation) widget then uses the whole width. In a narrow window (less than about 1200 pixels for the dashboard, the DNS resolver needs about 400 pixels for its header) both widgets are shown one below the other. A shown widget whose check is disabled shows what it requests and an **Enable** button.

**Type:** `Boolean`

**Default:** `Enabled`

#### Check DNS resolver

Enables or disables the detection of the DNS resolver in use via [`ip-api.com`](https://ip-api.com/). The check can also be enabled in the welcome dialog or with the **Enable** button of the widget. The switch is disabled while the widget is hidden (the value is kept).

**Type:** `Boolean`

**Default:** `Disabled`

## Internet latency

The **Internet latency** widget pings up to three targets on the internet and shows one line per target with its name, the round-trip time of the last ping and a chart of the last 60 seconds, like the dashboard of a router or gateway. By default it is shown to the right of the [Speed Test](#speed-test) (1/3 of the width, the targets one below the other). In a narrow window (less than about 900 pixels for the dashboard) or without the speed test it gets its own row below the speed test, with the targets side by side; see [Position](#position). By default these are the public DNS resolvers of Cloudflare (`1.1.1.1`), Google (`8.8.8.8`) and Quad9 (`9.9.9.9`): they answer pings reliably and are close to every user (anycast). The targets can be changed in the [settings](#settings-internet-latency).

The check is disabled by default: the widget shows what it does and an **Enable** button, which enables the check (like [Check internet latency](#check-internet-latency)) and starts right away.

The targets are only pinged while the dashboard is shown: not when you switch to another tool or the settings, and not while the window is minimized or in the tray. When the dashboard is shown again, the pings continue: the pings of the last minute stay, the pause is a gap in the chart and doesn't count as lost pings (nothing is pinged in the background). After a network change (e.g. a VPN connected, the targets may be reached via another route) and when the check is disabled, the values and the chart start again. Each target is pinged on its own (timeout 2 seconds), the next ping follows after the [interval](#interval-seconds) (default 2 seconds), so a slow target doesn't delay the others. A host name is resolved once with the DNS settings of the application (IPv4 is preferred, see the network settings) and again after a network change.

| Dot | Meaning |
|-----|---------|
| Green | The last ping was answered |
| Yellow | The last ping was answered, but a ping was lost in the last minute |
| Red | The last ping was not answered (the status is shown, e.g. timed out) or the host name could not be resolved |
| Gray | Not pinged yet |

A lost ping is a gap in the chart. Hover over a target to see the host, the IP address and the minimum / average / maximum round-trip time and the packet loss of the last minute. Right-click a target to open it in the **Ping Monitor**, **Traceroute** or **DNS Lookup**, **Copy** copies the host. The **Copy all** button copies the values and statistics of all targets as text.

## Speed Test

The **Speed Test** widget measures the download and upload speed, latency and jitter of the current connection against [`speed.cloudflare.com`](https://speed.cloudflare.com/). The test only runs when you start it, not when the dashboard is opened.

Before the first test, a privacy disclaimer informs you that `speed.cloudflare.com` receives requests with your IP address and network information. Click **Speed Test** to accept it and run the first test. The disclaimer is shown again after restarting the application (the acceptance is not saved). See the [Cloudflare privacy policy](https://www.cloudflare.com/privacypolicy/) for details. The widget never contacts Cloudflare's telemetry endpoint (`aim.cloudflare.com`).

The buttons in the header of the widget:

- **Run speed test** (refresh icon) starts a new test, while a test runs the button stops it (**Stop**). The values measured until then stay visible.
- **Copy all** copies all values and the time of the test as text, e.g. for a support request.

While the test runs, the header shows the current phase (latency, download, upload), afterwards the time of the test (**Last test at ...**, the tooltip shows the date; not after a canceled or failed test). A canceled or failed test and requests that failed are shown below the header.

The result includes:

- **Download** speed in Mbps, with a chart of the requests (hover over it to see the speed of a request)
- **Upload** speed in Mbps, with a chart of the requests
- **Latency** in ms (unloaded, 50th percentile of the latency probes)
- **Jitter** in ms (average difference between consecutive latency probes)
- **ISP** — internet service provider as reported by Cloudflare
- **Server** — Cloudflare server (city, country and airport code) that handles the test

The test measures like the official [cloudflare/speedtest](https://github.com/cloudflare/speedtest) library (used by [speed.cloudflare.com](https://speed.cloudflare.com/)), so the results are comparable: latency probes (empty downloads) and downloads / uploads of increasing size (100 kB up to 250 MB) one after the other, with a few latency probes between them. Download and upload are the 90th percentile of the requests that took at least 10 ms; the speed shown while a download runs is the result of the completed requests (only the first request is shown while it runs). A direction ends after a set of requests in which even the fastest request took longer than one second; the larger sizes are skipped.

A test takes about 15 seconds on a fast connection and up to 1–2 minutes on a slow one. It transfers a lot of data on a fast connection: up to about 1 GB download and 300 MB upload (1 Gbit/s), keep this in mind on a metered connection (e.g. mobile data).

## IP geolocation

The **IP geolocation** widget shows the location and the network of your public IPv4 address from [`ip-api.com`](https://ip-api.com/) (the same values as the [IP Geolocation](ip-geolocation.md) tool):

- **Location**: continent, country, region, city, district, zip code, latitude / longitude, time zone, offset from UTC (e.g. `UTC+01:00`, incl. daylight saving time) and currency
- **Network**: IP address, PTR record, ISP, organization, ASN / AS name, and whether the IP address belongs to a hosting provider, a proxy / VPN or a mobile network

## DNS resolver

The **DNS resolver** widget shows the DNS server that resolves the requests of your computer on the internet, detected with [`edns.ip-api.com`](https://ip-api.com/docs/dns): its IP address and geolocation (**DNS**), and the client subnet that the DNS server forwards (**EDNS**, e.g. `203.0.113.0`, `-/-` if it doesn't send one). If you use the DNS server of your router or internet provider, this is the server the requests are forwarded to (e.g. `1.1.1.1` or a server of the provider), not the router.

## IP geolocation and DNS resolver

Both checks send your IP address to `ip-api.com` and are disabled by default (welcome dialog). A widget whose check is disabled shows what it requests and an **Enable** button, which enables the check (like [Check IP geolocation](#check-ip-geolocation) / [Check DNS resolver](#check-dns-resolver)) and checks right away.

The buttons in the header of the widgets:

- **Copy all** copies all values as text, e.g. for a support request.
- **Refresh** checks again (also **F5** while the widget has the focus).

The header shows the time of the last check (not after an error). An error (e.g. no response within 10 seconds or the HTTP status code) or the rate limit of `ip-api.com` (45 requests per minute from your network, with the time until it can be requested again) is shown below the header.

Right-click an IP address (public IP address, DNS server) to open it in the **Port Scanner**, **Ping Monitor**, **Traceroute**, **DNS Lookup**, **RDAP** or **IP Geolocation**, the EDNS client subnet (a network address, not a host) in **RDAP** or **IP Geolocation**, and the PTR record in the **DNS Lookup**. **Copy** copies the selected text.

## Troubleshooting and limits

- The local IP address is detected by the route to the [Public IPv4 address](#public-ipv4-address) / [Public IPv6 address](#public-ipv6-address) (no packet is sent), otherwise from the first active network interface. With several network adapters (e.g. LAN and VPN), set an address that is routed via the adapter you want to see.
- The internet is shown as reachable if the [Public IPv4 address](#public-ipv4-address) / [Public IPv6 address](#public-ipv6-address) replies to the ping or the public IP address could be requested from `api.ipify.org` / `api6.ipify.org` (or the [custom API](#use-custom-ipv4-address-api)). If both are blocked (e.g. by a firewall, or websites are only reachable via a proxy), the internet is shown as a warning if Windows reports internet access (IPv4), otherwise as not reachable; use another public IP address / API. The IP geolocation and the DNS resolver are requested from `ip-api.com` (45 requests per minute, a request fails after 10 seconds without a response).
- Without IPv6 connectivity (or no IPv6 from the internet provider), the IPv6 checks are shown as information (gray) and skipped, the IPv4 checks are not affected.
- The [Internet latency](#internet-latency) widget uses ICMP (ping). A firewall, a VPN or a network that blocks ICMP shows the targets as timed out (red) although websites work; the network connection shows the internet as reachable via the public IP address API in that case.
- The [Speed Test](#speed-test) measures the connection to the nearest Cloudflare server. A VPN, a proxy or other traffic on the network at the same time lowers the result. A request of the speed test fails after 15 seconds without a response or data (e.g. the connection was lost). Like the official library, a failed request is skipped (shown as a warning after the test) and a request that is rate limited by Cloudflare (HTTP 429) is repeated up to three times (then the test ends with the error); if all requests of a size fail (e.g. a proxy rejects large uploads), the larger sizes of the direction are skipped. After three requests in a row without a response the test ends with the error, unless the server still answers a small request: if three requests of the same size failed (e.g. a proxy closes the connection of large uploads), the remaining requests of this size and the larger sizes of the direction are skipped, otherwise (e.g. the latency probes before failed during a short Wi-Fi drop) only the failed request is skipped. If no request succeeds at all (e.g. a proxy rejects all requests to `speed.cloudflare.com` with HTTP 403), the test ends with the error.
