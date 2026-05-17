<#
.SYNOPSIS
    Creates test computer accounts in Active Directory for the NETworkManager AD profile import feature.

.DESCRIPTION
    Creates a configurable number of computer accounts under a target OU with the dnsHostName
    attribute populated. Optionally creates the OU and disables every Nth account so the
    "Exclude disabled computer accounts" filter can be exercised.

    Requires the ActiveDirectory PowerShell module (RSAT) and write permissions to the target OU.

.PARAMETER OUPath
    Distinguished name of the OU (or container) under which the computers are created.
    Default: the domain's built-in Computers container (CN=Computers,<domain DN>).
    Example: "OU=NetworkManagerTest,DC=example,DC=com"

.PARAMETER Count
    Number of computer accounts to create. Default: 25.

.PARAMETER NamePrefix
    Name prefix for the computer accounts. Default: "NM-TEST-".

.PARAMETER DnsZone
    DNS zone appended to the computer name to build dnsHostName. Default: AD domain DNS root.

.PARAMETER CreateOU
    Create the target OU if it does not exist.

.PARAMETER DisableEvery
    Disable every Nth computer account (0 = never). Useful for testing the exclude-disabled filter.

.EXAMPLE
    .\Create-TestAdComputers.ps1 -OUPath "OU=NetworkManagerTest,DC=lab,DC=local" -Count 50 -CreateOU -DisableEvery 5

.EXAMPLE
    .\Create-TestAdComputers.ps1 -OUPath "OU=NM,DC=lab,DC=local" -Count 10 -DnsZone "lab.local"
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$OUPath,

    [int]$Count = 25,

    [string]$NamePrefix = "NM-TEST-",

    [string]$DnsZone,

    [switch]$CreateOU,

    [int]$DisableEvery = 0
)

if (-not (Get-Module -ListAvailable -Name ActiveDirectory)) {
    Write-Error "ActiveDirectory module not found. Install RSAT: 'Add-WindowsCapability -Online -Name Rsat.ActiveDirectory.DS-LDS.Tools~~~~0.0.1.0'"
    exit 1
}

Import-Module ActiveDirectory -ErrorAction Stop

$adDomain = $null
if (-not $DnsZone -or -not $OUPath) {
    $adDomain = Get-ADDomain
}

if (-not $DnsZone) {
    $DnsZone = $adDomain.DNSRoot
    Write-Verbose "Using DNS zone from current domain: $DnsZone"
}

if (-not $OUPath) {
    $OUPath = $adDomain.ComputersContainer
    Write-Verbose "Using default Computers container: $OUPath"
}

if (-not ([adsi]::Exists("LDAP://$OUPath"))) {
    if ($CreateOU) {
        $ouName = ($OUPath -split ',')[0] -replace '^OU=', ''
        $parentPath = ($OUPath -split ',', 2)[1]

        Write-Host "Creating OU '$ouName' under '$parentPath'..." -ForegroundColor Cyan
        if ($PSCmdlet.ShouldProcess($OUPath, "New-ADOrganizationalUnit")) {
            New-ADOrganizationalUnit -Name $ouName -Path $parentPath -ProtectedFromAccidentalDeletion $false
        }
    }
    else {
        Write-Error "OU '$OUPath' does not exist. Use -CreateOU to create it automatically."
        exit 1
    }
}

$created = 0
$disabled = 0
$skipped = 0

for ($i = 1; $i -le $Count; $i++) {
    $name = "{0}{1:D3}" -f $NamePrefix, $i
    $dnsHostName = "$($name.ToLower()).$DnsZone"
    $samName = "$name`$"

    if (Get-ADComputer -Filter "Name -eq '$name'" -SearchBase $OUPath -ErrorAction SilentlyContinue) {
        Write-Host "  [skip] $name already exists" -ForegroundColor DarkYellow
        $skipped++
        continue
    }

    if ($PSCmdlet.ShouldProcess($name, "New-ADComputer in $OUPath")) {
        try {
            New-ADComputer `
                -Name $name `
                -SAMAccountName $samName `
                -Path $OUPath `
                -DNSHostName $dnsHostName `
                -Description "NETworkManager AD import test ($(Get-Date -Format 'yyyy-MM-dd'))" `
                -Enabled $true `
                -ErrorAction Stop

            Write-Host "  [ok]   $name -> $dnsHostName" -ForegroundColor Green
            $created++

            if ($DisableEvery -gt 0 -and ($i % $DisableEvery -eq 0)) {
                Disable-ADAccount -Identity $samName
                Write-Host "         disabled (every $DisableEvery)" -ForegroundColor DarkGray
                $disabled++
            }
        }
        catch {
            Write-Warning "  [fail] $name : $($_.Exception.Message)"
        }
    }
}

Write-Host ""
Write-Host "Done. Created: $created | Disabled: $disabled | Skipped: $skipped" -ForegroundColor Cyan
Write-Host "Search Base for NETworkManager: $OUPath" -ForegroundColor Yellow