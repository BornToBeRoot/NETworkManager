---
layout: default
title: Network Interface
parent: Application
grand_parent: Documentation
nav_order: 2
description: "Documentation of the Network Interface"
permalink: /Documentation/Application/NetworkInterface
---

# Network Interface

In **Network Interface** you can see all network adapters of the computer with the most important information (like IP addresses, DNS servers etc.). The bandwidth of the connected network adapter can be monitored and the configuration such as IP address or DNS server can be changed via profiles.

## Information

On the **Information** tab, you can see all the important details of the selected network adapter such as the configured IP addresses, DNS servers, MAC address, and more. If information such as IPv6 configuration is not available, it is hidden in the view.

In addition, further actions can be performed using the buttons at the bottom left:

- **Network connections...** - Opens the `Control Panel > Network and Internet > Network Connections` window
- **Flush DNS cache** - Flush the DNS cache (`ipconfig /flushdns`)
- **Release & Renew**
  - **Release & Renew** - Releases the current IPv4 addresses obtained via DHCP and renews them via DHCP for all network adapters that are configured to automatically obtain an IPv4 address (`ipconfig /release && ipconfig /renew`)
  - **Release** - Releases the current IPv4 addresses obtained via DHCP for all network adapters that are configured to automatically obtain an IPv4 address. (`ipconfig /release`)
  - **Renew** - Renews the current IPv4 address via DHCP for all network adapters that are configured to automatically obtain an IPv4 address. (`ipconfig /renew`)

{: .note }
You may need to confirm a Windows UAC dialog to make changes to the network interface.

![NetworkInterface_Information](02_NetworkInterface_Information.png)

## Bandwidth

On the **Bandwidth** tab, you can monitor the currently used bandwidth of the selected network adapter.

You can see the current download and upload speed in bit/s (B/s). Depending on the bandwidth used, it is automatically changed to KBit/s (KB/s), MBit/s (MB/s) or GBit/s (GB/s). It also shows since when the bandwidth has been measured and how much has been downloaded and uploaded since then.

{: .note }
If you switch to another tool, monitoring will stop and when you switch back, the statistics will be reset and monitoring will continue.

![NetworkInterface_Bandwidth](02_NetworkInterface_Bandwidth.png)

## Configure

On the **Configure** tab, you can change the configuration of the selected network adapter. In order to change the settings, the network adapter must be connected.

The options you can set correspond to the network adapter properties `Internetprotokoll, Version 4 (TCP/IPv4) Properties` in the `Control Panel > Network and Internet > Network Connections`. These are explained in the [profiles section](#profile). Clicking the **Apply** button will launch an elevated PowerShell to configure the network adapter.

{: .note }
You may need to confirm a Windows UAC dialog to make changes to the network interface.

In addition, further actions can be performed using the buttons at the bottom left:

- **Additional config...**
  - **Add IPv4 address...** - Opens a dialog to add an IPv4 address with a subnet mask or CIDR to the selected network adapter.
  - **Remove IPv4 addres...** - Opens a dialog where you can select an IPv4 address to remove from the selected network adapter.

![NetworkInterface_Configure](02_NetworkInterface_Configure.png)

## Profile

### Obtain an IP address automatically

Obtain an IP address automatically from a DHCP server.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
If you select this option, the [Use the following IP address](#use-the-following-ip-address) option will be disabled.


### Use the following IP address:

**Type:** `Boolean`

**Default:** `Disabled`

### IPv4 address

**Type:** `String`

**Default:** `Empty`

**Example:** `192.168.178.20`

### Subnetmask or CIDR

**Type:** `String`

**Default:** `Empty`

**Example:** `/24` or `255.255.255.0`

### Default-Gateway

**Type:** `String`

**Default:** `Empty`

**Example:** `192.168.178.1`

### Obtain DNS server address automatically

### Use the following DNS server addresses:

### Primary DNS server

**Type:** `String`

**Default:** `Empty`

**Example:** `1.1.1.1`

### Secondary DNS server

**Type:** `String`

**Default:** `Empty`

**Example:** `1.0.0.1`
