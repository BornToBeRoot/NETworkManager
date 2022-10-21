---
layout: default
title: Appearance
parent: Settings
grand_parent: Documentation
nav_order: 3
description: "Documentation of the appearance settings"
permalink: /Documentation/Settings/Appearance
---

# Appearance

### Theme

Theme of the application which is based on [`MahApps.Metro themes`](https://mahapps.com/docs/themes/usage){:target="\_blank"}

**Type:** `String`

**Default:** `Dark`

**Possible values:**

- `Dark`
- `Light`

### Accent

Accent of the application which is based on [`MahApps.Metro themes`](https://mahapps.com/docs/themes/usage){:target="\_blank"}

**Type:** `String`

**Default:** `Lime`

**Possible values:**

- `Red`
- `Green`
- `Blue`
- `Purple`
- `Orange`
- `Lime`
- `Emerald`
- `Teal`
- `Cyan`
- `Cobalt`
- `Indigo`
- `Violet`
- `Pink`
- `Magenta`
- `Crimson`
- `Amber`
- `Yellow`
- `Brown`
- `Olive`
- `Steel`
- `Mauve`
- `Taupe`
- `Sienna`

> **NOTE:** If you add, change or delete a theme in the folder, you must restart the application for the changes to be applied.

### Use custom themes

Enables or disables the custom themes.

**Type:** `Boolean`

**Default:** `Disabled`

Custom themes can be placed in the `Themes` folder in the application folder (e.g. `C:\Program Files\NETworkManager\Themes`). The file name has the format `<THEME>.<ACCENT>.xaml`. For instructions on how to create custom themes, see the [MahApp.Metro documentation](https://mahapps.com/docs/themes/thememanager#creating-custom-themes){:target="\_blank"}.

> **NOTE:** Custom themes override the [`Accent`](#accent) and [`Theme`](#theme) settings.

### Apply theme to PowerShell console

Apply the current application theme to the PowerShell console global profile(s).

**Type:** `Boolean`

**Default:** `Disabled`

If enabled, the PowerShell console global profile(s) are modified in the registry under `Computer\HKEY_CURRENT_USER\Console` (`HKCU:\Console`). This adjusts the background of the console and some other settings so that the console window integrates better with the NETworkManager application.

The profiles are changed for the PowerShell consoles configured in the PowerShell and AWS Session Manager tools. Both Windows PowerShell and PowerShell 7 and later are supported.

Example paths under `HKCU:\Console`:

| PowerShell path                                             | Registry profile path                                         |
| ----------------------------------------------------------- | ------------------------------------------------------------- |
| `C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe` | `%SystemRoot%_System32_WindowsPowerShell_v1.0_powershell.exe` |
| `C:\Program Files\PowerShell\7\pwsh.exe`                    | `C:_Program Files_PowerShell_7_pwsh.exe`                      |

The following registry keys are created, modified or deleted in the registry profile path:

| Name        | Type         | Action              | Value                               | Value basis |
| ----------- | ------------ | ------------------- | ----------------------------------- | ----------- |
| `REG_DWORD` | `Add/Modify` | `CursorType`        | `1`                                 | `Hex`       |
| `REG_DWORD` | `Add/Modify` | `FontFamiliy`       | `36`                                | `Hex`       |
| `REG_DWORD` | `Add/Modify` | `FontSize`          | `120000`                            | `Hex`       |
| `REG_DWORD` | `Add/Modify` | `FontWeight`        | `400`                               | `Hex`       |
| `REG_DWORD` | `Add/Modify` | `DefaultBackground` | `252525` (Dark) or `FFFFFF` (Light) | `Hex`       |
| `REG_DWORD` | `Add/Modify` | `ColorTable00`      | `252525` (Dark) or `FFFFFF` (Light) | `Hex`       |
| `REG_DWORD` | `Add/Modify` | `ColorTable07`      | `252525` (Dark) or `252525` (Light) | `Hex`       |
| `REG_SZ`    | `Add/Modify` | `FaceName`          | `Consolas`                          | `String`    |
| `REG_DWORD` | `Delete`     | `ScreenColors`      |                                     |             |

If disabled, the Powershell console global profile(s) are no longer modified. But the original values are not restored.

> **NOTE:** Colors may not be set correctly when `Use custom themes`(#use-custom-themes) is enabled.
