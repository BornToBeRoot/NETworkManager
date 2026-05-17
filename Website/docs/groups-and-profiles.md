---
sidebar_position: 4
description: "Organize hosts and networks in named groups and profiles with per-feature connection settings."
keywords:
  [
    NETworkManager,
    groups,
    profiles,
    profile management,
    encrypted profiles,
    host organization,
    network profiles,
  ]
---

# Groups and Profiles

**Groups and Profiles** let you organize hosts and networks for quick access across all NETworkManager features. Settings defined in a group apply to every profile in that group; settings defined on a profile override the group defaults. See [FAQ > Settings priority](/docs/faq/settings-priority) for more information about the priority order.

## Overview

Groups and profiles for all features are managed on the `Profiles` tab in `Settings`.

![Profiles - Overview](./img/profiles--overview.png)

### Context menu

Right-click a selected group to open the group context menu.

| Action     | Description                     |
| ---------- | ------------------------------- |
| **Edit**   | Open the group settings dialog. |
| **Delete** | Delete the selected group.      |

Right-click a selected profile to open the profile context menu.

| Action     | Description                            |
| ---------- | -------------------------------------- |
| **Edit**   | Open the profile settings dialog.      |
| **Copy**   | Create a copy of the selected profile. |
| **Delete** | Delete the selected profile.           |

### Keyboard shortcuts

| Key   | Action                                |
| ----- | ------------------------------------- |
| `F2`  | Edit the selected group or profile.   |
| `Del` | Delete the selected group or profile. |

Inside a feature, the profile view shows only the groups and profiles enabled for that feature.

![Profiles - Feature overview](./img/profiles--overview-feature.png)

:::note

The edit button for a group appears when hovering over the group header.

:::

### Context menu

Right-click a selected profile to open the profile context menu.

| Action     | Description                            |
| ---------- | -------------------------------------- |
| **Edit**   | Open the profile settings dialog.      |
| **Copy**   | Create a copy of the selected profile. |
| **Delete** | Delete the selected profile.           |

Right-click a group header to expand or collapse all groups.

| Action           | Description          |
| ---------------- | -------------------- |
| **Expand all**   | Expand all groups.   |
| **Collapse all** | Collapse all groups. |

### Keyboard shortcuts

| Key   | Action                       |
| ----- | ---------------------------- |
| `F2`  | Edit the selected profile.   |
| `Del` | Delete the selected profile. |

## Group

In the group settings you can define general settings and feature-specific settings.

![Profiles - Group settings](./img/profiles--group-settings.png)

## Profile

In the profile settings you can define general settings and feature-specific settings.

:::note

Profiles are only displayed in a specific feature if they have been enabled via the checkbox. The [`Profiles` tab in `Settings`](#overview) shows all profiles regardless.

Use **tags** to organize profiles and filter by them. For example, tag profiles as `prod` or `dns` to group related ones. You can filter by `any` or `all` tags.

:::

:::tip

Some settings, such as the `host`, can be inherited from the general settings in the feature-specific settings.

See also the profile section in the specific [feature documentation](./introduction) for more information.

:::

![Profiles - Profile settings](./img/profiles--profile-settings.png)
