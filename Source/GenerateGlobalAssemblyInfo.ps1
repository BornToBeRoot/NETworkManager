param(
    [string]$TemplatePath,
    [string]$OutputPath,
    [string]$Year
)

# Create a mutex for thread-safe file generation across parallel builds
$mutexName = "Global\NETworkManager_GlobalAssemblyInfo_Mutex"
$mutex = New-Object System.Threading.Mutex($false, $mutexName)

try {
    # Wait up to 30 seconds to acquire the mutex
    if ($mutex.WaitOne(30000)) {
        try {
            # Check if file needs to be regenerated
            $needsRegeneration = $true
            if (Test-Path $OutputPath) {
                $existingContent = Get-Content $OutputPath -Raw
                if ($existingContent -match "Copyright © 2016-$Year BornToBeRoot") {
                    $needsRegeneration = $false
                }
            }
            
            if ($needsRegeneration) {
                Write-Host "Generating $OutputPath with year: $Year"
                $content = Get-Content $TemplatePath -Raw
                $content = $content -replace '\{CURRENT_YEAR\}', $Year
                $content | Set-Content $OutputPath -Encoding UTF8 -NoNewline
            } else {
                Write-Host "$OutputPath is already up-to-date for year: $Year"
            }
        } finally {
            $mutex.ReleaseMutex()
        }
    } else {
        Write-Warning "Failed to acquire mutex for GlobalAssemblyInfo generation within timeout"
    }
} finally {
    $mutex.Dispose()
}
