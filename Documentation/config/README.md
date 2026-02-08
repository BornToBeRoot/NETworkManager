# System-Wide Configuration

This directory contains an example `config.json` file that can be placed in the application installation directory to override user settings for all users on a machine.

## Usage

1. Copy the example configuration file to the NETworkManager installation directory (where `NETworkManager.exe` is located)
2. Rename `config.json.example` to `config.json`
3. Edit the settings as needed
4. Restart NETworkManager

## Configuration Options

### Update_CheckForUpdatesAtStartup

Controls whether the application checks for updates at startup for all users. This overrides the user's personal setting.

- Set to `true` to enable automatic update checks at startup
- Set to `false` to disable automatic update checks at startup
- Omit the property to allow users to control this setting themselves

**Example (disable updates):**
```json
{
  "Update_CheckForUpdatesAtStartup": false
}
```

**Example (enable updates):**
```json
{
  "Update_CheckForUpdatesAtStartup": true
}
```

This is useful for enterprise deployments where you want to:
- Control software update checking behavior centrally
- Ensure consistent update check behavior across all users
- Either enforce update checks or prevent them system-wide

## File Location

The `config.json` file should be placed in:
- **Installed version**: `C:\Program Files\NETworkManager\` (or your custom installation path)
- **Portable version**: Same directory as `NETworkManager.exe`

## Notes

- System-wide configuration takes precedence over user settings
- Users will see a message in the UI indicating when a setting is managed by the administrator
- If the file doesn't exist or contains invalid JSON, it will be ignored and default user settings will apply
- Changes to `config.json` require restarting the application to take effect
- The file is optional - if not present, user settings will be used as normal
