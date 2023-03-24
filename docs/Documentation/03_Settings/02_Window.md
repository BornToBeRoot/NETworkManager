---
layout: default
title: Window
parent: Settings
grand_parent: Documentation
nav_order: 2
description: "Documentation of the window settings"
permalink: /Documentation/Settings/Window
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

{. .warning}
Enabling this setting is not recommended. Multiple instances of the application share the same settings and profile files. The last instance to be closed may overwrite changes made by other instances.

### Always show tray icon

Always show the tray icon, even if the main window is visible.

**Type:** `Boolean`

**Default:** `Disabled`

### Show splash screen

Show the splash screen when the application is started.

**Type:** `Boolean`

**Default:** `Enabled`
