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
- New Icon/Logo for the application [#1362](https://github.com/BornToBeRoot/NETworkManager/pull/1362){:target="_blank"} [#1371](https://github.com/BornToBeRoot/NETworkManager/pull/1371){:target="_blank"}
- Profiles reworked [BREAKING CHANGE] [#1236](https://github.com/BornToBeRoot/NETworkManager/pull/1236){:target="_blank"}
  Group-specific settings can now be set for:
  - Remote Desktop (Credentials for a group of servers, Settings)
  - PowerShell (Settings)
  - PuTTY (Settings)
  - TigerVNC (Settings)

  The inheritance works as follows: General Settings > Group > Profile (Profile overwrites group, group overwrites general settings)

  **The profiles are migrated with a PowerShell script when the app is executed for the first time / when the profiles are loaded. To do this, they must first be decrypted in an old version.**
  
- PuTTY
  - Custom profile `NETworkManager` will be added to the registry (`HCKU\Software\SimonTatham\PuTTY\Sessions\NETworkManager`) which will set the PuTTY background to the application background [#1358](https://github.com/BornToBeRoot/NETworkManager/pull/1358){:target="_blank"} [#1236](https://github.com/BornToBeRoot/NETworkManager/pull/1236){:target="_blank"}
  - Add context menu button (right click on the tab) to fix the PuTTY embedded window size [#1366](https://github.com/BornToBeRoot/NETworkManager/pull/1366){:target="_blank"}
  - Try to automatically fix the embedded window size after the initial connect [#1236](https://github.com/BornToBeRoot/NETworkManager/pull/1236){:target="_blank"} [#1376](https://github.com/BornToBeRoot/NETworkManager/pull/1376){:target="_blank"}
  
## Improvements
- Profile page in settings improved [#1236](https://github.com/BornToBeRoot/NETworkManager/pull/1236){:target="_blank"}
- Validation of entries in the profile dialog improved [#1236](https://github.com/BornToBeRoot/NETworkManager/pull/1236){:target="_blank"} [#1283](https://github.com/BornToBeRoot/NETworkManager/issues/1283){:target="_blank"}
- Minimum required window size reduced to 800x600 [#1366](https://github.com/BornToBeRoot/NETworkManager/pull/1366){:target="_blank"} [#1275](https://github.com/BornToBeRoot/NETworkManager/issues/1275){:target="_blank"}
- Default history entries increased from 5 to 10 [#1372](https://github.com/BornToBeRoot/NETworkManager/issues/1372){:target="_blank"}
- Default background job time decreased from 15 to 5 minutes (save settings, profiles in background and not only when closing the application) [#1236](https://github.com/BornToBeRoot/NETworkManager/pull/1236){:target="_blank"}
- Edit group button in the profiles list is not visible when the group name is longer than the width of the profiles list [#1236](https://github.com/BornToBeRoot/NETworkManager/pull/1236){:target="_blank"}
- Port Scanner
  - MySQL and PostgreSQL ports added to port profile [4334b64](https://github.com/BornToBeRoot/NETworkManager/commit/4334b649e0f73ab419e524f50c438b128288d8e3){:target="_blank"}

## Bugfixes
- Visibility of the min/max/close button on the pulled out window fixed [#1366](https://github.com/BornToBeRoot/NETworkManager/pull/1366){:target="_blank"}
- App crash when building with SDK .NET 6.0.2 and running the app on 6.0.0 or 6.0.1 fixed [#1236](https://github.com/BornToBeRoot/NETworkManager/pull/1236){:target="_blank"} [#1381](https://github.com/BornToBeRoot/NETworkManager/issues/1381){:target="_blank"}
- App crash when renaming a profile file fixed [#1318](https://github.com/BornToBeRoot/NETworkManager/issues/1318){:target="_blank"}
- Language `zh-CN` and `zh-TW` is missing in dotnet publish. Build script changed from `dotnet` to `msbuild` [#1316](https://github.com/BornToBeRoot/NETworkManager/issues/1316){:target="_blank"}
- Remote Desktop 
  - Connection via Profile leads to error message "Error Code 4 (Total login limit was reached)" fixed [#1265](https://github.com/BornToBeRoot/NETworkManager/issues/1265){:target="_blank"}
- PowerShell
  - Validate host input in connect dialog [#1373](https://github.com/BornToBeRoot/NETworkManager/issues/1373){:target="_blank"}
- PuTTY
  - Fixed that some arguments from the connect dialog of PuTTY (like privatekey) were not applied correctly. Fix was also applied to remote desktop, powershell, tigervnc and webconsole, even if the bug could not be reproduced there. [#1372](https://github.com/BornToBeRoot/NETworkManager/issues/1372){:target="_blank"}

## Other
- Code refactoring, Cleanup, etc.
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="_blank"}
- Update OUI list [f2f6e77](https://github.com/BornToBeRoot/NETworkManager/commit/f2f6e77e2bae2fc30f6dcfe9e9ceeb759d2e2f70){:target="_blank"}
- Update Whois list [f2f6e77](https://github.com/BornToBeRoot/NETworkManager/commit/f2f6e77e2bae2fc30f6dcfe9e9ceeb759d2e2f70){:target="_blank"}
- Update Ports list [f2f6e77](https://github.com/BornToBeRoot/NETworkManager/commit/f2f6e77e2bae2fc30f6dcfe9e9ceeb759d2e2f70){:target="_blank"}
