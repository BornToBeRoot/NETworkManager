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

<details className="alert alert--info">
<summary>System-Wide Policy</summary>

This setting can be controlled by administrators using a system-wide policy. See [System-Wide Policies](../system-wide-policies.md) for more information.

**Policy Property:** `Profiles_FolderLocation`

**Values:**

- Absolute path (e.g., `C:\\Path\\To\\Profiles`)
- Path with environment variables (e.g., `%UserProfile%\\NETworkManager\\Profiles`)
- UNC path (e.g., `\\\\server\\share$\\NETworkManager\\Profiles`)
- Omit the property to allow the default location logic to apply (portable vs. non-portable)

**Example:**

```json
{
  "Profiles_FolderLocation": "%UserProfile%\\NETworkManager\\Profiles"
}
```

</details>

:::note

**Recommendation**  
It is strongly recommended to regularly back up your profile files.

**Automatic backups**  
NETworkManager automatically creates a backup of the profile file before applying any changes. See [Create daily backup](#create-daily-backup) and [Maximum number of backups](#maximum-number-of-backups) for configuration options.
- Location: `Profiles\Backups` subfolder (relative to the main configuration directory)
- Naming: timestamped (e.g. `yyyyMMddHHmmss_<profile>.json`)
- Frequency: **once per day** at most (even if multiple changes occur)
- Retention: keeps the **10 most recent backups** (default)

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

### Create daily backup

Create a daily backup of the profile file before applying any changes.

**Type**: `Boolean`

**Default:** `Enabled`

:::note

Backups are stored in the `Profiles\Backups` subfolder. See [Location](#location) for more details.

Backups are created at most once per day, even if multiple changes occur.

:::

### Maximum number of backups

Maximum number of backups to keep. Older backups will be deleted automatically once a new backup is created.

**Type:** `Integer` [Min `1`, Max `365`]

**Default:** `10`
