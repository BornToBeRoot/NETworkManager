---
layout: default
title: IP Scanner
parent: Application
grand_parent: Documentation
nav_order: 4
description: "Documentation of the IP Scanner"
permalink: /Documentation/Application/IPScanner
---

# IP Scanner

With the **IP Scanner** you can scan for active devices in subnets or IP ranges that are reachable via icmp.

Possible inputs are:

| Input                       | Description                                                                               |
| --------------------------- | ----------------------------------------------------------------------------------------- |
| `10.0.0.1`                  | Single IP address (`10.0.0.1`)                                                            |
| `10.0.0.100 - 10.0.0.199`   | All IP addresses in a given range (`10.0.0.100`, `10.0.0.101`, ..., `10.0.0.199`)         |
| `10.0.0.0/23`               | All IP addresses in a subnet (`10.0.0.0`, ..., `10.0.1.255`)                              |
| `10.0.0.0/255.255.254.0`    | All IP addresses in a subnet (`10.0.0.0`, ..., `10.0.1.255`)                              |
| `10.0.[0-9,20].[1-2]`       | Multipe IP address like (`10.0.0.1`, `10.0.0.2`, `10.0.1.1`, ...,`10.0.9.2`, `10.0.20.1`) |
| `example.com`               | Single IP address resolved from a host (`10.0.0.1`)                                       |
| `example.com/24`            | All IP addresses in a subnet resolved from a host (`10.0.0.0`, ..., `10.0.0.255`)         |
| `example.com/255.255.255.0` | All IP addresses in a subnet resolved from a host (`10.0.0.0`, ..., `10.0.0.255`)         |

{: note }#
Multiple inputs can be combined with a semicolon (`;`).<br />Example: `10.0.0.0/24; 10.0.[10-20]1`

The button to the left of the `Scan` button determines the IP address and subnet mask of the network interface currently in use in order to scan the local subnet for active devices.

If you right-click on a selected result, you can forward the device information to other applications (e.g. Port Scanner, Remote Desktop, etc), create a new profile with this information or execute a [custom command](#custom-commands).

![IPScanner](04_IPScanner.png)

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Disabled`

{: note }
If you enable this option, the [IP range](#ip-range) is overwritten by the host from the general settings and the [IP range](#ip-range) is disabled.

### IP range

IP range to scan for active devices.

**Type:** `String`

**Default:** `Empty`

**Example:** `10.0.0.0/24; 10.0.[10-20].1`

## Settings

### Show scan result for all IP addresses

Show the scan result for all IP addresses including the ones that are not active.

**Type:** `Boolean`

**Default:** `Disabled`

### Threads

Number of threads to use for scanning.

**Type:** `Integer` [Min `1`, Max `15000`]

**Default:** `256`

{: note }
Too many simultaneous requests may be blocked by a firewall. You can reduce the number of threads to avoid this, but this will increase the scan time.<br/><br/>Too many threads can also cause performance problems on the device.

### Timeout (ms)

Timeout in milliseconds for each icmp request and after which the packet is considered lost.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `4000`

### Buffer

Size of the buffer for each icmp request in bytes.

**Type:** `Integer` [Min `1`, Max `65535`]

**Default:** `32`

### Attempts

Attempts how often a request is retried for each IP address if the request has timed out.

**Type:** `Integer` [Min `1`, Max `10`]

**Default:** `2`

### Resolve hostname

Resolve the hostname (PTR) for each IP address.

**Type:** `Boolean`

**Default:** `Enabled`

### Show error message

Show a detailed error message if the DNS resolution fails for an IP address.

**Type:** `Boolean`

**Default:** `Disabled`

### Resolve MAC address and vendor

Resolve the MAC address and vendor for each IP address.

**Type:** `Boolean`

**Default:** `Enabled`

{: note }
Due to the fact that the MAC address is resolved via ARP, the device must be in the same subnet as the IP address.

### Custom commands

Custom commands that can be executed with a right click on the selected result.

**Type:** `List<NETworkManager.Utilities.CustomCommandInfo>`

**Default:**

| Name                      | File path      | Arguments                |
| ------------------------- | -------------- | ------------------------ |
| Internet Explorer         | `iexplore.exe` | `http://$$ipaddress$$/`  |
| Internet Explorer (https) | `iexplore.exe` | `https://$$ipaddress$$/` |
| Windows Explorer (c$)     | `explorer.exe` | `\\$$ipaddress$$\c$`     |

In the arguments you can use the following placeholders:

| Placeholder     | Description |
| --------------- | ----------- |
| `$$ipaddress$$` | IP address  |
| `$$hostname$$`  | Hostname    |
