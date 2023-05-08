---
layout: default
title: PowerShell
parent: Application
grand_parent: Documentation
nav_order: 10
description: "Documentation of PowerShell"
permalink: /Documentation/Application/PowerShell
---

# PowerShell



![PowerShell](10_PowerShell.png)

## Connect

## Profile

## Settings

### File path

Path to the PowerShell console.

**Type:** `String`

**Default:** `C:\Program Files\PowerShell\7\pwsh.exe`, `C:\Program Files (x86)\PowerShell\7\pwsh.exe` or `C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe`

**Possible values:**

- `C:\path\to\PowerShell.exe`
- `C:\path\to\pwsh.exe`

{: .note }
The `Configure` button opens the PowerShell console to configure it.

### Command

Default command to execute when the PowerShell console is started for local connections.

**Type:** `String`

**Default:** `Set-Location ~`

### Additional command line

Default additional command line arguments to pass to the PowerShell console when it is started.

**Type:** `String`

**Default:** `Empty`

### Execution policy

Default execution policy of the PowerShell console when it is started.

**Type:** `NETworkManager.Models.PowerShell.ExecutionPolicy`

**Default:** `RemoteSigned`

**Possible values:**

- `Restricted`
- `AllSigned`
- `RemoteSigned`
- `Unrestricted`
- `Bypass`
