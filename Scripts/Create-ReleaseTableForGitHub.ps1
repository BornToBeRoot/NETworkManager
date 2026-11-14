[CmdletBinding()]

param (
    [Parameter(Mandatory = $true)]
    [string]$Path
)

$Path = $Path.TrimEnd("\")

if (-not (Test-Path -Path $Path -PathType Container)) {
    Write-Error "Path does not exist or is not a directory: $Path"
    return
}

$RepoUrl = "https://github.com/BornToBeRoot/NETworkManager"

# Get current date as version
$Now = Get-Date
$Version = "$($Now.Year).$($Now.Month).$($Now.Day).0"

$SumFile = Join-Path $Path "NETworkManager_$($Version)_SHA256SUMS"

if (-not (Test-Path $SumFile)) {
    Write-Error "SHA256SUMS file not found: $SumFile (run Create-FileHash.ps1 first)"
    return
}

Write-Output "| File | Checksum [SHA256] |"
Write-Output "|---|---|"

Get-Content -Path $SumFile | ForEach-Object {
    # Line format: <hash><two spaces><filename>
    $Hash, $FileName = $_ -split "  ", 2
    $DownloadUrl = "$RepoUrl/releases/download/$Version/$FileName"

    # Format: |[`<filename>`](<download url>)| `<hash>` |
    Write-Output "|[``$FileName``]($DownloadUrl)| ``$Hash`` |"
}
