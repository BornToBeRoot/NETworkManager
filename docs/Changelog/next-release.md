---
layout: default
title: Next release
parent: Changelog
nav_order: 1
description: "Changelog for next release"
permalink: /Changelog/next-release
---

# Next release

**New system requirements**
- Windows 10 / Server x64 (1809 or later)
- [.NET Desktop Runtime 6.x (LTS)](https://dotnet.microsoft.com/download/dotnet/6.0){:target="_blank"}
- [Microsoft Edge - WebView2 Runtime (Evergreen)](https://developer.microsoft.com/en-us/microsoft-edge/webview2/){:target="_blank"}

## What's new?
- New Icon/Logo for the application [#1362](https://github.com/BornToBeRoot/NETworkManager/pull/1362){:target="_blank"} [#1371](https://github.com/BornToBeRoot/NETworkManager/pull/1371){:target="_blank"}
- PuTTY 
  - Custom profile `NETworkManager` will be added to the registry (`HCKU\Software\SimonTatham\PuTTY\Sessions\NETworkManager`) which will set the PuTTY background to the application background [#1358](https://github.com/BornToBeRoot/NETworkManager/pull/1358){:target="_blank"}
  - Add context menu button (right click on the tab) to fix the PuTTY embedded window size [#1366](https://github.com/BornToBeRoot/NETworkManager/pull/1366){:target="_blank"}
- Light theme temporarily removed because it is causing problems and needs to be reworked so that the font is more readable [#1358](https://github.com/BornToBeRoot/NETworkManager/pull/1358){:target="_blank"}

## Improvements
- Min Window size reduced to 800x600 [#1366](https://github.com/BornToBeRoot/NETworkManager/pull/1366){:target="_blank"}
- Default history entries increased from 5 to 10 [#1372](https://github.com/BornToBeRoot/NETworkManager/issues/1372){:target="_blank"}

## Bugfixes
- The Min/Max/Close button on the pulled out window is visible again [#1366](https://github.com/BornToBeRoot/NETworkManager/pull/1366){:target="_blank"}
- App crash fixed when renaming a profile file [#1318](https://github.com/BornToBeRoot/NETworkManager/issues/1318){:target="_blank"}
- Fixed that some arguments from the connect dialog of PuTTY (like privatekey) were not applied correctly. Fix was also applied to remote desktop, powershell, tigervnc and webconsole, even if the bug could not be reproduced there. [#1372](https://github.com/BornToBeRoot/NETworkManager/issues/1372){:target="_blank"}

## Other
- Language files updated [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration){:target="_blank"}
- Dependencies updated [#dependencies](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot){:target="_blank"}
