---
sidebar_position: 20
description: "Look up registration data for domains, IP addresses, AS numbers and entities via RDAP (Registration Data Access Protocol) using NETworkManager. Structured, JSON-based successor of Whois."
keywords: [NETworkManager, RDAP, Registration Data Access Protocol, domain lookup, IP lookup, ASN lookup, AS number, registrar, registry, Whois alternative]
---

# RDAP

With **RDAP** (Registration Data Access Protocol) you can retrieve registration data for domains, top-level domains, IP addresses and networks, AS numbers and entities (e.g. organizations or contacts) from the responsible registry.

RDAP is the standardized successor of Whois ([RFC 7480](https://www.rfc-editor.org/rfc/rfc7480), [RFC 9082](https://www.rfc-editor.org/rfc/rfc9082), [RFC 9083](https://www.rfc-editor.org/rfc/rfc9083), [RFC 9224](https://www.rfc-editor.org/rfc/rfc9224)). Unlike Whois, it uses HTTPS and returns structured JSON, so the result is shown in a structured view with details, status, events, nameservers, DNSSEC, contacts and notices. Links of notices (e.g. terms of service, privacy policy or the form to report inaccurate data) can be opened in the browser. The raw JSON response can be shown and exported as well.

:::info

The RDAP server for a query is determined using the bootstrap files published by IANA at [data.iana.org/rdap](https://data.iana.org/rdap/). They are downloaded with the first query, cached locally and refreshed automatically once a week. See [Settings](#settings).

- **Domains** are resolved via the registry of their top-level domain. Some registries (e.g. `.de`, `.ch`, `.li`, `.io`) run an RDAP server that is not (yet) listed by IANA; these are included in NETworkManager.
- **Top-level domains** (e.g. `de`) are queried at the IANA root zone RDAP server (`rdap.iana.org`).
- **IP addresses** and **AS numbers** are resolved via the Regional Internet Registries (RIRs). Private and reserved ranges (e.g. `10.0.0.0/8`, `fe80::/10`) are queried at ARIN, which returns the IANA reservation.
- **Entities** can only be resolved if their handle ends with a service provider tag registered at IANA (e.g. `ORG-RIEN1-RIPE`, `-ARIN`).

:::

:::note

Not every registry offers RDAP. Many country code top-level domains (e.g. `.ru`, `.jp`) still only provide Whois. In this case, the domain can be opened in [Whois](./whois.md) with a single click.

Registration data of personal contacts is usually redacted for privacy reasons (GDPR), the same as with Whois.

:::

:::note

The firewall must allow outgoing HTTPS connections (TCP port 443) to `data.iana.org` and to the RDAP server of the registry. Some registries only offer RDAP via HTTP (TCP port 80).

:::

### Example inputs

| Type       | Query            | Description                                                         |
| ---------- | ---------------- | ------------------------------------------------------------------- |
| Domain     | `borntoberoot.net` | Query a domain at the registry of the top-level domain              |
| TLD        | `de`             | Query a top-level domain at the IANA root zone                      |
| IP address | `1.1.1.1`        | Query the network that contains the IP address                      |
| IP address | `2001:db8::/32`  | Query the network that contains the CIDR prefix                     |
| ASN        | `AS13335`        | Query an AS number (`13335`, `AS13335` and asdot like `1.10` work)  |
| Entity     | `ORG-RIEN1-RIPE` | Query an entity (organization, contact) by its handle               |

### Export

The result can be exported via the **Export...** button below the result or via the context menu (right-click).

| Format   | Description                                                                                                       |
| -------- | ----------------------------------------------------------------------------------------------------------------- |
| **JSON** | Exports the response exactly as returned by the server. If the referral was followed, both responses as an array. |
| **TXT**  | Exports the structured view as text.                                                                              |

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

If this option is enabled, the [query](#query) is overwritten by the host from the general settings and the [query](#query) is disabled.

:::

### Query

Domain, top-level domain, IP address, AS number or entity handle to query.

**Type:** `String`

**Default:** `Empty`

**Example:** `borntoberoot.net`

### Type

Type of the query.

**Type:** `Domain`, `TLD`, `IP address`, `ASN`, `Entity`

**Default:** `Domain`

## Settings

### Bootstrap files

Status of the IANA bootstrap files (`dns.json`, `ipv4.json`, `ipv6.json`, `asn.json`, `object-tags.json`) that are used to find the RDAP server for a query. **Update now** checks for new versions immediately. **Open location** opens the folder where the files are cached.

:::note

The files are stored in `%LocalAppData%\NETworkManager\RDAP_Cache`.

:::

### Follow referral to registrar

For many domains (e.g. `.com` and `.net`), the registry only returns basic data and refers to the RDAP server of the registrar for details (e.g. the contacts). If enabled, the RDAP server of the registrar is queried as well and both results are shown. Only applies to domain queries.

**Type:** `Boolean`

**Default:** `Enabled`

### Timeout (ms)

Timeout in milliseconds for each request to an RDAP server, after which the request is considered lost.

**Type:** `Integer` [Min `1000`, Max `60000`]

**Default:** `10000`
