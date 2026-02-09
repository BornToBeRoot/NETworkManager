---
slug: system-wide-policies
title: System-Wide Policies for Enterprise Deployments
authors: [borntoberoot]
tags: [policies, enterprise, settings, new feature]
---

NETworkManager now supports system-wide policies, giving administrators centralized control over application settings across all users on a machine. This is especially useful in enterprise environments where consistent configuration and security standards need to be enforced.

![System-wide policy indicator](./system-wide-policy-indicator.png)

<!-- truncate -->

## What Are System-Wide Policies?

System-wide policies allow administrators to enforce specific application settings for all users on a machine. When a policy is active, the corresponding setting is locked in the UI and displays a shield icon along with a message indicating that the setting is managed by an administrator. Users can see the enforced value but cannot change it.

This ensures that critical settings — such as whether the application checks for updates at startup — remain consistent and tamper-proof across your organization.

## How to Configure Policies

Policies are defined in a simple `config.json` file placed in the same directory as `NETworkManager.exe`.

**File location:**

- **Installed version**: `C:\Program Files\NETworkManager\config.json`
- **Portable version**: Same directory as `NETworkManager.exe`

**Example `config.json`:**

```json
{
  "Update_CheckForUpdatesAtStartup": false
}
```

A `config.json.example` file is included in the application directory for reference. Simply rename it to `config.json` and set your desired policy values. See the [system-wide policies documentation](https://borntoberoot.net/NETworkManager/docs/system-wide-policies) for more details on the configuration file format and deployment.

:::note

The file must be named exactly `config.json`, contain valid JSON, and the application must be restarted for changes to take effect.

:::

## Deploying Policies

You can deploy the `config.json` file using your preferred method:

- **Group Policy** — Use Group Policy preferences or a startup script to copy the file to the installation directory.
- **Configuration management** — Deploy via SCCM/ConfigMgr, Microsoft Intune, Ansible, or similar tools.
- **Scripts** — Use PowerShell scripts or PSAppDeployToolkit for scripted deployments.
- **Manual** — Hand-copy the file for small-scale rollouts.

:::warning

Ensure the `config.json` file has appropriate permissions so that regular users cannot modify it. On standard installations in `Program Files`, this is automatically enforced by Windows file permissions.

:::

## Request More Policies

Additional policy options will be added in future releases. If you have specific requirements for system-wide policies in your organization, please submit a feature request via the [GitHub issue tracker](https://github.com/BornToBeRoot/NETworkManager/issues/new/choose). Your feedback helps prioritize which settings to add next.

More information is available in the [official documentation](https://borntoberoot.net/NETworkManager/docs/system-wide-policies).

If you find any issues or have suggestions for improvement, please open an [issue on GitHub](https://github.com/BornToBeRoot/NETworkManager/issues).
