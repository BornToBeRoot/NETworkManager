---
sidebar_position: 9
---

# PowerShell

With **PowerShell** you can launch PowerShell consoles locally or connect to remote computers using [PowerShell Remoting](https://learn.microsoft.com/en-us/powershell/scripting/learn/ps101/08-powershell-remoting). You can also execute command-line applications such as `wsl`, `k9s`, or any other tools typically accessible from a PowerShell session. In addition, you can run PowerShell scripts directly within the console.

The integration of PowerShell with NETworkManger supports tabs and profiles for hosts (and tools). You can launch the console / establish a connection via a profile (double-click, Enter key or right-click `Connect`) or directly via the [connection](#connect) dialog.

:::info

PowerShell is a command-line shell and scripting language developed by Microsoft for automating administrative tasks and managing system configurations. It provides a robust set of built-in commands and access to .NET Framework objects, allowing for efficient system administration and automation. PowerShell uses a verb-noun syntax, allowing users to perform a wide range of operations by executing simple and powerful commands.

:::

:::note

Windows PowerShell and PowerShell are supported. Indructions for installing PowerShell Core can be found on the [official website](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell?).

:::

:::tip

Launching a command-line application such as `wsl` or `k9s` can be done by passing the [command](#command) to the PowerShell console.

```powershell
# Connect to WSL
wsl -d <DISTRIBUTION>

# Connect to Kubernetes
k9s --readonly
```

See [FAQ > PowerShell with command-line applications](../faq/powershell-cmd-apps) for more information.

:::

:::warning

If Windows Terminal is installed, change the `Default terminal application` in the settings of Windows Terminal to `Windows Console Host` instead of `Windows Terminal` to use PowerShell in NETworkManager.

:::

![PowerShell](../img/powershell.png)

:::note

Right-click on the tab will open the context menu with the following options:

- **Reconnect** - Restart the PowerShell console (and reconnect to the remote computer).
- **Resize** - Resize the PowerShell console to the current view size (if connected).

:::

## Connect

### Remote console

Connect to a remote computer via PowerShell Remoting.

**Type:** `Boolean`

**Default:** `False`

### Host

Host name or IP address of the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.borntoberoot.net`
- `10.0.0.10`

:::note

Only available if [Remote console](#remote-console) is enabled.

:::

### Command

Command to execute when the PowerShell console is started locally.

**Type:** `String`

**Default:** [`Settings > Command`](#command-3)

**Example:** `Set-Location ~; Get-ChildItem`

:::note

Only available if [Remote console](#remote-console) is disabled.

:::

:::tip

Use `wsl -d <DISTRIBUTION>` to connect to Windows Subsystem for Linux.

:::

### Additional command line

Additional command line arguments to pass to the PowerShell console when it is started.

**Type:** `String`

**Default:** [`Settings > Additional command line`](#additional-command-line-3)

### Execution policy

Execution policy of the PowerShell console when it is started.

**Type:** `NETworkManager.Models.PowerShell.ExecutionPolicy`

**Default:** [`Settings > Execution policy`](#execution-policy-3)

**Possible values:**

- `Restricted`
- `AllSigned`
- `RemoteSigned`
- `Unrestricted`
- `Bypass`

## Profile

### Remote console

Connect to a remote computer via PowerShell Remoting.

**Type:** `Boolean`

**Default:** `Enabled`

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

If this option is enabled, the [Host](#host-1) is overwritten by the host from the general settings and the [Host](#host-1) is disabled.

:::

### Host

Host name or IP address of the remote computer.

**Type:** `String`

**Example:**

- `server-01.borntoberoot.net`
- `10.0.0.10`

:::note

Only available if [Remote console](#remote-console-1) is enabled.

:::

### Command

Command to execute when the PowerShell console is started locally.

**Type:** `String`

**Default:** `Empty`

**Example:** `Set-Location ~; Get-ChildItem`

:::note

Only available if [Remote console](#remote-console-1) is disabled.

:::

:::tip

Use `wsl -d <DISTRIBUTION>` to connect to Windows Subsystem for Linux.

Use `k9s` or `k9s --readonly` to manage Kubernetes clusters.

:::

### Additional command line

Additional command line arguments to pass to the PowerShell console when it is started.

**Type:** `String`

**Default:** `Empty`

### Execution policy

Execution policy of the PowerShell console when it is started.

**Type:** `NETworkManager.Models.PowerShell.ExecutionPolicy`

**Default:** `RemoteSigned`

**Possible values:**

- `Restricted`
- `AllSigned`
- `RemoteSigned`
- `Unrestricted`
- `Bypass`

## Group

### Command

Command to execute when the PowerShell console is started locally.

**Type:** `String`

**Default:** `Empty`

**Example:** `Set-Location ~; Get-ChildItem`

:::tip

Use `wsl -d <DISTRIBUTION>` to connect to Windows Subsystem for Linux.

Use `k9s` or `k9s --readonly` to manage Kubernetes clusters.

:::

### Additional command line

Additional command line arguments to pass to the PowerShell console when it is started.

**Type:** `String`

**Default:** `Empty`

### Execution policy

Execution policy of the PowerShell console when it is started.

**Type:** `NETworkManager.Models.PowerShell.ExecutionPolicy`

**Default:** `RemoteSigned`

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

**Default:** `%ProgramFiles%\PowerShell\7\pwsh.exe`, `%ProgramFiles(x86)%\PowerShell\7\pwsh.exe` or `%windir%\System32\WindowsPowerShell\v1.0\powershell.exe`

**Example:**

- `C:\Program Files\PowerShell\7\pwsh.exe`

:::note

The `Configure` button opens the PowerShell console to configure it.

:::

### Command

Default command to execute when the PowerShell console is started locally.

**Type:** `String`

**Default:** `Set-Location ~`

**Example:** `Set-Location ~; Get-ChildItem`

:::tip

Use `wsl -d <DISTRIBUTION>` to connect to Windows Subsystem for Linux.

Use `k9s` or `k9s --readonly` to manage Kubernetes clusters.

:::

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
