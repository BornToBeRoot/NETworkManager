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
It is recommended to backup the above files on a regular basis.

To restore the settings, close the application and copy the files from the backup to the above location.

:::

### Reset

Button to reset all application settings to their default values.

:::note

The application will be restarted afterwards.

Profiles are not reset.

:::
