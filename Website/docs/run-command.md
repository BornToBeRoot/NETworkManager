---
sidebar_position: 5
description: "Open a feature, run a lookup or connect to a host directly from the keyboard with the run command (Ctrl+Shift+P), including profiles and suggestions."
keywords: [NETworkManager, run command, command palette, keyboard shortcut, Ctrl+Shift+P, quick launch, ping, DNS lookup, remote desktop]
---

# Run Command

The **Run Command** opens a feature and runs it directly from the keyboard: press `Ctrl+Shift+P`, type a command like `ping 10.0.0.1` and press `Enter`.

The run command can also be opened with the **Run command...** button in the title bar.

## Syntax

```
command [target] [argument] [type]
command @profile
```

- **Command:** Name or alias of the feature (e.g. `dnslookup` or `dns`), see [Commands](#commands). Without a target, the feature is only opened.
- **Target:** Host(s), IP range, query or URL, depending on the feature. Lists are separated with `;` as everywhere in NETworkManager, spaces around `;` are allowed (e.g. `ping 10.0.0.1; 10.0.0.2`).
- **Argument** and **type:** Additional values of some features (e.g. DNS server and record type of the DNS lookup), in any order after the target. A value with spaces is written in quotes (e.g. `dns borntoberoot.net "Google Public DNS" aaaa`).
- **Profile:** `@` at the beginning of the target, followed by the name of a profile, runs the profile with its settings (e.g. `rdp @server-01`). Only profiles that are enabled for the feature can be used. The name can contain spaces (e.g. `rdp @Web Server 01`). If several profiles have the same name, an error is shown; select the profile in the [suggestions](#suggestions), which show its group.

The input is validated like the input field of the feature (e.g. a valid host or IP range). If it is not valid, an error is shown and the dialog stays open.

## Commands

| Command            | Aliases            | Arguments                           | Example                                  |
| ------------------ | ------------------ | ----------------------------------- | ---------------------------------------- |
| `ipscanner`        | `scan`             | `<range>`                           | `scan 10.0.0.0/24`                       |
| `portscanner`      | `portscan`         | `<hosts> [ports]`                   | `portscan 10.0.0.1 22; 80; 443`          |
| `pingmonitor`      | `ping`             | `<hosts>`                           | `ping 10.0.0.1; 10.0.0.2`                |
| `traceroute`       | `trace`, `tracert` | `<host>`                            | `trace borntoberoot.net`                 |
| `dnslookup`        | `dns`, `nslookup`  | `<hosts> [server] [type]`           | `dns borntoberoot.net 1.1.1.1 aaaa`      |
| `remotedesktop`    | `rdp`, `mstsc`     | `<host[:port]>`                     | `rdp server-01`                          |
| `powershell`       | `ps`, `pwsh`       | `<host>`                            | `ps server-01`                           |
| `putty`            | `ssh`              | `<host>`                            | `ssh server-01`                          |
| `tigervnc`         | `vnc`              | `<host>`                            | `vnc server-01`                          |
| `webconsole`       | `web`              | `<url>`                             | `web https://borntoberoot.net`           |
| `snmp`             |                    | `<host>`                            | `snmp 10.0.0.1`                          |
| `rdap`             |                    | `<query> [type]`                    | `rdap 1.10 asn`                          |
| `whois`            |                    | `<domain>`                          | `whois borntoberoot.net`                 |
| `ipgeolocation`    | `geo`              | `<host>`                            | `geo 1.1.1.1`                            |
| `wakeonlan`        | `wol`              | `<MAC address> [broadcast]`         | `wol 00:11:22:33:44:55 192.168.1.255`    |
| `settings`         |                    |                                     | `settings`                               |

All features that accept a target also accept a profile (`@profile`). The other features (e.g. `subnetcalculator`, `firewall`) can be opened with their name. Features that are hidden in [Settings > General](./settings/general.md) are not available.

:::note

- **Remote Desktop, PowerShell, PuTTY, TigerVNC and Web Console** open the connect dialog with the host, so the connection settings can be adjusted before connecting. A profile connects directly with its settings.
- **DNS lookup:** The type is the DNS record type (`a`, `aaaa`, `any`, `caa`, `cname`, `dnskey`, `mx`, `ns`, `ptr`, `soa`, `srv`, `txt`). Without a type, IP addresses are looked up with `ptr` and other hosts with the type selected in the feature. The DNS server is the name of a configured DNS server (case-insensitive, e.g. `cloudflare`) or a custom DNS server (e.g. `1.1.1.1`).
- **RDAP:** The type is `domain`, `nameserver`, `tld`, `ip`, `asn` or `entity`. Without a type, it is detected from the query (see [RDAP](./application/rdap.md#example-inputs)).
- **Wake on LAN:** The magic packet is sent to the broadcast address (or the one last used in the feature). If there is no valid broadcast address yet, the MAC address is only filled in. A profile sends the magic packet with its settings.

:::

## Suggestions

While typing, suggestions are shown below the input:

| Input                        | Suggestions                                                              |
| ---------------------------- | ------------------------------------------------------------------------ |
| _empty_                      | Recently run commands and all features                                   |
| Command (e.g. `tr`)          | Matching features                                                        |
| After the command (`rdp `)   | The input as typed, recent commands of the feature and its profiles      |
| `@` (e.g. `rdp @web`)        | Profiles of the feature (with group and host); exact names first         |
| After the target             | The input as typed, types of the feature (e.g. DNS record types), configured DNS servers (DNS lookup) and recent commands |

`Enter` runs what was typed: after the command, the input as typed is the first suggestion; profiles, recent commands, types and DNS servers are selected with the arrow keys. While typing the command, the best matching feature is selected (completing it keeps the arguments, e.g. `pin 10.0.0.1` → `ping 10.0.0.1`), but not if the input is empty or an unknown command has arguments. While typing `@`, a profile is only selected if exactly one matches.

| Key          | Action                                         |
| ------------ | ---------------------------------------------- |
| `Up`, `Down` | Select a suggestion                            |
| `Tab`        | Insert the selected suggestion into the input  |
| `Enter`      | Run the selected suggestion                    |
| `Esc`        | Close the dialog                               |

Run commands are saved in a history (the number of entries is set in [Settings > General](./settings/general.md)).
