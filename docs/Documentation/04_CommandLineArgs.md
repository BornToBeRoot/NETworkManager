---
layout: default
title: Command line arguments
parent: Documentation
nav_order: 4
description: "Documentation of the command line arguments"
permalink: /Documentation/CommandLineArguments
---

# Command line arguments

## Public
Publicly available command line arguments.

### `--help`
Displays the help dialog.

**Example:**
```
NETworkManager.exe --help
```

### `--reset-settings`
Resets all application settings.

**Example:**
```
NETworkManager.exe --reset-settings
```

### `--application:`
Start a specific application on startup.

**Possible values:**
  - `Dashboard`
  - `NetworkInterface`
  - `WiFi`
  - `IPScanner`
  - `PortScanner`
  - `PingMonitor`
  - `Traceroute`
  - `DNSLookup`
  - `RemoteDesktop`
  - `PowerShell`
  - `PuTTY`
  - `AWSSessionManager`
  - `TigerVNC`
  - `WebConsole`
  - `SNMP`
  - `SNTPLookup`
  - `DiscoveryProtocol`
  - `WakeOnLAN`
  - `Whois`
  - `SubnetCalculator`
  - `BitCalculator`
  - `Lookup`
  - `Connections`
  - `Listeners`
  - `ARPTable`

**Example:**
```
NETworkManager.exe --application:PingMonitor
```

## Internal
Internally used command line arguments.

### `--autostart`
Indicates whether the application was started automatically (via autostart).

**Example:**
```
NETworkManager.exe --autostart
```

### `--restart-pid:`
Process ID of the old application process to wait for it to end if the application is restarted.

**Example:**
```
NETworkManager.exe --restart-pid:35674
```
