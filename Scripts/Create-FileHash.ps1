[CmdletBinding()]

param (
    [Parameter(Mandatory = $true)]
    [string]$Path
)

$Path = $Path.TrimEnd("\")

if(-not (Test-Path -Path $Path -PathType Container)) {
    Write-Error "Path does not exist or is not a directory: $Path"
    return
}

# Get current date as version
$Version = (Get-Date).ToString("yyyy.MM.dd") + ".0"

# Create SHA256 file hashes
foreach ($Hash in Get-ChildItem -Path $Path | Where-Object { $_.Name.EndsWith(".zip") -or $_.Name.EndsWith(".msi") } | Sort-Object -Descending | Get-FileHash) {
    "$($Hash.Algorithm) | $($Hash.Hash) | $([System.IO.Path]::GetFileName($Hash.Path))" | Out-File -FilePath "$Path\NETworkManager_$($Version)_Hash.txt" -Encoding utf8 -Append
}
