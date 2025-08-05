---
sidebar_position: 3
---

# IP Scanner

With the **IP Scanner** you can scan for active devices based on the hostname or in IP ranges that are reachable via icmp or have a common tcp port open.

Example inputs:

| Host                             | Description                                                                               |
| -------------------------------- | ----------------------------------------------------------------------------------------- |
| `10.0.0.1`                       | Single IP address (`10.0.0.1`)                                                            |
| `10.0.0.100 - 10.0.0.199`        | All IP addresses in a given range (`10.0.0.100`, `10.0.0.101`, ..., `10.0.0.199`)         |
| `10.0.0.0/23`                    | All IP addresses in a subnet (`10.0.0.0`, ..., `10.0.1.255`)                              |
| `10.0.0.0/255.255.254.0`         | All IP addresses in a subnet (`10.0.0.0`, ..., `10.0.1.255`)                              |
| `10.0.[0-9,20].[1-2]`            | Multipe IP address like (`10.0.0.1`, `10.0.0.2`, `10.0.1.1`, ...,`10.0.9.2`, `10.0.20.1`) |
| `borntoberoot.net`               | Single IP address resolved from a host (`10.0.0.1`)                                       |
| `borntoberoot.net/24`            | All IP addresses in a subnet resolved from a host (`10.0.0.0`, ..., `10.0.0.255`)         |
| `borntoberoot.net/255.255.255.0` | All IP addresses in a subnet resolved from a host (`10.0.0.0`, ..., `10.0.0.255`)         |

:::note

Multiple inputs can be combined with a semicolon (`;`).

Example: `10.0.0.0/24; 10.0.[10-20]1`

:::

The button to the left of the `Scan` button determines the IP address and subnet mask of the network interface currently in use in order to scan the local subnet for active devices.

If you right-click on a selected result, you can forward the device information to other applications (e.g. Port Scanner, Remote Desktop, etc), create a new profile with this information or execute a [custom command](#custom-commands).

![IP Scanner](../img/ip-scanner.png)

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

### Attempts

Attempts how often an icmp request is retried for each IP address if the request has timed out.

**Type:** `Integer` [Min `1`, Max `10`]

**Default:** `2`

### Timeout (ms)

Timeout in milliseconds for each icmp request and after which the packet is considered lost.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `4000`

### Buffer

Size of the buffer for each icmp request in bytes.

**Type:** `Integer` [Min `1`, Max `65535`]

**Default:** `32`

### Resolve hostname

Resolve the hostname (PTR) for each IP address.

**Type:** `Boolean`

**Default:** `Enabled`

### Scan ports

Scan each IP address for open `tcp` ports.

**Type:** `Boolean`

**Default:** `Enabled`

### Ports

List of `tcp` ports to scan for each IP address.

**Type:** `String`

**Default:** `22; 53; 80; 139; 389; 636; 443; 445; 3389`

:::note

Multiple ports and port ranges can be combined with a semicolon (e.g. `22; 80; 443`). Only common and known ports should be scanned to check if a host is reachable. Use the [Port Scanner](./port-scanner) for a detailed port scan.

:::

### Timeout (ms)

Timeout in milliseconds after which a port is considered closed / timed out.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `4000`

### Scan for NetBIOS

Scan each IP address for NetBIOS information.

**Type:** `Boolean`

**Default:** `Enabled`

### Timeout (ms)

Timeout in milliseconds after which a NetBIOS request is considered lost.

**Type:** `Integer` [Min `100`, Max `15000`]

**Default:** `4000`

### Resolve MAC address and vendor

Resolve the MAC address and vendor for each IP address.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

Due to the fact that the MAC address is resolved via ARP, the device must be in the same subnet as the IP address.

:::

### Custom commands

Custom commands that can be executed with a right click on the selected result.

**Type:** `List<NETworkManager.Utilities.CustomCommandInfo>`

**Default:**

| Name                  | File path      | Arguments                                        |
| --------------------- | -------------- | ------------------------------------------------ |
| Edge                  | `cmd.exe`      | `/c start microsoft-edge:http://$$ipaddress$$/`  |
| Edge (https)          | `cmd.exe`      | `/c start microsoft-edge:https://$$ipaddress$$/` |
| Windows Explorer (c$) | `explorer.exe` | `\\$$ipaddress$$\c$`                             |

In the arguments you can use the following placeholders:

| Placeholder     | Description |
| --------------- | ----------- |
| `$$ipaddress$$` | IP address  |
| `$$hostname$$`  | Hostname    |

:::note

Right-click on a selected custom command to `edit` or `delete` it.

You can also use the Hotkeys `F2` (`edit`) or `Del` (`delete`) on a selected custom command.

:::

### Max. concurrent host threads

Maximal number of threads used to scan for active hosts (1 thread = 1 host/ip address).

**Type:** `Integer` [Min `1`, Max `512`]

**Default:** `256`

:::warning

Too many simultaneous requests may be blocked by a firewall. You can reduce the number of threads to avoid this, but this will increase the scan time.

Too many threads can also cause performance problems on the device.

:::

:::note

This setting only change the maximum number of concurrently executed threads per host scan. See also the [General](../settings/general#threadpool-additional-min-threads) settings to configure the application wide thread pool.

:::

### Max. concurrent port threads

Maximal number of threads that are used to scan for open ports for each host (IP address).

**Type:** `Integer` [Min `1`, Max `10`]

**Default:** `5`

:::warning

Too many simultaneous requests may be blocked by a firewall. You can reduce the number of threads to avoid this, but this will increase the scan time.

Too many threads can also cause performance problems on the device.

:::

:::note

This setting only change the maximum number of concurrently executed threads per port scan. See also the [General](../settings/general#threadpool-additional-min-threads) settings to configure the application wide thread pool.

:::
