---
layout: default
title: Ping Monitor
parent: Application
grand_parent: Documentation
nav_order: 6
description: "Documentation of the Ping Monitor"
permalink: /Documentation/Application/PingMonitor
---

# Ping Monitor

With the **Ping Monitor** you can monitor one or multiple hosts with ICMP echo requests to determine if the specific host is reachable.

Example inputs:

- `server-01.borntoberoot.net`
- `10.0.0.1`

{: .note }
Multiple inputs can be combined with a semicolon (`;`).<br />Example: `server-01.borntoberoot.net; 10.0.0.1`

![PingMonitor](06_PingMonitor.png)

{: .note}
Right-click on the result to copy or export the information.

<hr>

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
If this option is enabled, the [Host](#host) is overwritten by the host from the general settings and the [Host](#host) is disabled.

### Host

Hostname or IP address to ping.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.borntoberoot.net`
- `1.1.1.1`

<hr>

## Settings

### Timeout (ms)

Timeout in milliseconds for the icmp packet after which the packet is considered lost.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `4000`

### Buffer

Buffer size of the icmp packet.

**Type:** `Integer` [Min `1`, Max `65535`]

**Default:** `32`

### TTL

Time to live of the icmp packet.

**Type:** `Integer` [Min `1`, Max `255`]

**Default:** `64`

### Don't fragment

Don't fragment the icmp packet.

**Type:** `Boolean`

**Default:** `true`

### Time (ms) to wait between each ping

Time in milliseconds to wait between each ping.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `1000`
