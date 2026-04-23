---
sidebar_position: 17
description: "Manage Windows Firewall rules with NETworkManager. Add, edit, enable, disable, or delete inbound and outbound rules using a user-friendly interface."
keywords: [NETworkManager, Windows Firewall, firewall rules, inbound, outbound, network security, firewall management]
---

# Firewall

The **Firewall** allows you to view, add, edit, enable, disable, or delete Windows Firewall rules managed by NETworkManager. Rules are identified by a `NETworkManager_` prefix in their display name so that only rules created through NETworkManager are shown in this view.

:::info

Windows Firewall (Windows Defender Firewall) is a built-in host-based firewall included with all versions of Windows. It filters inbound and outbound network traffic based on rules that define the protocol, port, address, program, and action (allow or block).

Rules created by NETworkManager use the prefix `NETworkManager_` in their display name to distinguish them from system-managed or third-party rules. Only rules with this prefix are shown in the Firewall view.

:::

:::note

Adding, editing, enabling, disabling, or deleting firewall rules requires administrator privileges. If the application is not running as administrator, the view is in read-only mode. Use the **Restart as administrator** button to relaunch the application with elevated rights.

:::

<!-- ![Firewall](../img/firewall.png) -->

:::note

In addition, further actions can be performed using the buttons below:

- **Add entry...** - Opens a dialog to create a new firewall rule.
- **Windows Firewall Settings** - Opens the Windows Firewall management console (`WF.msc`).

:::

:::note

With `F5` you can refresh the firewall rules.

Right-click on a rule to `enable`, `disable`, `edit`, or `delete` it, or to `copy` or `export` the information.

You can also use the Hotkeys `F2` (`edit`) or `Del` (`delete`) on a selected rule.

:::