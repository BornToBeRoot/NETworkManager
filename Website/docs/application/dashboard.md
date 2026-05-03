---
sidebar_position: 0
description: "View the status of your network connection at a glance. The NETworkManager Dashboard monitors adapter changes and checks router and internet connectivity."
keywords: [NETworkManager, dashboard, network status, network connection, network monitoring, connectivity check]
---

# Dashboard

The **Dashboard** shows the status of your computer's current network connection to provide a quick overview of the most important information. Whenever the local network adapter changes state (e.g. an Ethernet cable is plugged in, Wi-Fi or VPN connects), the dashboard checks connectivity to the router and the internet.

![Dashboard](../img/dashboard.png)

### Keyboard shortcuts

| Key | Action |
|-----|--------|
| `F5` | Refresh the dashboard |

:::note

You may need to click into a widget first before keyboard shortcuts are recognized.

:::

## Settings

### Public IPv4 address

Public IPv4 address reachable via ICMP.

**Type:** `String`

**Default:** `1.1.1.1`

### Public IPv6 address

Public IPv6 address reachable via ICMP.

**Type:** `String`

**Default:** `2606:4700:4700::1111`

### Check public IP address

Enables or disables the resolution of the public IP address via [`api.ipify.org`](https://www.ipify.org/) and [`api6.ipify.org`](https://www.ipify.org/).

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
