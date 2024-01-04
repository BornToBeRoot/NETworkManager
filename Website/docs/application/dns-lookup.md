---
sidebar_position: 7
---

# DNS Lookup

With the **DNS Lookup** you can query DNS servers for various resource records.

Example inputs:

- `server-01.borntoberoot.net` (`A` record)
- `10.0.0.1` (`PTR` record)

:::note

Multiple inputs can be combined with a semicolon (`;`).

Example: `server-01.borntoberoot.net; 10.0.0.1`

:::

![DNSLookup](08_DNSLookup.png)

:::note

Right-click on the result to copy or export the information.

:::

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

If this option is enabled, the [Host](#host) is overwritten by the host from the general settings and the [Host](#host) is disabled.

:::

### Host

Hostname or IP address (or any other resource record) to query.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.borntoberoot.net` (`A` record)
- `1.1.1.1` (`PTR` record)

## Settings

### DNS server

List of DNS server profiles. A profile can contain one or more DNS servers with IP address and port.

**Type:** `List<NETworkManager.Models.Network.DNSServerConnectionInfoProfile>`

**Default:**

| Name              | Server 1          | Server 2          |
| ----------------- | ----------------- | ----------------- |
| Cloudflare        | `1.1.1.1:53`      | `1.0.0.1:53`      |
| DNS.Watch         | `84.200.69.80:53` | `84.200.70.40:53` |
| Google Public DNS | `8.8.8.8:53`      | `8.8.4.4:53`      |
| Level3            | `209.244.0.3:53`  | `209.244.0.4:53`  |
| Verisign          | `64.6.64.6:53`    | `64.6.65.6:53`    |

### Add DNS suffix (primary) to hostname

Add the primary DNS suffix to the hostname.

**Type:** `Boolean`

**Default:** `Enabled`

### Use custom DNS suffix

Add a custom DNS suffix to the hostname.

**Type:** `Boolean | String`

**Default:** `Disabled | Empty`

**Example:** `Enabled | borntoberoot.net`

### Recursion

Use recursion for the dns query.

**Type:** `Boolean`

**Default:** `Enabled`

### Use cache

Use the cache for the dns query.

**Type:** `Boolean`

**Default:** `Disabled`

### Query class

DNS class to use for the query.

**Type:** `String`

**Default:** `IN`

**Possible values:**

- `CS`
- `CH`
- `HS`
- `IN`

### Show only most common query types

Only show the most common query types (`A`, `AAAA`, `ANY`, `CNAME`, `MX`, `NS`, `PTR`, `SOA` and `TXT`) in the dropdown menu in the view. Otherwise all available query types are shown.

**Type:** `Boolean`

**Default:** `Enabled`

### Use only TCP

Only use TCP for the dns query. DNS uses UDP by default.

**Type:** `Boolean`

**Default:** `Disabled`

### Retries

Retries for the dns query after which the query is considered lost.

**Type:** `Integer` [Min `1`, Max `10`]

**Default:** `3`

### Timeout (s)

Timeout in seconds for the dns query after which the query is considered lost.

**Type:** `Integer` [Min `1`, Max `15`]

**Default:** `2`
