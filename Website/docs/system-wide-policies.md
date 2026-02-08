---
sidebar_position: 6
---

# System-Wide Policies

System-wide policies allow administrators to enforce specific settings for all users on a machine. These policies override user-specific settings and provide centralized control over application behavior in enterprise environments.

## Overview

Policies are defined in a `config.json` file placed in the application installation directory (the same folder as `NETworkManager.exe`). When this file exists, the application loads the policies at startup and applies them with precedence over user settings.

Users will see a visual indicator in the Settings UI when a setting is controlled by a system-wide policy, showing them the administrator-enforced value and preventing them from changing it.

![System-wide policy indicator](./img/system-wide-policy-indicator.png)

## Configuration File

The `config.json` file uses a simple JSON structure to define policy values. An example file (`config.json.example`) is included in the application installation directory for reference.

**File Location:**
- **Installed version**: `C:\Program Files\NETworkManager\config.json` (or your custom installation path)
- **Portable version**: Same directory as `NETworkManager.exe`

**File Format:**

```json
{
  "Update_CheckForUpdatesAtStartup": false
}
```

:::note

- The file must be named exactly `config.json` (case-sensitive)
- The file must contain valid JSON syntax
- Changes to the file require restarting the application to take effect
- If the file doesn't exist or contains invalid JSON, it will be ignored and user settings will apply

:::

## Available Policies

### Update_CheckForUpdatesAtStartup

Controls whether the application checks for updates at startup for all users.

**Type:** `Boolean` (true/false)

**Default:** Not set (users control this setting)

**Values:**
- `true` - Force enable automatic update checks at startup for all users
- `false` - Force disable automatic update checks at startup for all users
- Omit the property - Allow users to control this setting themselves

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

:::tip Use Case

This is particularly useful for enterprise deployments where you want to:
- Ensure consistent update check behavior across all users
- Prevent users from being prompted about updates when you manage updates centrally
- Enforce update checks to ensure users are notified of important security updates

:::

## User Experience

When a setting is controlled by a system-wide policy:

1. **Settings UI**: The toggle/control for the setting is disabled
2. **Visual Indicator**: An orange shield icon appears next to the setting
3. **Administrator Message**: The text "This setting is managed by your administrator" is displayed
4. **Value Display**: The UI shows the value set by the administrator (enabled or disabled)

This provides clear feedback to users about which settings are under administrative control and what values are being enforced.

## Deployment

For enterprise deployments:

1. **Create the configuration file**: 
   - Use the `config.json.example` as a template
   - Rename it to `config.json`
   - Set your desired policy values

2. **Deploy to installation directory**:
   - Place the `config.json` file in the same directory as `NETworkManager.exe`
   - For MSI installations, this is typically `C:\Program Files\NETworkManager\`
   - For portable installations, place it next to the executable

3. **Deploy methods**:
   - Group Policy (copy file to installation directory)
   - Configuration management tools (Ansible, SCCM, etc.)
   - MSI deployment scripts
   - Manual deployment for small-scale rollouts

4. **Verification**:
   - Launch the application
   - Navigate to Settings > Update
   - Verify the shield icon and administrator message appear
   - Confirm the toggle reflects the policy value and is disabled

:::warning

Ensure the `config.json` file has appropriate permissions so that regular users cannot modify it. On standard installations in `Program Files`, this is automatically enforced by Windows file permissions.

:::

## Troubleshooting

**Policy not being applied:**
- Verify the file is named exactly `config.json` (not `config.json.txt`)
- Check that the JSON syntax is valid (use a JSON validator)
- Confirm the file is in the correct directory (same as `NETworkManager.exe`)
- Restart the application after creating or modifying the file
- Check the application logs for any error messages related to policy loading

**Policy values not showing in UI:**
- Ensure the property name matches exactly: `Update_CheckForUpdatesAtStartup`
- Verify the value is a boolean (`true` or `false`), not a string (`"true"` or `"false"`)
- Check that there are no syntax errors in the JSON file

## Future Policies

Additional policy options will be added in future releases to provide more granular control over application behavior. If you have specific requirements for system-wide policies in your organization, please submit a feature request via the [GitHub issue tracker](https://github.com/BornToBeRoot/NETworkManager/issues/new/choose).
