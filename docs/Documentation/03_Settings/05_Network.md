---
layout: default
title: Network
parent: Settings
grand_parent: Documentation
nav_order: 5
description: "Documentation of the network settings"
permalink: /Documentation/Settings/Network
---

# Network

## DNS

### Use custom DNS server

Enables or disables the custom DNS server(s) for all DNS queries. If disabled, the DNS servers configured in Windows are used. If enabled, the servers configured under [DNS server(s)](#dns-servers) will be used.

**Type:** `Boolean`

**Default:** `Disabled`

### DNS server(s)

A semicolon-separated list of IP addresses of DNS servers to be used for DNS queries when [Use custom DNS server](#use-custom-dns-server) is enabled.

**Type:** `String`

**Default:** `Empty`

**Example:** `1.1.1.1; 1.0.0.1`
