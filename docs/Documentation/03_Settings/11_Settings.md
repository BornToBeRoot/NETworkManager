---
layout: default
title: Settings
parent: Settings
grand_parent: Documentation
nav_order: 10
description: "Documentation of the settings settings"
permalink: /Documentation/Settings/Settings
---

# Settings

### Location

Folder where the application settings are stored.

**Type**: `String`

**Default**:

| Version        | Path                                              |
| -------------- | ------------------------------------------------- |
| Setup / Archiv | `%UserProfile%\Documents\NETworkManager\Settings` |
| Portable       | `<APP_FOLDER>\Settings`                           |

{: .note}
It is recommended to backup the above files on a regular basis.

{: .note}
To restore the settings, close the application and copy the files from the backup to the above location.

### Reset

Button to reset all application settings to their default values.

{: .note}
The application will be restarted afterwards.

{: .note}
Profiles are not reset.
