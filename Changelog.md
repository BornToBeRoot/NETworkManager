# Version 2020.1.0
Date: **26.01.2020**

| File | Checksum
|---|---|
|[Setup](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.1.0/NETworkManager_2020.1.0_Setup.exe)| `A7BD0182269F012701D56285141A66279F41145F748539C7233C3129BE3765CB` |
|[Portable](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.1.0/NETworkManager_2020.1.0_Portable.zip)| `BEA66D1B8E1DE820B6077FD1F98ABDF5BDD4D7CD0477FC27941EFED326DCCEAD` |
|[Archiv](https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.1.0/NETworkManager_2020.1.0_Archiv.zip)| `497C6DEFAD22B074B0E8D0E43948545128503512434C096D221D2978B1344F91` |

## What's new?
- Discovery Protocol - Capture LLDP and/or CDP network packages and display informations like Port, Description, VLAN, etc. #196
- Web Console added #244
- Settings > Appearance 
  - Transparency feature removed. Remote Desktop, PowerShell, PuTTY and TigerVNC don't work while transparency is enabled. #220

## Improvements
- Network Interface > Bandwidth
  - Labels / values improved in the network usage section #235
  - ToolTip improved #219
- DNS Lookup
  - Error message now shows the ip address of the dns server #256 
- Command Line Parameter added
  - `--application:[Dashboard|IPScanner|etc.]` #237
- After restarting the application, the last view is displayed again #237
- Settings > Language
  - View improved #231
- Profiles
  - Rows (profiles) should now load faster (tested with ~5k profiles)
  - Search improved on slow systems with many profiles #227

## Bugfixes
- IP Scanner
  - Context menu redirect ro Ping Monitor #225 
  - Context menu icons are now correct #257
- Traceroute
  - Context menu redirect ro Ping Monitor #225  
  - Context menu icons are now correct #257
- Some bugs in the UI fixed (label, translation, placeholder, etc.) 

## Libraries
  - WebView added
  - Others updated

## Other
  - Language files updated

# Version 2019.12.0
Date: **25.12.2019**

| File | Checksum
|---|---|
|[Setup](https://github.com/BornToBeRoot/NETworkManager/releases/download/2019.12.0/NETworkManager_2019.12.0_Setup.exe)| `C615367946A818B4E67632FA99937723B4006385D86F62F52842709DC35CBA1F` |
|[Portable](https://github.com/BornToBeRoot/NETworkManager/releases/download/2019.12.0/NETworkManager_2019.12.0_Portable.zip)| `1A5A16A863425D827E1C58D711C337873C4C44B914D44F3FC27E327043597078` |
|[Archiv](https://github.com/BornToBeRoot/NETworkManager/releases/download/2019.12.0/NETworkManager_2019.12.0_Archiv.zip)| `1942EF7B3541782CB83F37ED802E8FF98DD63579F5772D792462EACF82BE7E72` |

⚠️  **SYSTEM REQUIREMENTS** ⚠️ 
- Windows 10 Build 1809 
- .NET-Framework 4.7.2

## What's new

- WiFi (Networks, Channels)
- Ping Monitor
- Profile - You can now add multiple profile files and select them in the window title bar in a drop down
   - If you want to migrate your profile file from one of the pre-releases... copy the file `%appdata%\NETworkManager\Settings\Profiles.xml` to `%appdata%\NETworkManager\Profiles\Profiles.xml` or in the portable version from the `settings` folder into the new `profiles` folder.
- Version number changed from `Version 1.x.x.x` to `Version {year}.{month} Patch {number}`
- Credential feature removed
- Settings - Make portable feature removed

## Improvements
- Dashboard
  - New icon
- Network Interface - Bandwidth
  - Measure bandwidth over time
  - TooTip improved
  - Legend added
- WiFi
  - Channel - Signal strength sections added
  - Channel - Values (signal strength / channel) are visible, even if no network has been scanned yet
  - Channel - ToolTip improved
- IP Scanner 
  - New icon
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
  - Hostname lookup --> DNS replaced with DnsClient
- Port Scanner 
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
  - Hostname lookup --> DNS replaced with DnsClient
- Ping
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
  - Hostname lookup --> DNS replaced with DnsClient
- Ping Monitor
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
  - Hostname lookup --> DNS replaced with DnsClient
- Traceroute
  - Performance improved when resolving hostnames. Default DNS replaced with DnsClient for reverse lookups.
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
  - Hostname lookup --> DNS replaced with DnsClient
- DNS Lookup
  - Heijden.DNS replaced with DnsClient.NET
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
- Remote Desktop
  - Animation when closing a dialog is now smooth
  - Settings - Display - "Use current view size as screen size" is now default
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
- PowerShell
  - New icon
  - DPI issues fixed
  - Animation when closing a dialog is now smooth
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
- PuTTY
  - New icon
  - Animation when closing a dialog is now smooth
  - DPI issues fixed
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
- TigerVNC
  - Animation when closing a dialog is now smooth
  - DPI issues fixed
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
- HTTP Headers
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
- Whois
  - The selected profile can be used with the `Enter` key when the search textbox or listview is in focus
- Lookup
  - New icon

## Bugfixes
- IP Scanner
  - Heijden.DNS replaced with DnsClient.NET #209
- Ping Monitor
  - ScrollBar fixed

## Libraries
  - Heijden.DNS removed from source
  - Microsoft.Windows.SDK.Contracts added
  - DnsClient.NET added
  - Others updated

## Other
  - Language files updated
  - Resources (OUI, Ports, Whois) updated
  - Some small bugfixes and improvements

---

You can find the changelog for version 1.x [here](Changelog_v1.md).

[back](../README.md)