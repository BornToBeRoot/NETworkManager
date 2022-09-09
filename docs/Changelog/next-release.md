---
layout: default
title: Next release
parent: Changelog
nav_order: 1
description: "Changelog for next release"
permalink: /Changelog/next-release
---

# Next release

**System requirements**
- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="_blank"}
- [Microsoft Edge - WebView2 Runtime (Evergreen)](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="_blank"}

## What's new?

  
## Improvements
- Port Scanner
  - Add more port profiles (LDAP, HTTP proxy, Filetransfer) [#1518](https://github.com/BornToBeRoot/NETworkManager/pull/1518){:target="\_blank"}
- PowerShell & PuTTY
  - Focus embedded window when the application has received the focus [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}
  - Focus embedded window when application is selected again [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}
  - Focus embedded window when switching tabs [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}


## Bugfixes
- Dashboard / Status Window
  - Handle null exception properly [#1510](https://github.com/BornToBeRoot/NETworkManager/pull/1510){:target="\_blank"}
- Remote Desktop, PowerShell, PuTTY & TigerVNC
  - "Unlock profile" dialog is now displayed correctly if an embedded window is already open [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}
PowerShell & TigerVNC
  - Embedded window is now resized correctly after the inital connect [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}
- Settings > Profiles
  - App crash fixed when a profile file is deleted and an encrypted but locked profile file is selected [#1512](https://github.com/BornToBeRoot/NETworkManager/pull/1512){:target="\_blank"}

## Other
- Code cleanup [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="_blank"}
