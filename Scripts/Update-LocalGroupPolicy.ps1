# Check for administrative privileges and rerun as administrator if needed
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Host "Script is not running with administrative privileges. Restarting with elevated rights..."
    Start-Process -FilePath "powershell.exe" -ArgumentList "-NoProfile -ExecutionPolicy Bypass -NoExit -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

# Update local group policy files for NETworkManager
[string]$GpoFolderPath = Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "GPO"
[string]$AdmxFilePath = Join-Path -Path $GpoFolderPath -ChildPath "NETworkManager.admx"
[string]$AdmlFolderPath = Join-Path -Path $GpoFolderPath -ChildPath "en-US"
[string]$AdmlFilePath = Join-Path -Path $AdmlFolderPath -ChildPath "NETworkManager.adml"

# Copy ADMX file to local policy store
$LocalAdmxPath = Join-Path -Path "$env:SystemRoot\PolicyDefinitions" -ChildPath "NETworkManager.admx"
Copy-Item -Path $AdmxFilePath -Destination $LocalAdmxPath -Force    

# Copy ADML file to local policy store
$LocalAdmlPath = Join-Path -Path "$env:SystemRoot\PolicyDefinitions\en-US" -ChildPath "NETworkManager.adml"
Copy-Item -Path $AdmlFilePath -Destination $LocalAdmlPath -Force

# End of script
Write-Host "Local Group Policy files for NETworkManager have been updated."
