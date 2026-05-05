---
sidebar_position: 21
description: "Retrieve geolocation data for public IP addresses and domain names using NETworkManager. Shows country, city, ISP, and other network information via ip-api.com."
keywords: [NETworkManager, IP geolocation, IP location, GeoIP, IP lookup, IP address location, geolocation tool]
---

# IP Geolocation

With **IP Geolocation** you can retrieve geolocation information for a fully qualified domain name (FQDN) or a public IP address.

:::info

IP geolocation data is provided by [ip-api.com](https://ip-api.com/) and the API endpoint `http://ip-api.com/json/<host>` is queried when the information is requested.

:::

:::note

The free API endpoint is limited to 45 requests per minute, supports only the `HTTP` protocol and is available for non-commercial use only.

:::

![IPGeolocation](../img/ip-geolocation.png)

### Example inputs

| Input | Description |
|-------|-------------|
| `borntoberoot.net` | Fully qualified domain name (FQDN) |
| `1.1.1.1` | Public IP address |

### Toolbar

| Button | Description |
|--------|-------------|
| **Export...** | Exports the information to a CSV, XML, or JSON file |

### Context menu

| Action | Description |
|--------|-------------|
| **Copy** | Copies the selected information to the clipboard |

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

If this option is enabled, the [host](#host) is overwritten by the host from the general settings and the [host](#host) is disabled.

:::

### Host

Host (FQDN or public IP address) to query for IP geolocation information.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `borntoberoot.net`
- `1.1.1.1`
