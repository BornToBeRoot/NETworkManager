---
sidebar_position: 4
description: "Configure the global network settings for NETworkManager, including custom DNS servers, DNS suffix behavior, and hostname resolution protocol preference."
keywords: [NETworkManager, network settings, DNS server configuration, custom DNS, DNS suffix, name resolution]
---

# Network

### Use custom DNS server

Enables or disables the custom DNS server(s) for all DNS queries. If disabled, the DNS servers configured in Windows are used. If enabled, the servers configured under [DNS server(s)](#dns-servers) will be used.

**Type:** `Boolean`

**Default:** `Disabled`

### DNS server(s)

The IP addresses (and ports) of the DNS servers to be used for DNS queries when [Use custom DNS server](#use-custom-dns-server) is enabled. Configured via the **Edit DNS server** button.

**Type:** `List of ServerConnectionInfo (IP address + port)`

**Default:** `Empty`

**Example:** `1.1.1.1:53; 1.0.0.1:53`

### Add DNS suffix (primary) to hostname

Enables or disables appending a DNS suffix to a bare hostname (one without a dot) before it is resolved. This applies to all tools that resolve hostnames through NETworkManager's DNS resolver (e.g. Ping, Traceroute, Port Scanner, IP Scanner, NTP Lookup). Fully-qualified hostnames and reverse (PTR) lookups are not affected. If disabled, hostnames are resolved as entered. If enabled, either the primary DNS suffix configured in Windows or the [custom DNS suffix](#use-custom-dns-suffix) is appended. If no primary DNS suffix is configured (e.g. the computer is not domain-joined), the connection-specific DNS suffix of the active network adapter is used instead.

**Type:** `Boolean`

**Default:** `Enabled`

### Use custom DNS suffix

Enables or disables the use of a custom DNS suffix instead of the primary DNS suffix configured in Windows, when [Add DNS suffix (primary) to hostname](#add-dns-suffix-primary-to-hostname) is enabled.

**Type:** `Boolean`

**Default:** `Disabled`

### DNS suffix

The custom DNS suffix to append to a hostname when [Use custom DNS suffix](#use-custom-dns-suffix) is enabled.

**Type:** `String`

**Default:** `Empty`

**Example:** `example.com`

### Preffered protocol when resolving hostnames:

Set the preferred protocol when resolving hostnames.

**Type:** `Boolean`

**Default:** `IPv4`

**Possible values:**

- `IPv4`
- `IPv6`
