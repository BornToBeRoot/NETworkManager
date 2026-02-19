---
slug: system-wide-policies-for-enterprise-deployments
title: System-Wide Policies for Enterprise Deployments
authors: [borntoberoot]
tags: [policies, enterprise, settings, new feature]
---

NETworkManager now supports system-wide policies, giving administrators centralized control over application settings across all users on a machine. This feature is available since the [pre-release version 2026.02.19.0](https://github.com/BornToBeRoot/NETworkManager/releases) and can now be tested.

![System-wide policy indicator](./system-wide-policy-indicator.png)

<!-- truncate -->

## What Are System-Wide Policies?

System-wide policies allow administrators to enforce specific settings for all users on a machine. These policies override user-specific settings and provide centralized control over application behavior in enterprise environments.

When a policy is active, the corresponding setting is locked in the UI and displays a shield icon along with a message indicating that the setting is managed by an administrator. Users can see the enforced value but cannot change it.

## How to Configure Policies

Policies are defined in a `config.json` file placed in the application installation directory (the same folder as `NETworkManager.exe`). When this file exists, the application loads the policies at startup and applies them with precedence over user settings. An example file (`config.json.example`) is included in the application installation directory for reference.

**File location:**

- **Installed version**: `C:\Program Files\NETworkManager\config.json` (or your custom installation path)
- **Portable version**: Same directory as `NETworkManager.exe`

**File format:**

```json
{
  "Policy_Name": true
}
```

Property names follow the pattern `Section_SettingName`. You can find the available policy names and values in the corresponding [setting's documentation](https://borntoberoot.net/NETworkManager/docs/category/settings).

### Example Policy

For example, the [`Update_CheckForUpdatesAtStartup`](https://borntoberoot.net/NETworkManager/docs/settings/update) policy controls whether the application checks for new program versions on GitHub when the application is launched.

**Values:**

- `true` — Force enable automatic update checks at startup for all users
- `false` — Force disable automatic update checks at startup for all users
- Omit the property — Allow users to control this setting themselves

**Example `config.json`:**

```json
{
  "Update_CheckForUpdatesAtStartup": false
}
```

:::note

- The file must be named exactly `config.json`
- The file must contain valid JSON syntax
- Changes to the file require restarting the application to take effect
- If the file doesn't exist or contains invalid JSON, it will be ignored and user settings will apply

:::

## Deploying Policies

1. **Create the configuration file** — Use the `config.json.example` as a template, rename it to `config.json`, and set your desired policy values.

2. **Deploy to installation directory** — Place the `config.json` file in the same directory as `NETworkManager.exe`. For MSI installations, this is typically `C:\Program Files\NETworkManager\`.

3. **Deploy methods:**
   - Group Policy — copy the file to the installation directory (use Group Policy preferences or a startup script)
   - Configuration management tools — SCCM/ConfigMgr, Microsoft Intune, Ansible, etc.
   - Scripts and deployment toolkits — PowerShell scripts, PSAppDeployToolkit
   - Manual deployment — hand-copy for small-scale rollouts

4. **Verification:**
   - Launch the application
   - Navigate to Settings > Update (e.g., "Check for updates at startup")
   - Verify the shield icon and the administrator message appear and that the control is disabled

:::warning

Ensure the `config.json` file has appropriate permissions so that regular users cannot modify it. On standard installations in `Program Files`, this is automatically enforced by Windows file permissions.

:::

## Request More Policies

Additional policy options will be added in future releases to provide more granular control over application behavior. If you have specific requirements for system-wide policies in your organization, please submit a feature request via the [GitHub issue tracker](https://github.com/BornToBeRoot/NETworkManager/issues/new/choose) to help us prioritize.

More information is available in the [official documentation](https://borntoberoot.net/NETworkManager/docs/system-wide-policies).

If you find any issues or have suggestions for improvement, please open an [issue on GitHub](https://github.com/BornToBeRoot/NETworkManager/issues).
