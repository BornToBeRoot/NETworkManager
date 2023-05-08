---
layout: default
title: TigerVNC
parent: Application
grand_parent: Documentation
nav_order: 13
description: "Documentation of TigerVNC"
permalink: /Documentation/Application/TigerVNC
---

# TigerVNC

With **TigerVNC** you can connect to a remote computer via VNC. The integration of TigerVNC with NETworkManger supports tabs and profiles for hosts. TigerVNC must be installed on the local computer in order to use this feature. On the [official website](https://tigervnc.org/){:target="\_blank"} you can find more information about TigerVNC and download it.

{: .info}
Virtual Network Computing (VNC) is a remote display protocol that allows you to control the desktop environment of a computer remotely. VNC uses the Remote Frame Buffer (RFB) protocol to transmit keyboard and mouse events from the client to the server and screen updates from the server to the client. VNC is platform-independent and supports Windows, Linux and macOS.

{: .info}
TigerVNC is a high-speed version of VNC based on the RealVNC 4 and X.org code bases. TigerVNC started as a next-generation development effort for TightVNC on Unix and Linux platforms, but it split from its parent project in early 2009 so that TightVNC could focus on Windows platforms. TigerVNC supports a variant of Tight encoding that is greatly accelerated by the use of the libjpeg-turbo JPEG codec. See [TigerVNC repository](https://github.com/TigerVNC/tigervnc/){:target="\_blank"} for more details.

![TigerVNC](13_TigerVNC.png)

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
If you enable this option, the [Host](#host) is overwritten by the host from the general settings and the [Host](#host) is disabled.

### Host

Host to connect to via VNC.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.example.com`
- `10.0.0.10`

### Port

TCP port to use for the VNC connection.

**Type:** `Integer` [Min `1`, Max `65535`]

## Settings

### File path

The path to the TigerVNC executable file.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `C:\path\to\vncviewer-x.x.x.exe`
- `C:\path\to\vncviewer64-x.x.x.exe`

{: .note }
The `Configure` button opens the TigerVNC application to configure it.

### Port

Default TCP port to use for the VNC connection.

**Type:** `Integer` [Min `1`, Max `65535`]

**Default:** `5900`
