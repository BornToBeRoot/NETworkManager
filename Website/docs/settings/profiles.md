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

**Recommendation**  
It is strongly recommended to regularly back up your profile files.

**Automatic backups**  
NETworkManager automatically creates a backup of the profile file before applying any changes.
- Location: `Profiles\Backups` subfolder (relative to the main configuration directory)
- Naming: timestamped (e.g. `yyyyMMddHHmmss_<profile>.json`)
- Frequency: **once per day** at most (even if multiple changes occur)
- Retention: keeps the **10 most recent backups**

**Restoring profiles**  
1. Completely close NETworkManager
2. Locate the desired backup in `Profiles\Backups`
3. Copy the file(s) back to the original folder (overwrite existing files or copy them under a different name)
4. Restart the application

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
