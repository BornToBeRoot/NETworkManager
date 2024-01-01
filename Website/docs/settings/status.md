---
sidebar_position: 5
---

# Status

### Show status window on network change

Show the status window when the network changes (e.g. Ethernet cable is plugged in, WLAN or VPN is connected, etc.).

**Type:** `Boolean`

**Default:** `Enabled`

### Time in seconds how long the status window is shown

Time in seconds how long the status window is shown after the network has changed. The status window will be closed automatically after the specified time.

**Type:** `Integer`

**Default:** `10`

:::note

This will only work if [Show status window on network change](#show-status-window-on-network-change) is enabled and the status window is opened due to a network change event.

:::
