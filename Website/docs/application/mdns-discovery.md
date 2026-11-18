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
2. Select or enter the **Service type**: `*` for automatic discovery, service types such as `_ipp._tcp` (IPP printers) or `_http._tcp` (web services) and / or [service type profiles](#service-type-profiles) such as `Printers`, separated by `;` (e.g. `Printers; _ssh._tcp`). `Tab` completes the name of a profile. The `.local.` suffix is optional; other domains are not accepted.
3. Select the **Duration (s)** and press **Browse** (or `Enter`). Press **Stop** (or `Esc`) to end discovery early.

A progress bar shows the elapsed duration while browsing (with **Until stopped** it runs until **Stop** is pressed). Errors (e.g. UDP port `5353` can't be used) and "canceled by user" (stopped before the duration expired) are shown below the input.

Discovery uses IPv4 (`224.0.0.251`) and IPv6 (`ff02::fb`) multicast on UDP port `5353`, where available on the selected interface. Records from different interfaces are kept separate. No Bonjour installation or administrator rights are required.

| Field | Type | Default | Example |
| --- | --- | --- | --- |
| Interface | Selection | Last used interface if available; otherwise the first active interface with multicast support | Ethernet |
| Service type | Editable selection (history, service type profiles) | `*` | `Printers`, `_ipp._tcp`, `Printers; Scanners; _ssh._tcp` |
| Duration (s) | Selection: `10`, `30`, `60`, `120`, `300`, **Until stopped** | `10` | `30` |

With `*`, the tool queries `_services._dns-sd._udp.local.` and browses the returned types. It also probes common printer, scanner, web, file sharing and media types because some devices do not answer service type enumeration. The list of the input shows the history (inputs other than a single profile) and the service type profiles with their service types.

Discovery stops when its duration expires or when **Stop** is pressed. With a duration it continues in the background when you switch to another tool. With **Until stopped** it is paused while another tool is shown and continues when you return; the results are kept (records that expired meanwhile are removed). The last results remain available for inspection and export. Starting again clears the previous results. The interface, service type and duration are remembered.

## Results

The header shows the time of the last browse (**Last scan at**), **Raw DNS records** and the search box; below it the number of services and records. The results are updated live while browsing.

The result shows one row per advertised service instance. One printer can advertise several services and therefore appear in several rows.

| Column / detail | Meaning |
| --- | --- |
| Name | Human-readable service instance name |
| Service type | Advertised type, including protocol and `.local.` domain |
| Hostname | SRV target, such as `printer.local.` |
| Port | Advertised service port, such as `631` for IPP |
| TTL | Remaining lifetime in seconds of the service instance's PTR record while browsing |
| IP addresses | Resolved IPv4 and IPv6 addresses; link-local IPv6 addresses include the interface scope |
| SRV priority / weight | Values used for service selection |
| TXT records | Advertised metadata, one TXT string per line; printer records often include model, capabilities and queue path |

Expand a row with the arrow in the first column (like the IP Scanner) to see the details of the service (full name, hostname, IP addresses, port, SRV priority / weight, TTL) and its TXT records, one per line. Empty hostname, port or address fields mean those records have not been resolved (or have expired). Details can arrive in separate responses and update while browsing. Goodbye messages and expired advertisements remove services while discovery is running. After discovery stops, TTL and other values remain at their last observed values.

The search box filters by name, type, hostname, port, addresses and TXT contents. `Enter` in the search box applies the filter without starting discovery. Standard grid sorting and multi-selection are supported.

The context menu provides **Copy...** with the name, service type, hostname, port, IP addresses, TXT records or all details of the selected services (one per line, in the order shown). **Export...** writes the filtered results (in their current sort order) or selected services to CSV, XML or JSON. It includes the interface, full service name, SRV fields, addresses, TXT records and remaining PTR TTL. Control characters and binary TXT values are escaped for display and export.

### Raw DNS records

With **Raw DNS records** checked, the result shows the cached DNS records received on the selected interface instead of the services, including records for services outside the selected type. Records occupy individual lines with **Name**, **Type**, **Class**, **TTL** and **Value** columns. Each character string within a TXT record is shown as a separate row, with the same owner name and TTL. Repeated announcements update the rows instead of adding duplicates.

- **PTR:** the service type or instance the record points to.
- **SRV:** priority, weight, port and target hostname, in that order.
- **TXT:** one entry per row (for example `txtvers=1`, `ty=Printer` or `rp=ipp/print`). Control characters are escaped. Hover over the value to read the complete text, or copy it.
- **A / AAAA:** the host address, with an interface scope for link-local IPv6 addresses.
- **Other types:** recognized DNS types keep their standard name (for example `NSEC`); unknown or private types use `TYPE<number>` (for example `TYPE65400`). Their raw data is preserved as `\# <byte count> <hex data>` (RFC 3597), including zero-length values. They can be searched, copied and exported like the decoded records.

Search filters the records by name, type and value. **Copy...** copies the name, value or the whole record (name, type, class, TTL and value) of the selected records, one per line. **Export...** exports the records while they are shown (filtered or selected, in view order), including the cache-flush flag and interface. The TTL counts down during discovery; after stopping, records remain as a snapshot until the next browse.

## Run command

Open the tool with `mdnsdiscovery`, `mdns` or `dnssd`. Add service types and / or service type profiles (validated like the input) to start browsing:

```text
mdns *
mdns _ipp._tcp
mdns printers
dnssd Printers;_http._tcp.local.
```

The selected interface and duration are used. This tool has no profiles (`@profile`).

## Settings

### Service type profiles

Named lists of service types. A service type profile can be entered by its name in the service type input (case-insensitive, also several and together with service types, e.g. `Printers; Scanners; _ssh._tcp`), so the name must be unique, must not contain `;` and must not be `*` or a service type (e.g. `_ipp._tcp`). The service types of a profile are separated by `;` (`*` and other profiles are not allowed). Several profiles can be selected and deleted at once (context menu or `Del`). Missing default profiles are added again on an update.

**Type:** `List<NETworkManager.Models.Network.MDNSServiceTypeProfileInfo>`

**Default:**

| Name          | Service types                                                                                     |
| ------------- | ------------------------------------------------------------------------------------------------- |
| Printers      | `_ipp._tcp; _ipps._tcp; _printer._tcp; _pdl-datastream._tcp`                                      |
| Scanners      | `_uscan._tcp; _uscans._tcp; _scanner._tcp`                                                        |
| Web           | `_http._tcp; _https._tcp`                                                                         |
| File sharing  | `_smb._tcp; _afpovertcp._tcp; _nfs._tcp; _ftp._tcp; _webdav._tcp; _webdavs._tcp`                  |
| Remote access | `_ssh._tcp; _sftp-ssh._tcp; _rfb._tcp; _rdp._tcp; _telnet._tcp`                                   |
| Media         | `_airplay._tcp; _raop._tcp; _googlecast._tcp; _spotify-connect._tcp; _sonos._tcp`                 |
| Smart home    | `_hap._tcp; _hap._udp; _matter._tcp; _matterc._udp; _home-assistant._tcp; _hue._tcp`              |
| Apple devices | `_companion-link._tcp; _device-info._tcp; _airplay._tcp; _apple-mobdev2._tcp`                     |

## Troubleshooting and limits

- Devices must advertise the services themselves. This tool does not probe arbitrary ports or configure printers.
- Discovery is limited to the selected local link and the `.local.` domain. VLANs, routed networks, VPNs, firewalls and Wi-Fi client isolation can prevent replies. A multicast reflector may make services from another link visible.
- Allow UDP port `5353` through the firewall. Another application that exclusively owns that port can prevent the browser from starting.
- If the interface disconnects, stop discovery, refresh the interface list and browse again.
- The browser limits the cache and the Records view to 4096 records / rows to remain responsive on busy networks.
- Advertised names and metadata are supplied by other devices and do not establish their identity. The tool displays metadata without executing TXT values or opening advertised URLs.

Protocol references: [Multicast DNS (RFC 6762)](https://www.rfc-editor.org/rfc/rfc6762) and [DNS-Based Service Discovery (RFC 6763)](https://www.rfc-editor.org/rfc/rfc6763).
