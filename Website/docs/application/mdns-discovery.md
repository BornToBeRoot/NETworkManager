---
sidebar_position: 19
description: "Discover and inspect local network services advertised with mDNS and DNS-SD, including printers, scanners, web servers and media devices."
keywords: [NETworkManager, mDNS, DNS-SD, Bonjour, Avahi, printers, service discovery]
---

# mDNS / DNS-SD

The **mDNS / DNS-SD** tool finds services advertised on a local network. Use it to inspect printers, scanners, web servers, SSH servers and media devices without knowing their IP addresses first. It browses DNS-SD service instances and resolves their SRV, TXT, A and AAAA records over multicast DNS.

Unlike the [IP Scanner's mDNS hostname lookup](./ip-scanner.md#resolve-mdns-hostname), this tool discovers advertised services rather than looking up the name of an already discovered IP address.

## Browse

1. Select the **Interface** connected to the devices. The refresh button next to it reloads the available interfaces.
2. Select or enter a **Service type**. Use `*` for automatic discovery, `_ipp._tcp` for IPP printers, or `_http._tcp` for web services. The `.local.` suffix is optional; other domains are not accepted.
3. Select the **Duration (s)** and press **Browse** (or `Enter`). Press **Stop** (or `Esc`) to end discovery early.

A progress bar shows the elapsed duration while browsing (with **Until stopped** it runs until **Stop** is pressed). Errors (e.g. UDP port `5353` can't be used) and "canceled by user" (stopped before the duration expired) are shown below the input.

Discovery uses IPv4 (`224.0.0.251`) and IPv6 (`ff02::fb`) multicast on UDP port `5353`, where available on the selected interface. Records from different interfaces are kept separate. No Bonjour installation or administrator rights are required.

| Field | Type | Default | Example |
| --- | --- | --- | --- |
| Interface | Selection | Last used interface if available; otherwise the first active interface with multicast support | Ethernet |
| Service type | Editable selection | `*` | `_ipp._tcp`, `_ipps._tcp`, `_printer._tcp`, `_pdl-datastream._tcp`, `_http._tcp`, `_ssh._tcp`, `_smb._tcp` |
| Duration (s) | Selection: `10`, `30`, `60`, `120`, `300`, **Until stopped** | `10` | `30` |

With `*`, the tool queries `_services._dns-sd._udp.local.` and browses the returned types. It also probes common printer, scanner, web, file sharing and media types because some devices do not answer service type enumeration. A specific type can be entered even when it is not in the suggestions.

Discovery stops when its duration expires or when **Stop** is pressed. It continues in the background when you switch to another tool. The last results remain available for inspection and export. Starting again clears the previous results. The interface, service type and duration are remembered.

## Results

The header shows the time of the last browse (**Last scan at**) and the search box; below it the number of services and records. The results are updated live while browsing.

The **Services** tab shows one row per advertised service instance. One printer can advertise several services and therefore appear in several rows.

| Column / detail | Meaning |
| --- | --- |
| Name | Human-readable service instance name |
| Service type | Advertised type, including protocol and `.local.` domain |
| Hostname | SRV target, such as `printer.local.` |
| Port | Advertised service port, such as `631` for IPP |
| IP addresses | Resolved IPv4 and IPv6 addresses; link-local IPv6 addresses include the interface scope |
| TTL | Remaining lifetime in seconds of the service instance's PTR record while browsing |
| SRV priority / weight | Values used for service selection |
| TXT records | Advertised metadata, one TXT string per line; printer records often include model, capabilities and queue path |

Select a row to expand its full name, SRV priority/weight and TXT records. Empty hostname, port or address fields mean those records have not been resolved (or have expired). Details can arrive in separate responses and update while browsing. Goodbye messages and expired advertisements remove services while discovery is running. After discovery stops, TTL and other values remain at their last observed values.

The search box filters by name, type, hostname, port, addresses and TXT contents. `Enter` in the search box applies the filter without starting discovery. Standard grid sorting and multi-selection are supported.

The context menu provides **Copy...** with the name, service type, hostname, port, IP addresses, TXT records or all details of the selected services (one per line, in the order shown). **Export...** writes the filtered results (in their current sort order) or selected services to CSV, XML or JSON. It includes the interface, full service name, SRV fields, addresses, TXT records and remaining PTR TTL. Control characters and binary TXT values are escaped for display and export.

### Records

The **Records** tab shows the cached DNS records received on the selected interface, including records for services outside the selected type. Records occupy individual lines with **Name**, **Type**, **Class**, **TTL** and **Value** columns. Each character string within a TXT record is shown as a separate row, with the same owner name and TTL. Repeated announcements update the rows instead of adding duplicates.

- **PTR:** the service type or instance the record points to.
- **SRV:** priority, weight, port and target hostname, in that order.
- **TXT:** one entry per row (for example `txtvers=1`, `ty=Printer` or `rp=ipp/print`). Control characters are escaped. Hover over the value to read the complete text, or copy it.
- **A / AAAA:** the host address, with an interface scope for link-local IPv6 addresses.
- **Other types:** recognized DNS types keep their standard name (for example `NSEC`); unknown or private types use `TYPE<number>` (for example `TYPE65400`). Their raw data is preserved as `\# <byte count> <hex data>` (RFC 3597), including zero-length values. They can be searched, copied and exported like the decoded records.

Search filters the records by name, type and value. **Copy...** copies the name, value or the whole record (name, type, class, TTL and value) of the selected records, one per line. **Export...** exports records when this tab is selected (filtered or selected, in view order), including the cache-flush flag and interface. The TTL counts down during discovery; after stopping, records remain as a snapshot until the next browse.

## Run command

Open the tool with `mdnsdiscovery`, `mdns` or `dnssd`. Add a service type to start browsing:

```text
mdns *
mdns _ipp._tcp
dnssd _http._tcp.local.
```

The selected interface and duration are used. This tool has no profiles.

## Troubleshooting and limits

- Devices must advertise the services themselves. This tool does not probe arbitrary ports or configure printers.
- Discovery is limited to the selected local link and the `.local.` domain. VLANs, routed networks, VPNs, firewalls and Wi-Fi client isolation can prevent replies. A multicast reflector may make services from another link visible.
- Allow UDP port `5353` through the firewall. Another application that exclusively owns that port can prevent the browser from starting.
- If the interface disconnects, stop discovery, refresh the interface list and browse again.
- The browser limits the cache and the Records view to 4096 records / rows to remain responsive on busy networks.
- Advertised names and metadata are supplied by other devices and do not establish their identity. The tool displays metadata without executing TXT values or opening advertised URLs.

Protocol references: [Multicast DNS (RFC 6762)](https://www.rfc-editor.org/rfc/rfc6762) and [DNS-Based Service Discovery (RFC 6763)](https://www.rfc-editor.org/rfc/rfc6763).
