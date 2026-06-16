# Active Directory Test Environment

Helper scripts for setting up Active Directory test data for NETworkManager.

## Scripts

### `Create-TestAdComputers.ps1`

Creates computer accounts in Active Directory to test NETworkManager's **AD profile import** feature (import of computer accounts as connection profiles).

#### Requirements

- Windows with the **ActiveDirectory** PowerShell module (RSAT)
- Domain-joined machine with write permissions to the target OU

Install RSAT if missing:

```powershell
Add-WindowsCapability -Online -Name Rsat.ActiveDirectory.DS-LDS.Tools~~~~0.0.1.0
```

#### Parameters

| Parameter       | Default                               | Description                                                                  |
| --------------- | ------------------------------------- | ---------------------------------------------------------------------------- |
| `-OUPath`       | Domain's built-in Computers container | Distinguished name of the target OU                                          |
| `-Count`        | `25`                                  | Number of computer accounts to create                                        |
| `-NamePrefix`   | `NM-TEST-`                            | Name prefix for computer accounts                                            |
| `-DnsZone`      | AD domain DNS root                    | DNS zone appended to the computer name to build `dnsHostName`                |
| `-CreateOU`     | _(switch)_                            | Create the target OU if it does not exist                                    |
| `-DisableEvery` | `0` _(never)_                         | Disable every Nth account — useful for testing the "exclude disabled" filter |

#### Examples

Create 25 computers in the default Computers container:

```powershell
.\Create-TestAdComputers.ps1
```

Create 50 computers in a custom OU (creates the OU if missing), disable every 5th:

```powershell
.\Create-TestAdComputers.ps1 `
    -OUPath "OU=NetworkManagerTest,DC=lab,DC=local" `
    -Count 50 `
    -CreateOU `
    -DisableEvery 5
```

Preview changes without applying them (`-WhatIf`):

```powershell
.\Create-TestAdComputers.ps1 -OUPath "OU=NM,DC=lab,DC=local" -Count 10 -WhatIf
```

#### Idempotency

The script skips accounts that already exist and reports them as `[skip]`. Re-running it is safe.
