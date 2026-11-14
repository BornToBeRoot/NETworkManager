---
sidebar_position: 1
description: "Configure NETworkManager window behavior including minimize to system tray, close-to-tray, and startup visibility options."
keywords: [NETworkManager, window settings, system tray, minimize to tray, close behavior]
---

# Window

### Minimize main window instead of terminating the application

Minimize the main window instead of terminating the application when the close button is clicked.

**Type:** `Boolean`

**Default:** `Disabled`

### Minimize to tray instead of taskbar

Minimize the main window to the tray instead of the taskbar when the minimize (or close) button is clicked.

**Type:** `Boolean`

**Default:** `Disabled`

### Confirm close

Show a confirmation dialog when the close button is clicked.

**Type:** `Boolean`

**Default:** `Disabled`

### Multiple instances

Allow multiple instances of the application to be opened.

**Type:** `Boolean`

**Default:** `Disabled`

:::warning

Enabling this setting is not recommended. Multiple instances of the application share the same settings and profile files. The last instance to be closed may overwrite changes made by other instances.

:::

### Dynamic tab width

The tabs of the tools (e.g. **IP Scanner**, **Remote Desktop**, **PuTTY**) are as wide as their title, up to the [maximum tab width](#maximum-tab-width). Longer titles are shortened, the tooltip of the tab shows the full title. When disabled, all tabs have the same width.

**Type:** `Boolean`

**Default:** `Disabled`

### Maximum tab width

Maximum width of a tab if [Dynamic tab width](#dynamic-tab-width) is enabled.

**Type:** `Integer` [Min `150`, Max `1000`]

**Default:** `300`

### Always show tray icon

Always show the tray icon, even if the main window is visible.

**Type:** `Boolean`

**Default:** `Disabled`

### Show splash screen

Show the splash screen when the application is started.

**Type:** `Boolean`

**Default:** `Enabled`
