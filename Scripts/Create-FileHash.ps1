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

# Get current date as version
$Now = Get-Date
$Version = "$($Now.Year).$($Now.Month).$($Now.Day).0"

# Create SHA256 file hashes
foreach ($Hash in Get-ChildItem -Path $Path | Where-Object { $_.Name.StartsWith("NETworkManager_") -and ($_.Name.EndsWith(".zip") -or $_.Name.EndsWith(".msi")) } | Sort-Object -Descending | Get-FileHash) {
    "$($Hash.Hash)  $([System.IO.Path]::GetFileName($Hash.Path))" | Out-File -FilePath "$Path\NETworkManager_$($Version)_SHA256SUMS" -Encoding utf8 -Append
}

$SumFile = Join-Path $Path "NETworkManager_$($Version)_SHA256SUMS"

if (Test-Path $SumFile) { 
    Remove-Item $SumFile 
}

Get-ChildItem -Path $Path | 
Where-Object { $_.Name.StartsWith("NETworkManager_") -and ($_.Name.EndsWith(".zip") -or $_.Name.EndsWith(".msi")) } |
Sort-Object -Descending |
Get-FileHash -Algorithm SHA256 |
ForEach-Object {
    # Format: <hash><two spaces><filename> + LF ending
    $Line = "$($_.Hash)  $([System.IO.Path]::GetFileName($_.Path))"
    # Use UTF-8 without BOM and LF line endings
    [System.IO.File]::AppendAllText($SumFile, "$Line`n", [System.Text.UTF8Encoding]::new($false))
}
