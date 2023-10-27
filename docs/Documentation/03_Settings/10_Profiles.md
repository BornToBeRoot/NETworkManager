---
layout: default
title: Profiles
parent: Settings
grand_parent: Documentation
nav_order: 9
description: "Documentation of the profile settings"
permalink: /Documentation/Settings/Profiles
---

# Profiles

### Location

Folder where the application profiles are stored.

**Type**: `String`

**Default**:

| Version        | Path                                              |
| -------------- | ------------------------------------------------- |
| Setup / Archiv | `%UserProfile%\Documents\NETworkManager\Profiles` |
| Portable       | `<APP_FOLDER>\Profiles`                           |

{: .note}
It is recommended to backup the above files on a regular basis.

{: .note}
To restore the profiles, close the application and copy the files from the backup to the above location.

### Profiles

List of profile files.

**Type**: `List<NETworkManager.Profiles.ProfileFileInfo>`

**Default**: `[Default]`

{: .note}
Profile files can be encrypted with a master password. Right click on a profile and select `Encryption... > Enable encryption...`. See [FAQ > How to enable profile file encryption?](https://borntoberoot.net/NETworkManager/FAQ/ProfileFileEncryption) for more details.

{: .note}
At least one profile is required and must exist.
