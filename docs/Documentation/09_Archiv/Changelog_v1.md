---
layout: default
title: Changelog - Version 1.x
parent: Archiv
nav_order: 1
description: "Changelog and download links of all 1.x versions"
permalink: /Documentation/Legacy/Changelog-v1
---

# Version 1.11.0.0
**05.08.2019**

## Download
[NETworkMananger_v1.11.0.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.11.0.0/NETworkManager_v1.11.0.0_Portable.zip)

[NETworkMananger_v1.11.0.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.11.0.0/NETworkManager_v1.11.0.0_Setup.msi)

## Checksum
SHA256 (Portable): `CABBA6D26FB217F6689BF2E696594A0E2E713F18150BD59858201B43CC1DEF8D`

SHA256 (MSI): `5000362E3379D40F62D71B3E1042F6B56F1FF1892D528E2E3BEF6BEF7DE86AC4`

## What's new?
  * Status window added. A pop-up window appears when the network changes (up/down/ip address change).
  * Language
    * Italy added
    * Chinese added

## Improvements
  * Dashboard
    * Reconnect button is now enabled as soon as the check is finished 
  * PowerShell
    * "Reconnect" -> context menu in tab header added 
  * PuTTY
    * "Reconnect" -> context menu in tab header added 
  * TigerVNC
    * "Reconnect" -> context menu in tab header added 

## Bugfixes
  * PowerShell
    * App crash fixed when closing a dragged out tab
  * Profiles
    * VirtualizingStackPanel has been removed due to a possible crash of the app ... The Profiles tab may be slower

## Other
  * Library
    * LoadingIndicators.WPF added
    * Others updated...

# Version 1.10.1.0
**16.06.2019**

## Download
[NETworkMananger_v1.10.1.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.10.1.0/NETworkManager_v1.10.1.0_Portable.zip)

[NETworkMananger_v1.10.1.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.10.1.0/NETworkManager_v1.10.1.0_Setup.msi)

## Checksum
SHA256 (Portable): `EB468C703AF35F00B43735917644DE5490F7446D7D4818B8322752B0A70AE194`

SHA256 (MSI): `CA3C88B20A044D4652B4622557238240EA805546827FA5286C2B86BD05FFE5D0`


## What's new?
* IP Scanner
  * Custom commands can be added in settings and used in the context menu

## Improvements
* IP Scanner
  * A Profile can be added via right-clicking (context menu) on the scanned host
* Remote Desktop
  * Performance settings (network connection type, desktop background, etc.) added #187
  * Audio settings (playback, record) added
* Dashboard
  * Refresh button added
  * Added help for some settings #190
* ARP Table
  * Refresh with `F5` added
* Connections
  * Refresh with `F5` added
* Listeners
  * Refresh with `F5` added

## Bugfixes
* First application in list was always loaded at program start
* IP Scanner
  * Scan won't finish when no dns server is configured
  * App crash when copy empty hostname
* DNS Lookup
  * Settings - Add/Edit DNS server dialog fixed
* Remote Desktop
  * Printer forwarding setting was ignored



# Version 1.10.0.0
**09.04.2019**

## Download
[NETworkMananger_v1.10.0.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.10.0.0/NETworkManager_v1.10.0.0_Portable.zip)

[NETworkMananger_v1.10.0.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.10.0.0/NETworkManager_v1.10.0.0_Setup.msi)

## Checksum
SHA256 (Portable): `124005F4EB385D80698208068AB1E5AEBF45CF447390A54BAD3C80037E3BF8AC`

SHA256 (MSI): `FD9D9E4A3BBCFE8599148D4238A83D406D74FA17E67691A9ED4107AB31E142DA`

## What's new?
- Dashboard added
  - Check current network connection (local, gateway, public ip)
- IP-Scanner
  - You have to re-enter the IP range in the profile due to a change in the profile manager

## Improvements
- Network Interface 
  - Detect if network (status/address) has changed and update informations #165
  - Detect which internet protocols (IPv4 and/or IPv6) are available #181
  - "IPv4 protocol available" [false] --> hide "IPv4 address", "Subnetmask", "IPv4-Default-Gateway" #181
  - "DHCP enabled" [false]--> hide "DHCP server", "DHCP lease obtained", "DHCP lease expires" #181
  - IPv6 protocol available [false] --> hide "Link-local IPv6 address", "IPv6 address", "IPv6-Default-Gateway" #181
  - Button added "Network connections..." (Control Panel > Network and Internet > Network Connections) #169
  - Show note if no network adapter is enabled on all tabs
- Profile
  - Add " - Copy" to the name when copy a profile
-Settings
  - Language Official/Community note added

## Bugfixes
- General
  - Duplicate application icons fixed
  - Don't accept enter key in view behind if dialog is open #122
- Network Interface
  - App crash if internet protocol IPv4 is disabled fixed #181
  - Possible NullReferenceException fixed
- PuTTY 
  - Setting "Serial Line" will be saved now #179
  - Restart session fixed #180

## Other
- Libary updates
  - SharpSNMP
  - IPNetwork2

# Version 1.9.0.0
**17.02.2019**

## Download
[NETworkMananger_v1.9.0.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.9.0.0/NETworkManager_v1.9.0.0_Portable.zip)

[NETworkMananger_v1.9.0.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.9.0.0/NETworkManager_v1.9.0.0_Setup.msi)

## Checksum
SHA256 (Portable): `D9CE21B8C26F9DC1D43DEC9B2A4119F5A98E066C318DB018F6F4A7B43A409F3B`

SHA256 (MSI): `2C01DE0C0B0F01D92062AAE9F3F3C9EB46887D5D7A9CC1E9ADBA3512C07D2EF5`

## What's new?
* Network Interface - Bandwidth meter added
* PowerShell with tabs and profiles added
* TightVNC replaced with TigerVNC #175
* Port Scanner - Now accepts ranges as input für hosts (like `192.168.178.0 - 192.168.178.100` or `example.com/24`)
* Profiles - Default settings can be overridden for following features
  * Remote Desktop
  * PowerShell 
  * PuTTY
  * TigerVNC
  * WakeOnLAN

## Improvements
* Profile - Hostnames can now be resolved to IPv4 address
* Port Scanner - Threads can now be adjusted
  * Concurrent hosts
  * Concurrent ports per hosts
* PuTTY settings added
  * Default username
  * Default additional command line
* PuTTY/TigerVNC - Window integration improved / application no longer freezes when waiting for the process
* Connections - Stop refresh when view is not visible
* Listeners - Stop refresh when view is not visible
* ARP Table - Stop refresh when view is not visible

## Bugfixes
* PuTTY/TigerVNC - Fixed an overflow exception when a connection error occurred
* Profiles - The initially selected profile could not be edited
* Fix DPI issues with RDP, PuTTY, TigerVNC and PowerShell
* Fix DataGrid sort #156

## Other
* Language files updated
* MAC address and vendors updated
* Ports and descriptions updated
* Whois server list updated
* Third party libraries updated
  * IPNetwork2
  * Newtonsoft.Json
  * Lextm.SharpSnmpLi
* Third party libraries added
  * LiveCharts
  * LiveCharts.Wpf

# Version 1.8.3.0
**26.12.2018**

## Download
[NETworkMananger_v1.8.3.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.8.3.0/NETworkManager_v1.8.3.0_Portable.zip)

[NETworkMananger_v1.8.3.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.8.3.0/NETworkManager_v1.8.3.0_Setup.msi)

## Checksum
SHA256 (Portable): `DD1122A0748387384893F22B52103ED80BF8591F310B22DDD62EB97EDC847A1A`

SHA256 (MSI): `BB9B66D526066FCBBFE15139333C350602D7325C9D58E319E6CC91164ED93D82`

## Bugfixes
* Configure Network Interface fixed #162

# Version 1.8.2.0
**11.12.2018**

## Download
[NETworkMananger_v1.8.2.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.8.2.0/NETworkManager_v1.8.2.0_Portable.zip)

[NETworkMananger_v1.8.2.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.8.2.0/NETworkManager_v1.8.2.0_Setup.msi)

## Checksum
SHA256 (Portable): `FE94199C7DE0510D47B5E87C8F50AEAE2E8E029D892615AA7B753D7378694069`

SHA256 (MSI): `3C3A650CC0444E28670C24867026F7425D3C2C3370F62F51AFCD6AFBB0E6AF40`

## What's new?
* Data can now be exported from the following features #150
  * IP Scanner (csv, xml, json)
  * Port Scanner (csv, xml, json)
  * Ping (csv, xml, json)
  * Traceroute (csv, xml, json)
  * DNS Lookup (csv, xml, json)
  * SNMP (csv, xml, json)
  * HTTP Headers (txt)
  * Whois (txt)
  * Subnet Calculator - Subnetting (csv, xml, json)
  * Lookup - OUI (csv, xml, json)
  * Lookup - Port (csv, xml, json)
  * Connections (csv, xml, json)
  * Listeners (csv, xml, json)
  * ARP Table (csv, xml, json)
* Network Interface - Config - Add IPv4 address added (allows adding an additional ip address to the network interface)
* Subnet Calculator - Supernet feature replaced with WideSubnet feature from IPNetwork. Currently works only with IPv4 #151
* SplashScreen added

## Improvements
* Ping - Sort columns removed
* Ping - Highlight timeout
* Whois - Scrollbar added to TextBox
* HTTP Headers - Scrollbar added to TextBox
* Profile - Added note on how to start a new credential file if you forgot your password
* Profiles - DataGrid rendering performance improved
* Profiles - Delay search when typing (DataGrid reloads less often)
* Profiles - Sort fixed
* Credentials - DataGrid rendering performance improved
* Credentials - Delay search when typing  (DataGrid reloads less often)
* Credentials - Sort fixed
* Language files updated
* UI improved (Vertical text improved, borders added/revised, Add tab message is now clickable)

## Bugfixes
* IP Scanner - Sort columns fixed #155
* Ping - Ping was canceled, when the ip-address did not resolve a hostname
* Remote Desktop - Automatic adjustment of the screen resolution was broken
* Settings - Applications - icons fixed (Remote Desktop, PuTTY, TightVNC)
* Settings - Profiles - Edit disabled when multiple profiles are selected
* Settings - Credentials - Edit disabled when multiple credentials are selected

# Version 1.8.1.0
**25.10.2018**
## Download
[NETworkMananger_v1.8.1.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.8.1.0/NETworkManager_v1.8.1.0_Portable.zip)

[NETworkMananger_v1.8.1.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.8.1.0/NETworkManager_v1.8.1.0_Setup.msi)

## Checksum
SHA256 (Portable): `A69DAE0D2170DB5C809FB38484FC4AAA5DBE53F9DBF694BF3146D27A88574AC1`

SHA256 (MSI): `CB2F9B4B1A269965AC26D3C5FE7D76D4D2B7CAED59549CF8599F83E2032CCF37`

## Improvements
* IP Scanner - Settings - "Read the MAC address from the ARP cache" renamed to "Resolve MAC address and vendor" #143
* Language files updated

## Bugfixes
* PuTTY/TightVNC - Window design issue fixed #152

# Version 1.8.0.0
**21.10.2018**

## Download
[NETworkMananger_v1.8.0.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.8.0.0/NETworkManager_v1.8.0.0_Portable.zip)

[NETworkMananger_v1.8.0.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.8.0.0/NETworkManager_v1.8.0.0_Setup.msi)

## Checksum
SHA256 (Portable): `254C2E57785F5B04088D1D0610881CEC12545EBCFA6E3EC4B0C662B228136CC5`

SHA256 (MSI): `F3470CA8626A0E2E8A7342D7EC6E1280B7955281B49F162B63215D1FB038AD67`

## What's new?
* Whois - Query the whois via port 43 to get domain informations
* TightVNC - Open multiple VNC sessions in tabs (similar to Remote Desktop or PuTTY)
* Settings/General - Hide some applications from the bar in the main window #133
* Network Interface - Configure - `ipconfig /release` and `ipconfig /renew` added
* DNS Lookup - Preset dns server, add/edit/delete custom dns servers, dns server can now selected in the view #130
* DNS Lookup - Profiles added #125
* HTTP Headers - Profiles added #124
* Whois - Profiles added
* PuTTY - Restart session (via ContextMenu in TabItem Header)

## Improvements

* Network Interface - Configure - Flushdns changed from `dnsapi.dll` to `ipconfig /flushdns`
* Remote Desktop/PuTTY - Connect / reconnect view improved

## Bugfixes
* The custom settings path is lost when updating to a newer version
* App crash fixed, when copying strings which are translated (e.g. ping status)
* App crash fixed, when copying strings (`Clipboard.SetText()` replaced with `Clipboard.SetDataObject()`)
* App crash fixed, when closing dragged out IPScanner and SNMP tab

# Version 1.7.1.0
**14.08.2018**

## Download
[NETworkMananger_v1.7.1.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.7.1.0/NETworkManager_v1.7.1.0_Portable.zip)

[NETworkMananger_v1.7.1.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.7.1.0/NETworkManager_v1.7.1.0_Setup.msi)

## Checksum
SHA256 (Portable): `45F7F57EB58AA7DD6BD91831FE6088BBD8412C80372057E3E573B2FFAA84646F`

SHA256 (MSI): `3382DE599621F0AFBD31002F04FFBFE36D022AC4F970848F230378C60F65D7DD`

## Bugfixes
* Network Interface - Configure - Valid subnetmask like 255.255.0.0 was not accepted as input #136
* Formatting fixed on some messages...

# Version 1.7.0.0
**11.08.2018**

## Download
[NETworkMananger_v1.7.0.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.7.0.0/NETworkManager_v1.7.0.0_Portable.zip)

[NETworkMananger_v1.7.0.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.7.0.0/NETworkManager_v1.7.0.0_Setup.msi)

## Checksum
SHA256 (Portable): `5D69C0A1DE87D0868078301ADBE718B0CFE7C5B40271F722B53B971F155DD1D3`

SHA256 (MSI): `15EB077FA5C98A83DE0AA2DBEB688D481FEEB39083DBD2C6F37CD5A858D2499F`

## What's new?
* Profile manager rewritten - Profiles are now shared between the tools. You can choose where the profile should be displayed. #92

**ATTENTION: _This has the consequence that all profiles must be created again. However, the new profile manager is more comfortable and hosts can be shared among the different tools!_**
* Language file es-ES (Spanish) added. Thanks to @MS-PC for the PR!
* Language files are now in resx format (previously xaml). Transifex is now used for the translation. The application must be restarted after changing the language. #131
* Ping - Hostname is now resolved when an IP address is entered. #105

## Improvements
* DNS Lookup - Show only most common query types (A, AAAA, ANY, CNAME, MX, NS, PTR, SOA, LOC, TXT), can be changed in the settings.
* Remote Desktop - AirSpace fixer added, interface improved when dialog is open.
* Remote Desktop - Show hint if transparency is enabled.
* Remote Desktop/Settings - Enable / disable Windows key combinations in remote session (default=enabled).
* PuTTY - Border of the PuTTY window removed.
* PuTTY - AirSpace fixer added, interface improved when dialog is open.
* PuTTY - Show hint if transparency is enabled.
* PuTTY/Settings - PuTTY.exe can be dragged into the textbox to set the file path.
* Settings/Profiles - Double click or F2 to edit, DEL to delete.
* Settings/Credentials - Double click or F2 to edit, DEL to delete.
* Settings/Appearance - Minimum transparency set to 25.
* Settings/Import - Backup file can be dragged into the textbox to set the file path.
* Settings/Settings - Folder can be dragged into the textbox to set the settings location.
* Settings - Reset settings if file is corrupt or incompatible with current version.
* Profile - Multiple profiles can be deleted at once.
* Groups - ComboBox with existing groups added to edit dialog.
* Credentials - Multiple credentials can be deleted at once.
* Credentials - Encryption -> Iterations changed from 25k to 100k.
* Application/Tool Title is hidden by default (can be enabled in settings/window).
* StatusBar removed, update notification will appear next to the documentation icon in the window title.
* First tab can now be dragged out .
* Dragged out tabs now supports transparency (if enabled in Settings/Appearance).

## Bugfixes
* App crash fixed - When two or more tabs are dragged out into a new window and from this window a tab is dragged out, the application crashes, because the `InterTabController.Partition` (dragablz/TabablzControl) was not set.

# Version 1.6.3.0
**08.06.2018**

## Download
[NETworkMananger_v1.6.3.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.6.3.0/NETworkManager_v1.6.3.0_Portable.zip)

[NETworkMananger_v1.6.3.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.6.3.0/NETworkManager_v1.6.3.0_Setup.msi)

## Checksum
SHA256 (Portable): `EAE9BAA6B360C5BDF22C6BB928404467D10150A0407BFDBE8DFAF292F47991C5`

SHA256 (MSI): `B0F837C819AF15A0CAEF01405472B6EBE5972470F23F86437D321322A89C1352`

## What's new?
* Subnet Calculator - IPv6 support added to Calculator #85
* Subnet Calculator - Splitter renamed to Subnetting, IPv6 support added #85
* Subnet Calculator - Supernetting for IPv4/IPv6 added #85
* Connections - Show all active tcp connections (similar to `netstat`)
* Listeners - Show all active tcp/udp listeners (similar to `netstat`)
* Language file ru-RU (Russian) added

## Improvements
* IP Scanner - Custom DNS server can be configured in the settings to resolve ip addresses #90
* IP Scanner - Allow hostname.example.com/24 or hostname/255.255.255.128 as input #93
* Traceroute - Open other tools via context menu (IP Scanner, Port Scanner, Ping, DNS Lookup, Remote Desktop, PuTTY, SNMP)
* ARP Table - Auto refresh added
* ARP Table - Search improved, filter by Multicast
* Settings/About - Button to open folder with licenses added
* Window - Hide current application title (settings > window > show current application title) #96
* Resize profiles/sessions/clients #95
* Hide statstics #98
* Tabs now have a fixed size and a ToolTip with the title
* OUI list updated
* Port list updated
* Interface improved (ToolTip added, more...)

## Bugfixes
* Port Scanner - Empty tab header fixed (when the host is empty in the profile)
* Traceroute - Value cannot be null #87

# Version 1.6.2.0
**13.05.2018**

## Download
[NETworkMananger_v1.6.2.0_Portable.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.6.2.0/NETworkManager_v1.6.2.0_Portable.zip)

[NETworkMananger_v1.6.2.0_Setup.msi](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.6.2.0/NETworkManager_v1.6.2.0_Setup.msi)

## Checksum
SHA256 (Portable): `2DB129E28227CC83F3D7B92EBBE1CCBAB9C4B805D281CBD46C7D341E721505F7`

SHA256 (MSI): `9DBED1062CCC8B99E682227C31B7E272CBA126B700CE5A0095D1AAD9ABE4CDDB`

## What's new?
* MSI package added

## Improvements
* IP Scanner - Tabs added
* IP Scanner - Open other tools via context menu (Port Scanner, Ping, Traceroute, DNS Lookup, Remote Desktop, PuTTY, SNMP)
* Port Scanner - Tabs added
* Ping - Profiles added
* Traceroute - Tabs added
* Traceroute - Profiles added
* DNS Lookup - Tabs added
* SNMP - Tabs added
* HTTP Headers - Tabs added
* Wake on LAN - Disable button while sending / indicator added
* Header to the window added (if a tab is dragged out of the default view)
* Settings - Settings location (folder) can be set via drag & drop
* Import / Export - Import filepath can be set via drag & drop
* Interface improved

## Bugfixes
* Do not change the settings view if the user has searched and reopen the settings
* Settings now moved correctly if the user don't want to overwrite files
* Settings/Appearance - View was not displayed correctly

# Version 1.6.1.0
**24.04.2018**

[NETworkMananger_v1.6.1.0.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.6.1.0/NETworkManager_v1.6.1.0.zip)

SHA256: `E911F59EB8137FC88CDCFCC5741FBD1FB7C87FF4CC403C1BE0C506F57C3465FA`

## Bugfixes
* Remote Desktop / PuTTY not shown, when edit a group and click cancel
* Network Interface - NullReferenceException fixed (IPv4Gateway is a collection which can be empty) #83
* Design of disabled buttons fixed

# Version 1.6.0.0
**22.04.2018**

[NETworkMananger_v1.6.0.0.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.6.0.0/NETworkManager_v1.6.0.0.zip)

SHA256: `5C8036D49ED268144C058581EC6A5B2E0101D41A1705BE38F35153656A93C759`

## What's new?
* PuTTY with tabs added
* Documentation added (can be opened from within the application)

## Improvements
* Network Interface / Config - Button to flush DNS cache added
* IP Scanner - Show scan result for all ip addresses
* Ping - Close tabs with middle mouse button
* Ping - Cancel ping when tab is closed
* Remote Desktop - Disconnect when tab is closed
* Remote Desktop - Close tabs with middle mouse button
* Remote Desktop - Search sessions by host (hostname)
* Rename groups #78
* Close application via windows taskbar "X Close window", if it is minimized and the setting "Minimize main windows instead of terminating the application" is enabled.
* Settings - When moving settings... Ask to overwrite existing settings in destination folder.
* Profiles/Sessions/Clients - Fade out scroll bar added
* Nuget packages updated (Lextm.SharpSnmpLib, MahApps.Metro, MahApps.Metro.IconPacks, Dragablz)

## Bugfixes
* Ping - NullReferenceException fixed when user aborts a DNS query for a host that does not exist
* Inputs like hostname, ports, ip range could be overwritten by profiles during the scan

# Version 1.5.0.0

**25.02.2018**

[NETworkMananger_v1.5.0.0.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.5.0.0/NETworkManager_v1.5.0.0.zip)

SHA256: `B70D60EF1DCF285F6DB190C7034E2228F6ABC6B2AF3BAE52B832082FB46681D1`

## What's new?
* SNMP - Get/Walk/Set v1, v2c, v3 added
* PING - Tabs added
* Update check - Check the latest release on github and notify the user about the update #34
* Custom Themes/Accent added - Self-created theme/accent-files can be stored in the "Themes" folder (Documentation: [How to create a custom theme and accent?](https://github.com/BornToBeRoot/NETworkManager/blob/master/Documentation/en-US/HowTo/Create_custom_theme_and_accent.md))

## Bugfixes / improvements
* Remote Desktop - Check if RDP 8.1 is installed (Documentation: [How to install RDP 8.1 on Windows 7/Server 2008 R2](https://github.com/BornToBeRoot/NETworkManager/blob/master/Documentation/en-US/HowTo/Install_RDP_8dot1_on_Windows6dot1.md)) 
* Remote Desktop - More options for screen size added (size of the current view, auto adjust screen, custom size, drop down with common values)
* Remote Desktop - History for hosts added
* Remote Desktop - Custom port can be configured
* Remote Desktop - Connect via mstsc.exe from session context menu
* IP Scanner - Clients are now displayed if no MAC address was found
* IP Scanner/Port Scanner - Profile view (fold/unfold) are now saved correctly
* Port Scanner - Check if scan is finished, when user has canceled
* Port Scanner - Validation fixed (multiple hosts can not end with `;`)
* DNS Lookup - Query multiple dns servers (Add them in settings and separate them with `;`)
* DNS Lookup - Make several requests at the same time (separate them with `;`)
* DNS Lookup - Custom port can be set in the settings
* DNS Lookup - Resolve CNAME only on ANY requests
* Subnet Calculator - Displays a warning when calculating a large number of subnets #56
* Subnet Calculator - IPv4 Splitter - Removed /0 and 0.0.0.0 as subnetmask input, because it is not a valid subnetmask
* Subnet Calculator - IPv4 Splitter - Disable combobox while calculating
* HTTP Headers - Timeout can be configured in settings
* Bring window to front if minimized and the option "Confirm close" is enabled
* Open settings from tray fixed
* Added search for settings
* PasswordBox - Reveal password added, caps lock icon improved, watermark added
* UI improved
* Documentation can be opened out of the application

## 

# Version 1.4.2.0
**01.01.2018**

[NETworkMananger_v1.4.2.0.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.4.2.0/NETworkManager_v1.4.2.0.zip)

SHA256: `10FF4C0DF4812CC3B661ACC1614B3B3496298F98A9AC2F8696DCF9D2AB5C3A19`

## What's new?
* CredentialManager added

## Bugfixes / improvements
* Network Interface - "Apply" added to profile context menu
* Network Interface - Reload network interfaces with F5
* IP Scanner - "Scan" added to profile context menu
* IP Scanner - Don't show cancel message if already finished
* Port Scanner - "Scan" added to profile context menu
* Remote Desktop - Select credential in add/edit profile dialog
* Remote Desktop - Color depth was not saved
* Remote Desktop - Settings "EnableCredSspSupport" and "AuthenticationLevel" added
* Remote Desktop - Connect as...
* Wake on LAN - "Wake Up" added to client context menu
* Traceroute - Async removed due to too many bugs
* Traceroute - Enable/disable reverse dns lookup
* ARP Table - Add/delete entry
* ARP Table - Multicast indicator added
* ARP Table - View improved
* ARP Table - Reload animation added
* HTTP-Headers - URI validation changed
* Hotkeys on autostart fixed
* Tooltip removed from settings button when application list is expanded
* All dialogs improved (help message, watermarks)
* Application list / settings - Scroll into view
* Groups (Remote Desktop, Wake on Lan, etc.) are now sorted ascending
* ComboBox design improved
 
# Version 1.4.1.0
**28.11.2017**

[NETworkMananger_v1.4.1.0.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.4.1.0/NETworkManager_v1.4.1.0.zip)

SHA256: `F90A24DA927A9FEA5EC55A3A25B2926D11B3B46ABD398FD868C8304984EC8AB5`

## What's new?
* HTTP Headers added - read http headers from websites

## Bugfixes / improvements
* Crash when open second instance fixed (mutex)
* Remote Desktop - Disconnect when tab is pulled out #65
* Network Interface - Fixed a bug where the interace cannot be configured if the name contains a space
* Network Interface - Gateway was not automatically published in the configure view
* Traceroute - Cancel-button won't cancel traceroute #66
* Traceroute - Improve performance
* Settings/Appearance/Transparency warning added
---

# Version 1.4.0.0
**22.11.2017**

[NETworkMananger_v1.4.0.0.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.4.0.0/NETworkManager_v1.4.0.0.zip)

SHA256: `3E79772EA13D32EE702864BD38787CDC9FAA464F6E34462532FE89CC01913068`

## What's new?
* ARP Table - Show and delete 
* IP Scanner - IP range can be specified as follows: 192.168.[1-100,200].1 --> 192.168.1.1, 192.168.2.1, {..}, 192.168.100.1, 192.168.200.1 (This can be useful to scan gateways)
* Wiki renamed to Lookup
* DNS Lookup - Type (A, AAAA, PTR) moved from settings to main view
* Remote Desktop - Tags for session added (Search "tag=xxx")
* Context menu to selected data grid row added (copy single values)
* Networkinterface profiles - Custom dialog, with groups etc.

## Bugfixes / improvements
* Remote Desktop - Disconnect on tab close
* Remote Desktop - Reconnect button added
* Remote Desktop - Adjusted screen automatically
* IP-Scanner - Added default sorting (IP address, ascending)
* Wake on LAN, OUI-Lookup - Added MAC-Address notation 0000.0000.0000
* ContextMenu design improved
* Custom confirm delete/remove dialog
* ComboBox scrollviewer scrollbar adjusted
* Small UI improvements
* Code improved/cleanup
---

# Version 1.3.0.0
**04.11.2017**

[NETworkMananger_v1.3.0.0.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.3.0.0/NETworkManager_v1.3.0.0.zip)

SHA256: `978FEDA24B66488D2CDC6BEC733E0BE6BD30EB1F77D18506680DE8E1B74F249A`

## What's new?
- Remote Desktop manager added
- Rmote Desktop manager - Sessions added (add/edit/copy as /delete)
- IP-Scanner - Profiles added (add/edit/copy as/delete)
- PortScanner - Profiles added (add/edit/copy as/delete) with default profiles (1-1023, ftp, ldap(s), rdp, ssh, webserver) #27
- WakeOnLAN - Clients view changed, added dialog (add/delete/copy as/delete)
- NetworkInterfaces - Enable network interfaces (open "NCPA.cpl" with a button), when no active interfaces found #52

## Bugfixes / improvements

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

# Version 1.2.1.0
**01.10.2017**

[NETworkManager_v1.2.1.0_BETA.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.2.1.0_BETA/NETworkManager_v1.2.1.0_BETA.zip)

SHA256: `A67F0DE6ED4CCEA0D966A878B69DEA9C8DB12F2F3498DFA261464F076332DF9F`

## Bugfix
- Fixed icon path

---

# Version 1.2.0.0
**01.10.2017**

[NETworkManager_v1.2.0.0_BETA.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.2.0.0_BETA/NETworkManager_v1.2.0.0_BETA.zip)

SHA256: `823D49F2EE0FD0C41AEE887E48729D724F722253D9EFBC7DB8AF19A4420D99B8`

## What's new? 
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

## Bugfixes / improvements
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

# Version 1.1.0.0
**24 Aug 2017**

[NETworkManager_v1.1.0.0_BETA.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.1.0.0_BETA/NETworkManager_v1.1.0.0_BETA.zip)

SHA256: `26E42E49198F96E8385411B75A0040D3EA0171B93666905E860284B8E8B4A9AB`

## Whats new?
- DNS lookup / resolver added (A, AAAA, CNAME, PTR & more) - based on Heijden.DNS (DNS.NET Resolver) #37
- IP-Scanner - statistics/informations added #30
- PortScanner - statistics/informations added #31
- PortScanner - now several hosts can be scanned
- CommandLine - help added (--help) #39 #40
- CommandLine - reset settings (--reset-settings) #34

## Bugfixes / improvements
- Settings now get saved, when pc is shutting down #34
- Settings now get saved, when restart the application #33
- App crash when windows language is not "en-US" or "de-DE" fixed
- Dialogs/Messages improved 
- Transparency can be enabled under settings --> appearance (this won't work on all systems!)
- WakeOnLAN - default port added to settings #35
- Minor improvements...
- Code cleanup

---

# Version 1.0.0.0
**24 Jul 2017**

[NETworkManager_v1.0.0.0_BETA.zip](https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.0.0.0_BETA/NETworkManager_v1.0.0.0_BETA.zip)

SHA256: `0A6AF2CE7C43E4AE9AC0221F9E01F0738C388F16043B9D71DD443BC31840ECF6`