---
layout: default
title: PuTTY
parent: Application
grand_parent: Documentation
nav_order: 11
description: "Documentation of PuTTY"
permalink: /Documentation/Application/PuTTY
---

# PuTTY

With **PuTTY** you can connect to a remote computer via Secure Shell (SSH) or Telnet. In addition you can connect to a local serial port. The integration of PuTTY with NETworkManger supports tabs and profiles for hosts. The connection can be established via a profile (double-click, Enter key or right-click `Connect`) or directly via the [connection](#connect) dialog.

Right-click on the tab will open the context menu with the following options:

- **Reconnect** - Restart PuTTY and reconnect to the remote computer.
- **Resize** - Resize the PuTTY window to the current view size (if connected).
- **Restart session** - Restart the PuTTY session (if connected).

{: .info}
PuTTY is a free and open-source application that acts as a terminal emulator, serial console and network file transfer tool. It offers support for various network protocols such as SSH, Telnet, SCP, rlogin and raw socket connections. In addition to its networking capabilities, PuTTY allows connection to serial ports and provides convenient session management options for saving connection settings and quickly accessing frequently used systems. Its lightweight design and customizable interface make it a popular tool for system administrators and network engineers for tasks such as remote system administration, configuration and troubleshooting.

{: .note}
PuTTY must be installed on the local computer in order to use this feature. You can download the latest version of PuTTY from the [official website](https://www.chiark.greenend.org.uk/~sgtatham/putty/latest.html){:target="\_blank"}.

![PuTTY](11_PuTTY.png)

## Connect

### Mode

Mode used to connect to the remote computer.

**Type:** `NETworkManager.Models.PuTTY.ConnectionMode`

**Default:** `SSH`

**Possible values:**

- `SSH`
- `Telnet`
- `Serial`
- `Rlogin`
- `RAW`

### Host

Hostname or IP address of the remote computer to connect to.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.borntoberoot.net`
- `10.0.0.1`

{: .note }
Only available if [Mode](#mode) is set to `SSH`, `Telnet`, `Rlogin` or `RAW`.

### Port

Port of the remote computer to connect to.

**Type:** `Integer` [Min `1`, Max `65535`]

**Default:**

- [`Settings > SSH port`](#ssh-port) if [Mode](#mode) is set to `SSH`
- [`Settings > Telnet port`](#telnet-port) if [Mode](#mode) is set to `Telnet`
- [`Settings > Rlogin port`](#rlogin-port) if [Mode](#mode) is set to `Rlogin`
- [`Settings > RAW port`](#raw-port) if [Mode](#mode) is set to `RAW`

{: .note }
Only available if [Mode](#mode) is set to `SSH`, `Telnet`, `Rlogin` or `RAW`.

### Serial line

Serial line to connect to.

**Type:** `String`

**Default:** `COM1`

{: .note }
Only available if [Mode](#mode) is set to `Serial`.

### Baud rate

Baud rate to use for the serial connection.

**Type:** `Integer`

**Default:** `9600`

**Possible values:**

- `300`
- `600`
- `1200`
- `2400`
- `4800`
- `9600`
- `14400`
- `19200`
- `28800`
- `38400`
- `57600`
- `115200`
- `128000`
- `256000`
- `512000`
- `921600`

{: .note }
Only available if [Mode](#mode) is set to `Serial`.

### Username

Username to use for the connection.

**Type:** `String`

**Default:** `Empty`

**Example:** `root`

{: .note }
Only available if [Mode](#mode) is set to `SSH`, `Telnet` or `Rlogin`.

### Private key file

Path to the private key file to use for the `SSH` connection.

**Type:** `String`

**Default:** [`Settings > Private key file`](#private-key-file-3)

**Example:** `C:\Users\BornToBeRoot\Documents\id_rsa.ppk`

{: .note }
Only available if [Mode](#mode) is set to `SSH`.

### Profile

PuTTY profile to use for the connection.

**Type:** `String`

**Default:** [`Settings > Profile`](#profile-4)

**Example:** `NETworkManager`

{: .note }
Existing PuTTY profile to use for the connection. If no profile is specified, the default profile is used. Profiles are stored in the Windows registry under `HKEY_CURRENT_USER\Software\SimonTatham\PuTTY\Sessions`.

### Additional command line

Additional command line parameters to use for the connection which are appended.

**Type:** `String`

**Default:** [`Settings > Additional command line`](#additional-command-line-3)

**Example:** `-v`

{: .note }
Additional command line parameters to use for the connection. For more information about the available parameters, see the [PuTTY documentation](https://the.earth.li/~sgtatham/putty/latest/htmldoc/Chapter3.html){:target="\_blank"}.

## Profile

## Group

## Settings
