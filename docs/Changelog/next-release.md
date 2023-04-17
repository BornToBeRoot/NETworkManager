---
layout: default
title: Next release
parent: Changelog
nav_order: 1
description: "Changelog for next release"
permalink: /Changelog/next-release
---

Version: **Next release** <br />
Release date: **xx.xx.2023**

**System requirements**

- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="\_blank"}

## What's new?
- WiFi
  - Connect to a WiFi network has been added. Supported networks / methods are `Open`, `WPS`, `WPA PSK (Pre-shared key)` and `WPA Enterprise`. To do this, right-click on a selected WiFi network to open the ContextMenu and select `Connect...`. [#2133](https://github.com/BornToBeRoot/NETworkManager/pull/2133){:target="\_blank"}
  - Disconnect from a WiFi network has been added. To do this, right-click on the connected WiFi network to open the ContextMenu and select `Disconnect`. [#2133](https://github.com/BornToBeRoot/NETworkManager/pull/2133){:target="\_blank"}
- Remote Desktop
  - Support for Remote Desktop Gateway server has been added [#2108](https://github.com/BornToBeRoot/NETworkManager/pull/2108){:target="\_blank"}
- SNMP
  - SNMP query can now be canceled (It's supported by the library now) [#2124](https://github.com/BornToBeRoot/NETworkManager/pull/2124){:target="\_blank"}
  - Common OID profiles have been added. You can now select a profile and the OID will be automatically filled in (similar to the port profiles). [#2167](https://github.com/BornToBeRoot/NETworkManager/pull/2167){:target="\_blank"}
  
## Improvements
- WiFi
  - Show current connected network [#2133](https://github.com/BornToBeRoot/NETworkManager/pull/2133){:target="\_blank"}
  - Bssid to channel ToolTip added [#2133](https://github.com/BornToBeRoot/NETworkManager/pull/2133){:target="\_blank"}
  - Show error message if scan fails (e.g. wifi adapter is unplugged while scanning) [#2133](https://github.com/BornToBeRoot/NETworkManager/pull/2133){:target="\_blank"}
  - Automatically refresh is now enabled if it was enabled on the last exit [#2116](https://github.com/BornToBeRoot/NETworkManager/pull/2116){:target="\_blank"}
- Remote Desktop
  - Domain added to connect dialog [#2165](https://github.com/BornToBeRoot/NETworkManager/pull/2165){:target="\_blank"}
  - Domain added to credentials [#2108](https://github.com/BornToBeRoot/NETworkManager/pull/2108){:target="\_blank"}
- SNMP
  - Get request can query multiple OIDs at once now [#2167](https://github.com/BornToBeRoot/NETworkManager/pull/2167){:target="\_blank"}
  - SHA256, SHA384 and SHA512 for SNMPv3 auth added [#2166](https://github.com/BornToBeRoot/NETworkManager/pull/2166){:target="\_blank"}
  - AES192 and AES256 for SNMPv3 privacy added [#2166](https://github.com/BornToBeRoot/NETworkManager/pull/2166){:target="\_blank"}
- Connections
  - Automatically refresh is now enabled if it was enabled on the last exit [#2116](https://github.com/BornToBeRoot/NETworkManager/pull/2116){:target="\_blank"}
- Listeners
  - Automatically refresh is now enabled if it was enabled on the last exit [#2116](https://github.com/BornToBeRoot/NETworkManager/pull/2116){:target="\_blank"}
- ARP Table
  - Automatically refresh is now enabled if it was enabled on the last exit [#2116](https://github.com/BornToBeRoot/NETworkManager/pull/2116){:target="\_blank"}
- Status window is now automatically closed again if it was opened by a network change event [#2105](https://github.com/BornToBeRoot/NETworkManager/pull/2105){:target="\_blank"}

## Bugfixes
- Profiles
  - OK button not disabled initially if remote desktop domain is invalid [#2165](https://github.com/BornToBeRoot/NETworkManager/pull/2165){:target="\_blank"}
  - OK button not disabled initially on unlock profile & decrypt profile file [#2133](https://github.com/BornToBeRoot/NETworkManager/pull/2133){:target="\_blank"}

## Other
- Code cleanup [#2100](https://github.com/BornToBeRoot/NETworkManager/pull/2100){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
- Update documentation for:
  - Application > WiFi [#2133](https://github.com/BornToBeRoot/NETworkManager/pull/2133){:target="\_blank"}
- Add documentation for:
  - Application > Discovery Protocol [#2178](https://github.com/BornToBeRoot/NETworkManager/pull/2178){:target="\_blank"}
  - Application > Lookup [#2112](https://github.com/BornToBeRoot/NETworkManager/pull/2112){:target="\_blank"}
  - Application > Connections [#2109](https://github.com/BornToBeRoot/NETworkManager/pull/2109){:target="\_blank"}
  - Application > Listerers [#2109](https://github.com/BornToBeRoot/NETworkManager/pull/2109){:target="\_blank"}
  - Application > ARP Table [#2109](https://github.com/BornToBeRoot/NETworkManager/pull/2109){:target="\_blank"}
  - Settings > Window [#2105](https://github.com/BornToBeRoot/NETworkManager/pull/2105){:target="\_blank"}
  - Settings > Status [#2105](https://github.com/BornToBeRoot/NETworkManager/pull/2105){:target="\_blank"}
  - Settings > HotKeys [#2105](https://github.com/BornToBeRoot/NETworkManager/pull/2105){:target="\_blank"}
  - Settings > Autostart [#2105](https://github.com/BornToBeRoot/NETworkManager/pull/2105){:target="\_blank"}
  - Settings > Profiles [#2105](https://github.com/BornToBeRoot/NETworkManager/pull/2105){:target="\_blank"}
