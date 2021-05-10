---
layout: default
title: General
parent: Settings
grand_parent: Documentation
nav_order: 1
description: ""
permalink: /Documentation/Settings/General
---

# General

## Default application

### Show the following application on startup:
Indicates which application is displayed when the application is started.

**Possible values**
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

**Default**
```
Dashboard
```

## Visible applications in the bar:
Indicates which applications are displayed in the left bar.

### Visible applications / hidden applications
All applications are visible by default.

## Background job
The time how often the background job runs to save for example the settings and profiles. With `0` the background job is deactivated. Changes to this setting will be applied after a restart of the application.

**Possible values**
```
0-120
```

**Default**
```
15
```

## History

### Number of stored entries
Specifies how many entries are stored in the input fields.

**Possible values**
```
0-25
```

**Default**
```
5
```