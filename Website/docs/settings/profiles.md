---
sidebar_position: 9
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

:::note

It is recommended to backup the above files on a regular basis.

To restore the profiles, close the application and copy the files from the backup to the above location.

:::

### Profile files

List of profile files.

**Type**: `List<NETworkManager.Profiles.ProfileFileInfo>`

**Default**: `[Default]`

:::note

Right-click on a profile file to `edit`, `encrypt/decrypt` or `delete` it.

You can also use the Hotkeys `F2` (`edit`) or `Del` (`delete`) on a selected profile file.

Profile files can be encrypted with a master password. See [FAQ > How to enable profile file encryption?](https://borntoberoot.net/NETworkManager/docs/faq/profile-file-encryption#how-to-enable-profile-file-encryption) for more details.

At least one profile is required and must exist.

:::
