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

Maximum number of entries stored in the history for several application inputs. With `0`, no entries are stored (existing entries are removed with the next input).

Type: `Integer`

Default: `5` [Min `0`, Max `25`]

### Number of stored entries (Run command)

Maximum number of commands stored in the history of the [run command](../run-command.md#suggestions). With `0`, no commands are stored.

Type: `Integer`

Default: `20` [Min `0`, Max `50`]

### Suggest the clipboard content as target

Suggest the commands for a target in the clipboard (e.g. a copied IP address, subnet or MAC address) when the [run command](../run-command.md#target-without-a-command) is opened. `Enter` runs the first one. While typing a command, the commands for the clipboard that start with it are suggested (e.g. `pi` → `ping 10.0.0.1`). A single word (host without a domain) is not suggested. The clipboard is only read when the run command is opened and is not stored.

Type: `Boolean`

Default: `Disabled`

### Show key hints

Show the keys (e.g. `Tab`, `Ctrl+Enter`) at the bottom of the [run command](../run-command.md#suggestions).

Type: `Boolean`

Default: `Enabled`
