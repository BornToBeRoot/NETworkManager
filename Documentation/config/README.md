# System-Wide Configuration

This directory contains an example `config.json` file that can be placed in the application installation directory to override user settings for all users on a machine.

## Usage

1. Copy the example configuration file to the NETworkManager installation directory (where `NETworkManager.exe` is located)
2. Rename `config.json.example` to `config.json`
3. Edit the settings as needed
4. Restart NETworkManager

## Configuration Options

### Update_DisableUpdateCheck

When set to `true`, disables the automatic update check at startup for all users.

**Example:**
```json
{
  "Update_DisableUpdateCheck": true
}
```

This is useful for enterprise deployments where you want to:
- Control software updates centrally
- Prevent users from being prompted about updates
- Disable update checks on multiple machines without user intervention

## File Location

The `config.json` file should be placed in:
- **Installed version**: `C:\Program Files\NETworkManager\` (or your custom installation path)
- **Portable version**: Same directory as `NETworkManager.exe`

## Notes

- System-wide configuration takes precedence over user settings
- If the file doesn't exist or contains invalid JSON, it will be ignored and default user settings will apply
- Changes to `config.json` require restarting the application to take effect
- The file is optional - if not present, user settings will be used as normal
