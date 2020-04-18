# Command Line Arguments

## Parameter `--help`
Displays a dialog with possible command line arguments and values.

**Example** 
```
.\NETworkmanager.exe --help
```

## Parameter `--reset-settings`
Resets all application settings.

**Example** 
```
.\NETworkmanager.exe --reset-settings
```

## Parameter `--application:`
Application that is displayed at startup. This parameter overwrites the application setting.

**Values** 
```
None, Dashboard, NetworkInterface, WiFi, IPScanner, PortScanner, Ping, PingMonitor, Traceroute, DNSLookup, RemoteDesktop, PowerShell, PuTTY, TigerVNC, WebConsole, SNMP, DiscoveryProtocol, WakeOnLAN, HTTPHeaders, Whois, SubnetCalculator, Lookup, Connections, Listeners, ARPTable
```

**Example** 
```
.\NETworkmanager.exe --application:RemoteDeskop
```