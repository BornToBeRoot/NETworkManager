---
sidebar_position: 6
description: "Use command line arguments to control NETworkManager startup behavior, reset settings, and open a specific feature on launch."
keywords: [NETworkManager, command line arguments, CLI, command line options, automation, scripting, parameters]
---

# Command Line Arguments

The **Command Line Arguments** reference lists all CLI options accepted by `NETworkManager.exe`, enabling automation, scripting, and startup customization.

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
- `HostsFileEditor`
- `Firewall`
- `LLDPCDPCapture`
- `WakeOnLAN`
- `RDAP`
- `Whois`
- `IPGeolocation`
- `SubnetCalculator`
- `BitCalculator`
- `Lookup`
- `Connections`
- `Listeners`
- `NeighborTable`

**Example:**

```powershell
NETworkManager.exe --application:PingMonitor
```

### `--screenshot`

Starts the application in screenshot mode to take consistent screenshots, e.g. for the documentation:

- The window has a fixed size of `1280x800` and is centered on the primary screen. It can't be resized or maximized.
- The size is in device-independent pixels and is scaled with the Windows display scaling. For example, `1280x800` is `1920x1200` pixels with `150 %` scaling, which looks sharp on high-resolution screens.
- The saved window position and size are not loaded and not overwritten.
- The default theme and accent (**Dark** / **Mint**, accent glow **Medium**) are used instead of your appearance settings (they are not changed).
- The window always looks like an active window (accent glow), even if the screenshot tool takes the focus.
- Embedded **PowerShell** and **PuTTY** windows don't take the focus automatically (e.g. when the window is activated or the tab is switched), so the screenshot tool keeps it. Click into the console to type.
- The **Dashboard** shows sample data instead of your own IP addresses, hostnames, provider and location (network connection, IP geolocation, DNS resolver, speed test). It uses the address ranges and domains reserved for documentation (e.g. `203.0.113.0/24`, `2001:db8::/32`, `example.net`). The speed test shows a sample result without running a test.
- **LLDP / CDP Capture** shows a sample LLDP packet of a switch when it is opened and on **Capture**, without capturing (no administrator rights required).
- The input of the [Run command](run-command.md) always shows the same placeholder text (`Ready when your network isn't...`) instead of a random one.

**Example:**

```powershell
NETworkManager.exe --screenshot
NETworkManager.exe --screenshot --application:IPScanner
```

:::note

If NETworkManager is already running (and multiple instances are not allowed), the running instance is shown instead. Close it first.

:::

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
