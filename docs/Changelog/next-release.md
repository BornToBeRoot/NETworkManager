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
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="\_blank"}

## What's new?

New Feature
{: .label .label-green }

- Bit Calculator [#1684](https://github.com/BornToBeRoot/NETworkManager/pull/1684){:target="\_blank"} (See [documentation](https://borntoberoot.net/NETworkManager/Documentation/Application/BitCalculator) for more details)
- Global application DNS settings under `Settings > Network > DNS` added [#1733](https://github.com/BornToBeRoot/NETworkManager/pull/1733){:target="\_blank"} 

## Improvements

- Performance of DNS resolutions improved [#1733](https://github.com/BornToBeRoot/NETworkManager/pull/1733){:target="\_blank"}
- Detect new DNS servers if they have been changed e.g. by a new network connection (LAN, WLAN) or VPN connection [#1733](https://github.com/BornToBeRoot/NETworkManager/pull/1733){:target="\_blank"}
- Error messages for failed DNS resolution improved [#1733](https://github.com/BornToBeRoot/NETworkManager/pull/1733){:target="\_blank"}
- Check if folder exists in export dialog [#1760](https://github.com/BornToBeRoot/NETworkManager/pull/1760){:target="\_blank"}

## Bugfixes

- IP Scanner & Port scanner
  - Error message was not displayed when a single hostname could not be resolved [#1733](https://github.com/BornToBeRoot/NETworkManager/pull/1733){:target="\_blank"} 
- Fixed a bug in Dragablz that in certain circumstances locks the tab when you right-click on it and try to move it (See [#132](https://github.com/ButchersBoy/Dragablz/issues/132) & [#1702](https://github.com/BornToBeRoot/NETworkManager/issues/1702) for more details) [#1712](https://github.com/BornToBeRoot/NETworkManager/pull/1712){:target="\_blank"}
- Lookup - OUI, Lookup
  - Input cannot end with ";" [#1515](https://github.com/BornToBeRoot/NETworkManager/pull/1515){:target="\_blank"}

## Deprecated

- IP Scanner
  - Custom DNS server settings have been replaced by the global application DNS settings under `Settings > Network > DNS`. [#1733](https://github.com/BornToBeRoot/NETworkManager/pull/1733){:target="\_blank"}

## Other

- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="\_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="\_blank"}
- Docs improved
- Code cleanup
