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

:::note

**Recommendation**  
It is strongly recommended to regularly back up your settings files.

**Automatic backups**  
NETworkManager automatically creates a backup of the settings files before applying any changes.
- Location: `Settings\Backups` subfolder
- Naming: timestamped (e.g. `yyyyMMddHHmmss_Settings.json`)
- Frequency: **once per day** at most (even if multiple changes occur)
- Retention: keeps the **10 most recent backups**

**Restoring settings**  
1. Completely close NETworkManager
2. Locate the desired backup in `Settings\Backups`
3. Copy the file(s) back to the original folder (overwriting existing files)
4. Restart the application

:::

### Reset

Button to reset all application settings to their default values.

:::note

The application will be restarted afterwards.

Profiles are not reset.

:::
