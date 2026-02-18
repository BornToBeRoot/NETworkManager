---
sidebar_position: 8
---

# Update

### Check for updates at startup

Check for new program versions on GitHub when the application is launched.

**Type:** `Boolean`

**Default:** `Enabled`

:::info System-Wide Policy

<details>
<summary>Click to expand</summary>

This setting can be controlled by administrators using a system-wide policy. See [System-Wide Policies](../system-wide-policies.md) for more information.

**Policy Property:** `Update_CheckForUpdatesAtStartup`

**Values:**

- `true` - Force enable automatic update checks at startup for all users
- `false` - Force disable automatic update checks at startup for all users
- Omit the property - Allow users to control this setting themselves

**Example:**

```json
{
  "Update_CheckForUpdatesAtStartup": false
}
```

</details>

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
