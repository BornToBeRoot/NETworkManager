---
sidebar_position: 5
description: "Configure the NETworkManager status window that appears on network adapter changes and connectivity events."
keywords: [NETworkManager, status settings, status window, network change notification, connectivity status]
---

# Status

The status window checks the network connection with the settings of the dashboard (ping of the public IP address, public IP address API), see [Dashboard > Network connection](../application/dashboard.md#settings-network-connection).

### Show status window on network change

Show the status window when the network changes (e.g. Ethernet cable is plugged in, WLAN or VPN is connected, etc.). The window opens once the network has settled (5 seconds without further changes, e.g. after the DHCP address was assigned, at most 20 seconds after the first change) and checks the network connection, see [Dashboard](../application/dashboard.md#network-connection). It doesn't take the keyboard focus; click into it to keep it open.

**Type:** `Boolean`

**Default:** `Enabled`

### Time in seconds how long the status window is shown

Time in seconds how long the status window is shown after the network has changed. The status window will be closed automatically after the specified time.

**Type:** `Integer`

**Default:** `10`

:::note

This will only work if [Show status window on network change](#show-status-window-on-network-change) is enabled and the status window is opened due to a network change event.

:::
