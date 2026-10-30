---
sidebar_position: 3
description: "Scan for active devices in your network using ICMP and TCP port checks. NETworkManager IP Scanner supports IP ranges, CIDR subnets, and hostnames."
keywords:
  [
    NETworkManager,
    IP scanner,
    network scanner,
    ping sweep,
    host discovery,
    ICMP scanner,
    subnet scan,
  ]
---

# IP Scanner

With the **IP Scanner** you can scan for active devices based on the hostname or in IP ranges that are reachable via ICMP, answer on a common TCP port (open, or closed with a TCP reset) or reply to a NetBIOS request.

![IP Scanner](../img/ip-scanner.png)

### Example inputs

| Host                             | Description                                                                                  |
| -------------------------------- | -------------------------------------------------------------------------------------------- |
| `10.0.0.1`                       | Single IP address (`10.0.0.1`)                                                               |
| `10.0.0.100 - 10.0.0.199`        | All IP addresses in a given range (`10.0.0.100`, `10.0.0.101`, ..., `10.0.0.199`)            |
| `10.0.0.100-199`                 | All IP addresses in a given range, shorthand for the last octet (`10.0.0.100`, ..., `10.0.0.199`) |
| `10.0.0.0/23`                    | All IP addresses in a subnet (`10.0.0.0`, ..., `10.0.1.255`)                                 |
| `10.0.0.0/255.255.254.0`         | All IP addresses in a subnet (`10.0.0.0`, ..., `10.0.1.255`)                                 |
| `10.0.[0-9,20].[1-2]`            | Multiple IP addresses like (`10.0.0.1`, `10.0.0.2`, `10.0.1.1`, ...,`10.0.9.2`, `10.0.20.1`) |
| `borntoberoot.net`               | Single IP address resolved from a host (`10.0.0.1`)                                          |
| `borntoberoot.net/24`            | All IP addresses in a subnet resolved from a host (`10.0.0.0`, ..., `10.0.0.255`)            |
| `borntoberoot.net/255.255.255.0` | All IP addresses in a subnet resolved from a host (`10.0.0.0`, ..., `10.0.0.255`)            |

:::note

Multiple inputs can be combined with a semicolon (`;`). A column pasted from Excel (one entry per line) is converted to the semicolon-separated form automatically.

Example: `10.0.0.0/24; 10.0.[10-20].1; 10.0.0.100-199`

IP addresses that are part of multiple inputs (e.g. `10.0.0.1; 10.0.0.0/24`) or entered with different hostnames (e.g. two names of one server) are scanned only once. The hostname is resolved via DNS (`PTR`) or NetBIOS, not taken from the input. The network and broadcast address of a subnet are scanned as well.

A scan of more than `65,536` IP addresses (e.g. a `/15`) must be confirmed, more than `16,777,216` IP addresses (a `/8`) cannot be scanned at once.

:::

### Toolbar

| Button                  | Description                                                                                                            |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| **Detect local subnet** | Populates the host field with the IP address and subnet mask of the current network interface to scan the local subnet |

:::note

The local IP address (and subnet mask) is determined by trying to route to a public IP address. If this fails (e.g.
no network connection), NETworkManager iterates over active network adapters and selects the first valid IPv4 address,
with link-local addresses (`169.254.x.x`) given lower priority.

:::

### Context menu

Right-clicking a result allows forwarding device information to another tool (e.g. [Port Scanner](./port-scanner), Remote Desktop), querying the registration data of its IP address or hostname via [RDAP](./rdap), creating a new profile, or executing a [custom command](#custom-commands).

| Action                                                                            | Description                                                                                                   |
| --------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| **Rescan**                                                                        | Scans the selected host(s) again with the settings of the shown scan and updates their rows (a host that is unreachable now is shown as down). A canceled rescan keeps the previous results. Not available while a scan is running |
| **Port Scanner** / **Ping Monitor** / **Traceroute**                              | Opens the host (DNS hostname if resolved, otherwise the IP address) in the tool                              |
| **DNS lookup...** > **IP address** / **Hostname**                                 | Looks up the IP address (`PTR`) or the DNS hostname in [DNS Lookup](./dns-lookup)                             |
| **RDAP...** > **IP address** / **Hostname**                                       | Queries the registration data of the IP address or the registered domain of the DNS hostname in [RDAP](./rdap) |
| **Remote Desktop** / **PowerShell** / **PuTTY** / **TigerVNC** / **SNMP**         | Opens the host (DNS hostname if resolved, otherwise the IP address) in the tool                              |
| **Web Console**                                                                   | Opens the host in the [Web Console](./web-console): https if port `443` is open, otherwise http on `80`, `8443` (https) or `8080`, http if none of them is open (or the port scan is disabled) |
| **Wake on LAN**                                                                   | Sends a magic packet to the MAC address of the host with [Wake on LAN](./wake-on-lan) (only with a MAC address) |
| **Custom commands**                                                               | Executes a [custom command](#custom-commands) for the host                                                    |
| **Add profile...**                                                                | Creates a new profile with the host (IP address), the hostname as name and the MAC address for Wake on LAN    |
| **Copy**                                                                          | Copies the selected information to the clipboard (ping and port values only when the probe is finished)      |
| **Export...**                                                                     | Exports the selected or all results to a file                                                                 |

A NetBIOS name (shown as hostname if there is no DNS hostname) is not passed to other tools, because it may not be resolvable via DNS - the IP address is used instead. **DNS lookup...** / **RDAP...** > **Hostname** are only available with a DNS hostname. In [custom commands](#custom-commands), `{{Hostname}}` is the shown hostname (DNS or NetBIOS).

If multiple results are selected, **Rescan**, **Copy** > **IP address** / **Hostname** / **MAC address** (one value per line, in the order shown, without duplicates) and **Export...** are available.

### Result

The number of reachable (up) and unreachable (down) IP addresses is shown next to the **Result** header. Expand a row to see the ping details, the scanned ports and the NetBIOS information (computer name, domain/workgroup, user and MAC address) of the host.

A host is shown (and counted as up) as soon as it answers the first probe: ping reply, open or refused port or NetBIOS answer. Values of probes that are still running are shown as `...`; the results of the other probes, the hostname (DNS) and the MAC address / vendor are filled in when they arrive. Unreachable hosts are added when all probes are finished. A column you sorted by is not sorted again for values that arrive later, click the header again. The scan is finished when all hosts and all lookups are finished.

The probes a host answered are shown in the tooltip of its status icon (e.g. `Up (detected by Ping, TCP)`) and as **Detected by** in the row details: **Ping** (ICMP echo reply), **TCP** (a [port](#ports) is open or refused the connection) and **NetBIOS** (answer to the [NetBIOS](#scan-for-netbios) request). Probes that answer after the host is shown are added. The export contains them as `DetectedBy` (e.g. `ICMP, TCP`, empty for unreachable hosts).

Only reachable hosts are shown, unless [Show unreachable IP addresses and ports](#show-unreachable-ip-addresses-and-ports) is enabled. A message below the input is shown if no reachable host was found, the scan was canceled, a hostname could not be resolved or an error occurred.

The search box right of the **Result** header filters the results by IP address, hostname, MAC address, vendor and open ports (number or service), also while the scan is running (hosts whose hostname or MAC address arrives later are filtered again). **Export...** > all exports the filtered results; the up / down counters always count all hosts. Results can also be exported while the scan is running; values of probes that are still running (shown as `...`) are exported as empty values.

Expand a row with the button in front of it to see its details, or enable [Expand host view](#expand-host-view) to expand all hosts.

The **Time** column shows the round trip time of the ping reply (`-/-` without a reply); sorted by time, hosts without a reply are always at the end. Sorted by **MAC address**, hosts without a MAC address are always at the end. Hosts with the same time or MAC address (or none) are sorted by IP address.

The columns (e.g. **Hostname**, **Port status**, **MAC address**) and the row details are shown according to the settings the scan was started with. Settings changed afterwards apply to the next scan.

### Keyboard shortcuts

| Key      | Action                                                        |
| -------- | ------------------------------------------------------------- |
| `Enter`  | Start the scan (in the search box: filter the results)        |
| `Esc`    | Cancel the running scan                                       |
| `Ctrl+T` | Open a new tab                                                |

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

If this option is enabled, the [Host](#host) is overwritten by the host from the general settings and the [Host](#host) is disabled.

:::

### Host

Hostname or IP range to scan for active devices.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `10.0.0.0/24; 10.0.[10-20].1`
- `server-01.borntoberoot.net`

:::note

See also the [IP Scanner](./ip-scanner) example inputs for more information about the supported host formats.

:::

## Settings

### Show unreachable IP addresses and ports

Show the scan result for all IP addresses and ports including the ones that are not active.

**Type:** `Boolean`

**Default:** `Disabled`

### Expand host view

Show the details (ping, ports, NetBIOS) of every host in the result, also of hosts that are added while the scan is running. A host that was expanded or collapsed with the button in front of the row keeps its state. Applies immediately.

**Type:** `Boolean`

**Default:** `Disabled`

### Ping hosts

Send a ping (ICMP echo request) to every IP address. Disable it if ICMP is blocked or rate limited in the network (e.g. by an IDS): without ping, an unreachable host is finished after the port scan / NetBIOS timeout instead of after all ping attempts, so the scan is faster. Hosts that only answer the ping are not found then. The **Ping status** and **Time** columns and the ping details are hidden for a scan without ping.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

A host is only found if it answers a probe: at least one of **Ping hosts**, [Scan ports](#scan-ports) or [Scan for NetBIOS](#scan-for-netbios) must be enabled, otherwise the scan doesn't start.

:::

### Attempts

Maximum number of ICMP requests sent to each IP address. Another request is only sent if the previous one has timed out (e.g. a lost packet), other replies like "destination host unreachable" are final.

**Type:** `Integer` [Min `1`, Max `10`]

**Default:** `2`

### Timeout (ms)

Timeout in milliseconds for each ICMP request, after which the packet is considered lost.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `2000`

### Buffer

Size of the buffer for each ICMP request in bytes.

**Type:** `Integer` [Min `1`, Max `65500`]

**Default:** `32`

### Resolve hostname

Resolve the hostname (PTR) for each IP address.

**Type:** `Boolean`

**Default:** `Enabled`

### Scan ports

Scan each IP address for open TCP ports.

**Type:** `Boolean`

**Default:** `Enabled`

### Ports

List of TCP ports to scan for each IP address. The button next to the input restores the default ports.

**Type:** `String`

**Default:** `21; 22; 23; 53; 80; 135; 139; 389; 443; 445; 554; 631; 636; 3389; 5900; 5985; 7000; 8009; 8080; 8443; 9100; 62078`

:::note

Multiple ports and port ranges can be combined with a semicolon (e.g. `22; 80; 443`). Only common and known ports should be scanned to check if a host is reachable. Use the [Port Scanner](./port-scanner) for a detailed port scan.

:::

### Timeout (ms)

Timeout in milliseconds after which a port is considered timed out (no answer). A port is closed if the host refused the connection.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `2000`

:::note

A closed port is reported as soon as the host refuses the connection. Each attempt sends the connection request once (see [Attempts](#attempts-1)).

:::

### Attempts

Number of connection requests sent to each port. Another request is only sent if there was no answer (timed out), an open or closed port is final.

**Type:** `Integer` [Min `1`, Max `10`]

**Default:** `1`

:::tip

In a local network (LAN), `1` is enough. On an unreliable network (e.g. Wi-Fi, VPN) or for hosts on the internet, a lost connection request can make an open port appear as timed out and a host appear as unreachable - increase it to `2` in this case. This only takes longer for ports without answer (attempts × timeout); with the default settings, unreachable hosts are not slower up to `2` attempts, because the ping takes as long (2 × 2000 ms).

:::

### Scan for NetBIOS

Scan each IP address for NetBIOS information.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

Only IPv4 addresses are scanned for NetBIOS, NetBIOS doesn't exist over IPv6.

:::

### Timeout (ms)

Timeout in milliseconds after which a NetBIOS request is considered lost.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `2000`

### Resolve MAC address and vendor

Resolve the MAC address and vendor for each IP address.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

The MAC address is resolved via ARP (IPv4) or NDP (IPv6) from the neighbor cache. If no entry is found there, NetBIOS is used as a fallback. Because ARP and NDP are link-layer protocols, the device must be in the same subnet as the local machine.

:::

### Custom commands

Custom commands that can be executed with a right click on the selected result.

**Type:** `List<NETworkManager.Utilities.CustomCommandInfo>`

**Default:**

| Name                      | File path      | Arguments                                        |
| ------------------------- | -------------- | ------------------------------------------------ |
| Edge                      | `cmd.exe`      | `/c start microsoft-edge:http://{{IPAddress}}/`  |
| Edge (https)              | `cmd.exe`      | `/c start microsoft-edge:https://{{IPAddress}}/` |
| Windows Explorer (c$)     | `explorer.exe` | `\\{{IPAddress}}\c$`                             |
| Windows Explorer (shares) | `explorer.exe` | `\\{{IPAddress}}`                                |
| Computer Management       | `mmc.exe`      | `compmgmt.msc /computer:\\{{IPAddress}}`         |
| Event Viewer              | `eventvwr.exe` | `\\{{IPAddress}}`                                |
| System Information        | `msinfo32.exe` | `/computer {{IPAddress}}`                        |

Computer Management, Event Viewer and System Information need administrative rights on the remote computer.

In the arguments you can use the following placeholders:

| Placeholder      | Description                                          |
| ---------------- | ---------------------------------------------------- |
| `{{IPAddress}}`  | IP address                                           |
| `{{Hostname}}`   | Hostname (DNS hostname, otherwise the NetBIOS name)  |

:::note

The hostname comes from the network (DNS PTR record or NetBIOS name). For security reasons, `{{Hostname}}` is replaced with an empty value if the hostname contains characters that are not allowed in a hostname (e.g. `&` or a space), so it can't be used to run other commands.

:::

:::note

Right-click on a selected custom command to `edit` or `delete` it.

You can also use the Hotkeys `F2` (`edit`) or `Del` (`delete`) on a selected custom command.

:::

### Max. concurrent host threads

Maximum number of hosts (IP addresses) that are scanned at the same time. A thread is not an operating system thread, but one host that is pinged, scanned for ports and NetBIOS concurrently.

**Type:** `Integer` [Min `1`, Max `256`]

**Default:** `128`

:::warning

Every host opens up to [Max. concurrent port threads](#max-concurrent-port-threads) TCP connections at the same time (with the default settings up to 128 × 11 = 1408). Too many simultaneous requests may be detected as a port scan and blocked or rate-limited by a firewall (local, on the network or on the scanned devices) and can overload small routers. If hosts are missing in the result or the network becomes slow during the scan, reduce the number of threads. This will increase the scan time.

:::

### Max. concurrent port threads

Maximum number of ports that are scanned at the same time for each host (IP address). If this is less than the number of [ports](#ports), the remaining ports are scanned after the first ones are finished, which increases the time per host by the [timeout](#timeout-ms-1).

**Type:** `Integer` [Min `1`, Max `32`]

**Default:** `16`

:::warning

Too many simultaneous requests may be detected as a port scan and blocked or rate-limited by a firewall (local, on the network or on the scanned devices). If hosts are missing in the result, reduce the number of threads. This will increase the scan time.

:::
