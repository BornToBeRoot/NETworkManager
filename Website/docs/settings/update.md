---
sidebar_position: 8
---

# Update

### Check for updates at startup

Check for new program versions on GitHub when the application is launched.

**Type:** `Boolean`

**Default:** `Enabled`

:::info System-Wide Policy

This setting can be controlled by administrators using a system-wide policy. See [System-Wide Policies](../system-wide-policies.md) for more information on how to configure the `Update_CheckForUpdatesAtStartup` policy.

:::

:::note

The URL `https://api.github.com/` must be reachable to check for updates.

:::

### Check for pre-releases

Check for pre-releases when checking for updates.

**Type:** `Boolean`

**Default:** `Disabled`

### Experimental features

Enable experimental features that are not yet complete.

**Type:** `Boolean`

**Default:** `Disabled`

:::warning

These features may contain bugs, crash the application, or change until release.

:::
