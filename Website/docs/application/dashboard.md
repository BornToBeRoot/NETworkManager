---
sidebar_position: 0
description: "View the status of your network connection at a glance. The NETworkManager Dashboard monitors adapter changes and checks router and internet connectivity."
keywords: [NETworkManager, dashboard, network status, network connection, network monitoring, connectivity check]
---

# Dashboard

The **Dashboard** shows the status of your computer's current network connection to provide a quick overview of the most important information. Whenever the local network adapter changes state (e.g. an Ethernet cable is plugged in, Wi-Fi or VPN connects), the dashboard checks connectivity to the router and the internet. When you switch back to the dashboard, the network connection is only checked again if the last check is at least 60 seconds old (e.g. the public IP address may have changed) or the network or the dashboard settings have changed in the meantime, so switching between the tools doesn't send a request each time. **F5** or the refresh button always checks again.

![Dashboard](../img/dashboard.png)

## Network connection

The **Network connection** widget (also shown in the status window) checks the connection of the computer, the gateway / router and the internet, each for IPv4, IPv6 and DNS. The gateway is pinged (2 attempts with a timeout of 2 seconds, like the IP Scanner). The internet is checked with a ping of the [Public IPv4 address](#public-ipv4-address) / [Public IPv6 address](#public-ipv6-address), the public IP address from [`api.ipify.org`](https://www.ipify.org/) (if [Check public IP address](#check-public-ip-address) is enabled) and the internet connectivity reported by Windows (detects a hotspot / captive portal that requires a sign-in). The value is the public IP address, or the pinged address with the round-trip time if the public IP address is not requested. The icon in front of each value shows the result, hover over it to see the reason. **DNS** is the DNS name (PTR record) of the IPv4 address, or of the IPv6 address if the IPv4 address has none:

| Icon | Meaning |
|------|---------|
| Green | OK |
| Yellow | Works partly or may not work, e.g. the computer has only a link-local IPv4 address (`169.254.x.x`, no address from a DHCP server) or no route to the internet (the address of the first active network adapter is shown), the gateway did not reply to the ping (it may block ICMP), the network adapter has no gateway (e.g. a VPN connection), the public IP address API returned a private IP address (e.g. the page of a hotspot / captive portal or a proxy), Windows reports a hotspot / captive portal, the ping works but the public IP address API doesn't, or the ping and the API fail but Windows reports internet access (e.g. both are blocked by a firewall or only reachable via a proxy) |
| Red | Failed, e.g. the computer has no IPv4 address, the DNS server did not answer, or the internet is not reachable |
| Gray | Information, e.g. no IPv6 connection: the network or the internet provider may not support IPv6 (the IPv6 checks of the router and the internet are skipped), or no DNS name (PTR record) for the IP address, which many networks don't have |

Right-click a value to open it in another tool: an IP address in the **Port Scanner**, **Ping Monitor**, **Traceroute** or **DNS Lookup**, the gateway / router also in the **Web Console** (e.g. its web interface), the public IP address also in **RDAP** and **IP Geolocation**, and a DNS name in the **DNS Lookup**. **Copy** copies the value.

### Keyboard shortcuts

| Key | Action |
|-----|--------|
| `F5` | Refresh the dashboard |

:::note

You may need to click into a widget first before keyboard shortcuts are recognized.

:::

## Run command

Open the tool with `dashboard`. The command accepts no target and no profile, see [Run Command](../run-command.md).

## Settings

### Public IPv4 address

Public IPv4 address that is pinged to check the internet connection. The route to this address is used to detect the local IPv4 address (no packet is sent for this), so it should be routed via the network adapter that is used for the internet.

**Type:** `String`

**Default:** `1.1.1.1`

### Public IPv6 address

Public IPv6 address that is pinged to check the internet connection. The route to this address is used to detect the local IPv6 address (no packet is sent for this), so it should be routed via the network adapter that is used for the internet.

**Type:** `String`

**Default:** `2606:4700:4700::1111`

### Check public IP address

Enables or disables the resolution of the public IP address via [`api.ipify.org`](https://www.ipify.org/) and [`api6.ipify.org`](https://www.ipify.org/). If disabled, the internet is only checked with the ping of the [Public IPv4 address](#public-ipv4-address) / [Public IPv6 address](#public-ipv6-address) and the connectivity reported by Windows.

**Type:** `Boolean`

**Default:** `Enabled`

### Use custom IPv4 address API

Override the default IPv4 address API to resolve the public IP address. The API should return only a plain text IPv4 address like `xx.xx.xx.xx`.

**Type:** `Boolean | String`

**Default:** `Disabled | Empty`

**Example:**

- [`api.ipify.org`](https://api.ipify.org/)
- [`ip4.seeip.org`](https://ip4.seeip.org/)
- [`api.my-ip.io/ip`](https://api.my-ip.io/ip)

### Use custom IPv6 address API

Override the default IPv6 address API to resolve the public IP address. The API should return only a plain text IPv6 address like `xxxx:xx:xxx::xx`.

**Type:** `Boolean | String`

**Default:** `Disabled | Empty`

**Example:**

- [`api6.ipify.org`](https://api6.ipify.org/)

### Check IP geolocation

Enables or disables the resolution of the IP geolocation via [`ip-api.com`](https://ip-api.com/).

:::note

The free API endpoint is limited to 45 requests per minute, supports only the `http` protocol and is available for non-commercial use only.

:::

**Type:** `Boolean`

**Default:** `Enabled`

### Check DNS resolver

Enables or disables the detection of the DNS resolver in use via [`ip-api.com`](https://ip-api.com/).

**Type:** `Boolean`

**Default:** `Enabled`

## Speed Test

The **Speed Test** widget measures the current connection's download and upload speed, latency, and jitter against [`speed.cloudflare.com`](https://speed.cloudflare.com/). The test is **user-initiated** — it does not run automatically on dashboard load. Click **Speed Test** to run a measurement.

The result includes:

- **Download** speed in Mbps
- **Upload** speed in Mbps
- **Latency** in ms (unloaded, 50th percentile of latency probes)
- **Jitter** in ms (average consecutive delta between latency samples)
- **ISP** — internet service provider as reported by Cloudflare
- **Server location** — Cloudflare PoP (Point of Presence) handling the test

Before running the test, a privacy disclaimer informs the user that `speed.cloudflare.com` will receive requests with the client's IP address and network information. The disclaimer is shown again after restarting the app because acceptance is not currently persisted. See the [Cloudflare privacy policy](https://www.cloudflare.com/privacypolicy/) for details. The widget never contacts Cloudflare's telemetry endpoint (`aim.cloudflare.com/__log`).

## Troubleshooting and limits

- The local IP address is detected by the route to the [Public IPv4 address](#public-ipv4-address) / [Public IPv6 address](#public-ipv6-address) (no packet is sent), otherwise from the first active network interface. With several network adapters (e.g. LAN and VPN), set an address that is routed via the adapter you want to see.
- The internet is shown as reachable if the [Public IPv4 address](#public-ipv4-address) / [Public IPv6 address](#public-ipv6-address) replies to the ping or the public IP address could be requested from `api.ipify.org` / `api6.ipify.org` (or the [custom API](#use-custom-ipv4-address-api)). If both are blocked (e.g. by a firewall, or websites are only reachable via a proxy), the internet is shown as a warning if Windows reports internet access (IPv4), otherwise as not reachable; use another public IP address / API. The IP geolocation and the DNS resolver are requested from `ip-api.com` (45 requests per minute).
- Without IPv6 connectivity (or no IPv6 from the internet provider), the IPv6 checks are shown as information (gray) and skipped, the IPv4 checks are not affected.
- The [Speed Test](#speed-test) measures the connection to the nearest Cloudflare server. A VPN, a proxy or other traffic on the network at the same time lowers the result.
