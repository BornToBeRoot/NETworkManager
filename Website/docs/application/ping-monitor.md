---
sidebar_position: 5
description: "Monitor the reachability of one or multiple hosts with continuous ICMP echo requests. NETworkManager Ping Monitor tracks uptime and latency in real time."
keywords: [NETworkManager, ping monitor, ICMP ping, host monitoring, network uptime, latency monitor, continuous ping]
---

# Ping Monitor

With the **Ping Monitor** you can monitor one or multiple hosts with ICMP echo requests to determine whether each host is reachable.

:::info

ICMP (Internet Control Message Protocol) is a network-layer protocol used to send diagnostic and control messages. An ICMP echo request (commonly known as a ping) asks a remote host to send back an echo reply, confirming it is reachable on the network.

:::

![Ping Monitor](../img/ping-monitor.png)

### Example inputs

| Host                             | Description                                                                                |
| -------------------------------- | ------------------------------------------------------------------------------------------ |
| `10.0.0.1`                       | Single IP address (`10.0.0.1`)                                                             |
| `10.0.0.100 - 10.0.0.199`        | All IP addresses in a given range (`10.0.0.100`, `10.0.0.101`, ..., `10.0.0.199`)          |
| `10.0.0.0/23`                    | All IP addresses in a subnet (`10.0.0.0`, ..., `10.0.1.255`)                               |
| `10.0.0.0/255.255.254.0`         | All IP addresses in a subnet (`10.0.0.0`, ..., `10.0.1.255`)                               |
| `10.0.[0-9,20].[1-2]`            | Multiple IP addresses like (`10.0.0.1`, `10.0.0.2`, `10.0.1.1`, ...,`10.0.9.2`, `10.0.20.1`) |
| `borntoberoot.net`               | Single IP address resolved from a host (`10.0.0.1`)                                        |
| `borntoberoot.net/24`            | All IP addresses in a subnet resolved from a host (`10.0.0.0`, ..., `10.0.0.255`)          |
| `borntoberoot.net/255.255.255.0` | All IP addresses in a subnet resolved from a host (`10.0.0.0`, ..., `10.0.0.255`)          |

:::note

Multiple inputs can be combined with a semicolon (`;`).

Example: `10.0.0.0/24; 10.0.[10-20]1`

:::

### Context menu

| Action | Description |
|--------|-------------|
| **Copy** | Copies the selected information to the clipboard |
| **Export...** | Exports the selected or all results to a file |

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

If this option is enabled, the [Host](#host) is overwritten by the host from the general settings and the [Host](#host) is disabled.

:::

### Host

Hostname or IP address to ping.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.borntoberoot.net`
- `1.1.1.1`

## Settings

### Timeout (ms)

Timeout in milliseconds for each ICMP packet, after which the packet is considered lost.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `4000`

### Buffer

Buffer size of the ICMP packet.

**Type:** `Integer` [Min `1`, Max `65535`]

**Default:** `32`

### TTL

Time to live of the ICMP packet.

**Type:** `Integer` [Min `1`, Max `255`]

**Default:** `64`

### Don't fragment

Don't fragment the ICMP packet.

**Type:** `Boolean`

**Default:** `Enabled`

### Time (ms) to wait between each ping

Time in milliseconds to wait between each ping.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `1000`

### Expand host view

Expand the host view to show more information when the host is added.

**Type:** `Boolean`

**Default:** `Disabled`
