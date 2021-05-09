---
layout: default
title: Command line arguments
parent: Command line
grand_parent: Documentation
nav_order: 1
description: "Documentation of the command line arguments."
permalink: /Documentation/CommandLine/CommandLineArguments
---

# Command line arguments

## Public
Publicly available command line arguments.

### `--help`
Displays the help dialog.

**Example**
```
NETworkManager.exe --help
```

### `--reset-settings`
Resets all application settings.

**Example**
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
  - `TigerVNC`
  - `WebConsole`
  - `SNMP`
  - `DiscoveryProtocol`
  - `WakeOnLAN`
  - `Whois`
  - `SubnetCalculator`
  - `Lookup`
  - `Connections`
  - `Listeners`
  - `ARPTable`

**Example**
```
NETworkManager.exe --application:PingMonitor
```

## Internal
Internally used command line arguments.

### `--autostart`
Indicates whether the application was started by autostart.

**Example**
```
NETworkManager.exe --autostart
```

### `--restart-pid:`
Passed when the application is restarted to wait for the old process to end.

**Example**
```
NETworkManager.exe --restart-pid:35674
```