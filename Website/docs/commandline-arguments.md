---
sidebar_position: 5
---

# Command Line Arguments

## Public

Publicly available command line arguments.

### `--help`

Displays the help dialog.

**Example:**

```powershell
NETworkManager.exe --help
```

### `--reset-settings`

Resets all application settings.

**Example:**

```powershell
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
- `SNTPLookup`
- `DiscoveryProtocol`
- `WakeOnLAN`
- `Whois`
- `IPGeolocation`
- `SubnetCalculator`
- `BitCalculator`
- `Lookup`
- `Connections`
- `Listeners`
- `ARPTable`

**Example:**

```powershell
NETworkManager.exe --application:PingMonitor
```

## Internal

Internally used command line arguments.

### `--autostart`

Indicates whether the application was started automatically (via autostart).

**Example:**

```powershell
NETworkManager.exe --autostart
```

### `--restart-pid:`

Process ID of the old application process to wait for it to end if the application is restarted.

**Example:**

```powershell
NETworkManager.exe --restart-pid:35674
```
