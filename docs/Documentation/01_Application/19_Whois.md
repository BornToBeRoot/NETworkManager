---
layout: default
title: Whois
parent: Application
grand_parent: Documentation
nav_order: 19
description: "Documentation of Whois"
permalink: /Documentation/Application/Whois
---

# Whois

With **Whois** you can retrieve Whois information for a domain directly from the Whois server associated with the top-level domain.

{: .information }
Whois data from a domain is publicly available. The data is provided by the domain registrar and can be queried via the whois protocol. The whois protocol is based on TCP and uses port 43. The whois protocol is not encrypted and the data is transmitted in plain text. Because the whois protocol is not standardized, the data may have a different format depending on the registrar.

{: .note}
In order to use the whois protocol, the firewall must allow outgoing connections on port 43 to the whois server associated with the top-level domain. For example, if you want to query the whois information for `example.com`, you must allow outgoing connections to `whois.verisign-grs.com` on port 43.

{: .note}
For .de domains, DENIC no longer provides information via the whois protocol.

Possible inputs:

- `example.com`

![Whois](19_Whois.png)

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
If you enable this option, the [domain](#domain) is overwritten by the host from the general settings and the [domain](#domain) is disabled.

### Domain

Domain to query for whois information.

**Type:** `String`

**Default:** `Empty`

**Example:** `example.com`
