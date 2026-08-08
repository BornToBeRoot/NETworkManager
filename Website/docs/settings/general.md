---
sidebar_position: 0
description: "Configure which applications appear in the NETworkManager sidebar and adjust general application behavior."
keywords: [NETworkManager, general settings, sidebar configuration, application settings]
---

# General

### Applications

Applications that are displayed in the main window in the sidebar.

Type: `NETworkManager.Models.ApplicationInfo`

Default: `All`

:::note

Applications can be sorted via drag & drop.

Right-click on an application opens a context menu with the following options:

- `Set default` (Set the default application that is launched on startup - available if not set)
- `Show` (Shows the application in the main window - available if hidden)
- `Hide` (Hides the application from the main window - available if shown)

:::

### Run background job every x-minutes

Run a background job every x-minutes to save profiles and settings.

Type: `Integer`

Default: `5` [Min `0`, Max `120`]

:::note

The value 0 will disable the background job. Changes to this value will take effect after restarting the application.

:::

### Number of stored entries

Maximum number of entries stored in the history for several application inputs.

Type: `Integer`

Default: `5` [Min `0`, Max `25`]
