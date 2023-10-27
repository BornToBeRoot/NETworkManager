---
layout: default
title: File locations
parent: FAQ
description: "FAQ > File locations"
permalink: /FAQ/FileLocations
---

# File locations

## Where are files stored?

The setup installs the application by default in the following path: `%ProgramFiles%\NETworkManager`

You can run the archive and portable version from anywhere.

Profiles, settings and themes are stored in the following folders:

| File(s)  | Setup or Archiv                                     | Portable                  |
| -------- | --------------------------------------------------- | ------------------------- |
| Profiles | `%UserProfile%\Documents\NETworkManager\Profiles\*` | `<APP_FOLDER>\Profiles\*` |
| Settings | `%UserProfile%\Documents\NETworkManager\Settings\*` | `<APP_FOLDER>\Settings\*` |
| Themes   | `<APP_FOLDER>\Themes\*`                             | `<APP_FOLDER>\Themes\*`   |

{: .note }
It is recommended to backup the above files on a regular basis. You can also roam the files with a cloud service like OneDrive or Nextcloud to use them on multiple devices.

In addition, some files and settings, as well as the cache, are stored in the following locations:

| File(s)             | Setup, Archiv and Portable                                           |
| ------------------- | -------------------------------------------------------------------- |
| Local settings      | `%LocalAppData%\NETworkManager\NETworkManager_Url_<RANDOM_STRING>\*` |
| Log                 | `%LocalAppData%\NETworkManager\NETworkManager.log`                   |
| PowerShell profiles | `HKCU:\Console\<PATH_OF_CONSOLE>`                                    |
| PuTTY log           | `%LocalAppData%\NETworkManager\PuTTY_Log\*`                          |
| PuTTY profile       | `HKCU:\Software\SimonTatham\PuTTY\Sessions\NETworkManager`           |
| WebConsole cache    | `%LocalAppData%\NETworkManager\WebConsole_Cache\*`                   |
