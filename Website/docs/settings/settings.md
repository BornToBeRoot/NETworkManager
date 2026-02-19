---
sidebar_position: 10
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

<details className="alert alert--info">
<summary>System-Wide Policy</summary>

This setting can be controlled by administrators using a system-wide policy. See [System-Wide Policies](../system-wide-policies.md) for more information.

**Policy Property:** `SettingsFolderLocation`

**Values:**

- Absolute path (e.g., `C:\\Path\\To\\Settings`)
- Path with environment variables (e.g., `%UserProfile%\\NETworkManager\\Settings`)
- UNC path (e.g., `\\\\server\\share$\\NETworkManager\\Settings`)
- Omit the property to allow the default location logic to apply (portable vs. non-portable)

**Example:**

```json
{
  "SettingsFolderLocation": "%UserProfile%\\NETworkManager\\Settings"
}
```

</details>

:::note

**Recommendation**  
It is strongly recommended to regularly back up your settings files.

**Automatic backups**  
NETworkManager automatically creates a backup of the settings files before applying any changes. See [Create daily backup](#create-daily-backup) and [Maximum number of backups](#maximum-number-of-backups) for configuration options.

- Location: `Settings\Backups` subfolder
- Naming: timestamped (e.g. `yyyyMMddHHmmss_Settings.json`)
- Frequency: **once per day** at most (even if multiple changes occur)
- Retention: keeps the **10 most recent backups** (default)

**Restoring settings**

1. Completely close NETworkManager
2. Locate the desired backup in `Settings\Backups`
3. Copy the file(s) back to the original folder (overwriting existing files)
4. Restart the application

:::

### Create daily backup

Create a daily backup of the application settings before applying any changes.

**Type**: `Boolean`

**Default:** `Enabled`

:::note

Backups are stored in the `Settings\Backups` subfolder. See [Location](#location) for more details.

Backups are created at most once per day, even if multiple changes occur.

:::

### Maximum number of backups

Maximum number of backups to keep. Older backups will be deleted automatically once a new backup is created.

**Type:** `Integer` [Min `1`, Max `365`]

**Default:** `10`

### Reset

Button to reset all application settings to their default values.

:::note

The application will be restarted afterwards.

Profiles are not reset.

:::
