---
layout: default
title: SNTPLookup
parent: Application
grand_parent: Documentation
nav_order: 16
description: ""
permalink: /Documentation/Application/SNTPLookup
---

# SNTP Lookup

New Feature
{: .label .label-green }

next-release
{: .label .label-purple }

With SNTP Lookup you can query one or more SNTP servers to get the current network time.
SNTP server and get the network current time. It will also show the offset to the local time.

![SNTPLookup](16_SNTPLookup.png)

## Settings

### Servers

List of SNTP server profiles. A profile can contain one or more SNTP servers with host/IP address and port.

**Type:** `List<NETworkManager.Models.Network.ServerInfoProfile>`

**Default:**

| Name              | Server(s)                                                                                                     |
| ----------------- | ------------------------------------------------------------------------------------------------------------- |
| Cloudflare        | `time.cloudflare.com:123`                                                                                     |
| Google Public NTP | `time.google.com:123; time1.google.com:123; time2.google.com:123; time3.google.com:123; time4.google.com:123` |
| Microsoft         | `time.windows.com:123`                                                                                        |
| pool.ntp.org      | `0.pool.ntp.org:123; 1.pool.ntp.org:123; 2.pool.ntp.org:123; 3.pool.ntp.org:123`                              |

{: .note }
At least one SNTP server profile must exist.

### Timeout (ms)

Timeout in milliseconds after the SNTP request is canceled.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `4000`
