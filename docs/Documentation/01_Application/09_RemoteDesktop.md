---
layout: default
title: Remote Desktop
parent: Application
grand_parent: Documentation
nav_order: 9
description: "Documentation of Remote Desktop"
permalink: /Documentation/Application/RemoteDesktop
---

# Remote Desktop

With **Remote Desktop** you can connect to a remote computer using the Remote Desktop Protocol (RDP).

{: .info}
Remote Desktop Protocol (RDP) is a proprietary protocol developed by Microsoft that allows users to remotely access and control a computer or virtual machine (VM) over a network connection. It provides a secure communication channel by encrypting the data transmitted between the client and the remote computer, protecting it from unauthorized access.

![RemoteDesktop](09_RemoteDesktop.png)

## Connect

### Host

Host to connect to via VNC.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.borntoberoot.net`
- `10.0.0.10`

### Use credentials

Use credentials to authenticate with the remote computer.

**Type:** `Boolean`

**Default:** `Disabled`

### Username

Username to authenticate with the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:** `Administrator`

{: .note }
Only available if [Use credentials](#use-credentials) is enabled.

### Domain

Domain to authenticate with the remote computer. This is optional.

**Type:** `String`

**Default:** `Empty`

**Example:** `BORNTOBEROOT`

{: .note }
Only available if [Use credentials](#use-credentials) is enabled.

### Password

Password to authenticate with the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:** `P@ssw0rd`

{: .note }
Only available if [Use credentials](#use-credentials) is enabled.

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
If this option is enabled, the [Host](#host-1) is overwritten by the host from the general settings and the [Host](#host-1) is disabled.

### Host

Host to connect to via VNC.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.borntoberoot.net`
- `10.0.0.10`

### Username

Username to authenticate with the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:** `Administrator`

{: .note }
Only available if [Use credentials](#use-credentials-1) is enabled.

### Domain

Domain to authenticate with the remote computer. This is optional.

**Type:** `String`

**Default:** `Empty`

**Example:** `BORNTOBEROOT`

{: .note }
Only available if [Use credentials](#use-credentials-1) is enabled.

### Password

Password to authenticate with the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:** `P@ssw0rd`

{: .note }
Only available if [Use credentials](#use-credentials-1) is enabled.

## Group

## Settings
