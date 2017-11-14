# Changelog

## Next Version
**coming soon**

### What's new?
* ARP-Table - Show and Delete 
* IP-Scanner - IP range can be specified as follows: 192.168.[0-100].1 --> 192.168.0.1, 192.168.1.1, 192.168.2.1, ... (This can be useful to scan gateways)
* Wiki renamed to Lookup
* DNS Lookup - Type (A, AAAA, PTR) moved from settings to main view
* Remote Desktop - Tags for session added (Search "tag=xxx")
* Context menu to selected data grid row added (copy single values)
* Networkinterface profiles - Custom dialog, with groups etc.

### Bugfixes / improvements
* IP-Scanner - Added default sorting (IP address, ascending)
* WakeOnLAN, OUI-Lookup - Added MAC-Address notation 0000.0000.0000
* ContextMenu design improved
* ComboBox scrollviewer scrollbar adjusted
* Small UI improvements
* Code improved/cleanup

## Version 1.3.0.0
**04.11.2017**

[NETworkMananger_v.1.3.0.0.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.3.0.0/NETworkManager_v1.3.0.0.zip)

SHA256: `978FEDA24B66488D2CDC6BEC733E0BE6BD30EB1F77D18506680DE8E1B74F249A`

### What's new?
- Remote Desktop manager added
- Rmote Desktop manager - Sessions added (add/edit/copy as /delete)
- IP-Scanner - Profiles added (add/edit/copy as/delete)
- PortScanner - Profiles added (add/edit/copy as/delete) with default profiles (1-1023, ftp, ldap(s), rdp, ssh, webserver) #27
- WakeOnLAN - Clients view changed, added dialog (add/delete/copy as/delete)
- NetworkInterfaces - Enable network interfaces (open "NCPA.cpl" with a button), when no active interfaces found #52

### Bugfixes / improvements

- Networkinterface/PortScanner - Profiles are now sorted in ascending order
- Networkinterface - IPv4Gateway NullReferenceException during launch fixed. #58
- IP-Scanner - IP-Addresses can now be sorted (ascending/descending) #16
- General - ToolTip for applications (TranslatedName) added (only visible if not expanded) #19
- SubnetCalculator - Caluclation was wrong for CIDR /31 and /32
- SubnetCalculator - Fix app crash, change Int32 to Int64 for CIDR /0 and /1
- SubnetCalculator - IPv4Split is now async
- SubnetCalculator - IPv4Split ip addresses can now be sort
- Search added for sessions/profiles/clients
- Bugfix - sessions/profiles/clients where not reset if view wasn't loaded
- Key F2 to edit (session/profile/client)
- Update MahApps.Metro - Window position will be saved again
- Code cleanup, bugfixes
---

## Version 1.2.1.0 (BETA)
**01.10.2017**

[NETworkManager_v1.2.1.0_BETA.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.2.1.0_BETA/NETworkManager_v1.2.1.0_BETA.zip)

SHA256: `A67F0DE6ED4CCEA0D966A878B69DEA9C8DB12F2F3498DFA261464F076332DF9F`

### Bugfix
- Fixed icon path

---

## Version 1.2.0.0 (BETA)
**01.10.2017**

[NETworkManager_v1.2.0.0_BETA.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.2.0.0_BETA/NETworkManager_v1.2.0.0_BETA.zip)

SHA256: `823D49F2EE0FD0C41AEE887E48729D724F722253D9EFBC7DB8AF19A4420D99B8`

### What's new? 
- NetworkInterface/Config - replaced wmiObject with netsh (not so faulty / admin rights are requested at runtime)
- Settings view in main window integrated
- DNS Lookup - statistics/informations added #45
- DNS Lookup - append domain suffix to hostname #47
- Traceroute - statistics/informations added #46
- Settings/Import - application restart is only required when application settiings have beeen overwritten
- Settings/Import - added option to `override` or `add` #49
- SubnetCalculator - IPv4-Splitter added #21
- Libary - MahApps.Metro updated to v1.6.0-aplha016
- Libary - MahApps.Metro.IconPacks updated to v1.9.1
- Libary - ControlzEx v3.0.2.4 added

### Bugfixes / improvements
- Resources - data format changed from txt to xml #50
- NetworkInterface - `template` renamed to `profile`
- WakeOnLAN - `template` renamed to `clients`
- Ping - added datetime to log #44
- DNS Lookup - application crash fixed, when custom dns server is empty
- Settings/Export - application crash fixed, when exporting empty settings #48
- NetworkInterface/Profiles - note added (if there are no profiles)
- About - added link to license for project and libaries (opens webbrowser)
- Project slogan added
- UI improved / strings & translation improved / code cleanup 

---

## Version 1.1.0.0 (BETA)
**24 Aug 2017**

[NETworkManager_v1.1.0.0_BETA.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.1.0.0_BETA/NETworkManager_v1.1.0.0_BETA.zip)

SHA256: `26E42E49198F96E8385411B75A0040D3EA0171B93666905E860284B8E8B4A9AB`

### Whats new?
- DNS lookup / resolver added (A, AAAA, CNAME, PTR & more) - based on Heijden.DNS (DNS.NET Resolver) #37
- IP-Scanner - statistics/informations added #30
- PortScanner - statistics/informations added #31
- PortScanner - now several hosts can be scanned
- CommandLine - help added (--help) #39 #40
- CommandLine - reset settings (--reset-settings) #34

### Bugfixes / improvements
- Settings now get saved, when pc is shutting down #34
- Settings now get saved, when restart the application #33
- App crash when windows language is not "en-US" or "de-DE" fixed
- Dialogs/Messages improved 
- Transparency can be enabled under settings --> appearance (this won't work on all systems!)
- WakeOnLAN - default port added to settings #35
- Minor improvements...
- Code cleanup

---

## Version 1.0.0.0 (BETA)
**24 Jul 2017**

[NETworkManager_v1.0.0.0_BETA.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.0.0.0_BETA/NETworkManager_v1.0.0.0_BETA.zip)

SHA256: `0A6AF2CE7C43E4AE9AC0221F9E01F0738C388F16043B9D71DD443BC31840ECF6`