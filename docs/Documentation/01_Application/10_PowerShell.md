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

With **PowerShell** you can start PowerShell consoles on the local computer or connect to remote computers via [PowerShell Remoting](https://learn.microsoft.com/en-us/powershell/scripting/learn/ps101/08-powershell-remoting){:target="\_blank"}. The integration of PowerShell with NETworkManger supports tabs and profiles for hosts.

{: .note}
Windows PowerShell and PowerShell Core are supported. Indructions for installing PowerShell Core can be found on the [official website](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell?){:target="\_blank"}.

![PowerShell](10_PowerShell.png)

## Connect

### Remote console

Connect to a remote computer via PowerShell Remoting.

**Type:** `Boolean`

**Default:** `False`

### Host

Host name or IP address of the remote computer.

**Type:** `String`

**Example:**

- `server-01.example.com`
- `10.0.0.10`

{: .note }
Only available if [Remote console](#remote-console) is enabled.

### Command

Command to execute when the PowerShell console is started locally.

**Type:** `String`

{: .note }
Only available if [Remote console](#remote-console) is disabled.

### Additional command line

Additional command line arguments to pass to the PowerShell console when it is started.

**Type:** `String`

### Execution policy

Execution policy of the PowerShell console when it is started.

**Type:** `NETworkManager.Models.PowerShell.ExecutionPolicy`

**Possible values:**

- `Restricted`
- `AllSigned`
- `RemoteSigned`
- `Unrestricted`
- `Bypass`

## Profile

## Group

### Command

Command to execute when the PowerShell console is started locally.

**Type:** `String`

### Additional command line

Additional command line arguments to pass to the PowerShell console when it is started.

**Type:** `String`

### Execution policy

Execution policy of the PowerShell console when it is started.

**Type:** `NETworkManager.Models.PowerShell.ExecutionPolicy`

**Possible values:**

- `Restricted`
- `AllSigned`
- `RemoteSigned`
- `Unrestricted`
- `Bypass`

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

Default command to execute when the PowerShell console is started locally.

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
