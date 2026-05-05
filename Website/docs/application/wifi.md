---
sidebar_position: 2
description: "Analyze available WiFi networks with details on channels, signal strength, and encryption. NETworkManager WiFi Analyzer supports 2.4, 5, and 6 GHz bands."
keywords:
  [
    NETworkManager,
    WiFi analyzer,
    wireless networks,
    WiFi channels,
    signal strength,
    WiFi scanner,
    5GHz,
    6GHz,
  ]
---

# WiFi

The **WiFi** tool shows all available wireless networks with details such as channel, signal strength, and encryption type.

Hidden wireless networks are shown as `Hidden Network`.

## WiFi

On the **WiFi** tab, you can select which wireless network adapter is used to scan for wireless networks. Wireless networks can be filtered by 2.4 GHz, 5 GHz, and 6 GHz.

In the search field, you can filter the wireless networks by `SSID`, `Security`, `Frequency`, `Channel`, `BSSID (MAC Address)`, `Vendor`, and `Phy kind`. The search is case-insensitive.

:::note

Starting with Windows 11 24H2, you need to allow access to the Wi-Fi adapter.

Open `Windows Settings > Privacy & security > Location`, enable access for Desktop Apps / NETworkManager, then restart the application.

:::

:::note

Due to limitations of the `Windows.Devices.WiFi` API, the channel bandwidth cannot be detected.

:::

![WiFi](../img/wifi.png)

### Context menu

| Action         | Description                                                                        |
| -------------- | ---------------------------------------------------------------------------------- |
| **Connect...** | Opens a dialog to connect to the selected wireless network (only if not connected) |
| **Disconnect** | Disconnects from the selected wireless network (only if connected)                 |
| **Export...**  | Opens a dialog to export the selected or all wireless networks to a file           |
| **Copy**       | Copies the selected information to the clipboard                                   |

### Keyboard shortcuts

| Key  | Action            |
| ---- | ----------------- |
| `F5` | Scan for networks |

## Channels

On the **Channels** tab, all wireless networks of the selected wireless network adapter are displayed in a graphical view with channel and signal strength. This can be useful to identify overlapping wireless networks that do not originate from the same access point.

![WiFi - Channel](../img/wifi--channel.png)

:::note

Move the mouse over a channel to display all wireless networks occupying that channel in a tooltip.

:::
