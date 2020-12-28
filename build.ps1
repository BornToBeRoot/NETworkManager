$Version = "2020.12.1"
$IsPreview = $false

$BuildPath = "$PSScriptRoot\Build"

Set-Location -Path $PSScriptRoot

if (Test-Path -Path $BuildPath) {
    Remove-Item -Path $BuildPath -Recurse -ErrorAction Stop
}

# Dotnet clean, restore and build
dotnet clean "$PSScriptRoot\Source\NETworkManager.sln"
dotnet restore "$PSScriptRoot\Source\NETworkManager.sln"
dotnet build --configuration Debug "$PSScriptRoot\Source\NETworkManager.sln"

$ReleasePath = "$PSScriptRoot\Source\NETworkManager\bin\Release\net5.0-windows10.0.17763.0"

# Test if release build is available
if(-not(Test-Path -Path $ReleasePath))
{
    Write-Error "Could not find dotnet release build. Is .NET SDK 5.0 or later installed?" -ErrorAction Stop
}

# Copy files
Copy-Item -Recurse -Path $ReleasePath -Destination "$BuildPath\NETworkManager"

# Cleanup .pdb files
Get-ChildItem -Recurse | Where-Object {$_.Name.EndsWith(".pdb")} | Remove-Item

# Is preview?
if ($IsPreview) {
    New-Item -Path "$BuildPath\NETworkManager" -Name "IsPreview.settings" -ItemType File
}

# Archiv Build / Sources
Compress-Archive -Path "$BuildPath\NETworkManager" -DestinationPath "$BuildPath\NETworkManager_$($Version)_Archiv.zip"

# Portable Build
New-Item -Path "$BuildPath\NETworkManager" -Name "IsPortable.settings" -ItemType File
Compress-Archive -Path "$BuildPath\NETworkManager" -DestinationPath "$BuildPath\NETworkManager_$($Version)_Portable.zip"
Remove-Item -Path "$BuildPath\NETworkManager\IsPortable.settings"

# Installer Build
$InnoSetupCompiler = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"

if (Test-Path -Path $InnoSetupCompiler) {
    Start-Process -FilePath $InnoSetupCompiler -ArgumentList """$PSScriptRoot\InnoSetup.iss""" -NoNewWindow -Wait
}
else {
    Write-Host "InnoSetup not installed or not found. Skip installer build..." -ForegroundColor Yellow
}

Get-ChildItem -Path $BuildPath | Where-Object {$_.Name.EndsWith(".zip") -or $_.Name.EndsWith(".exe")} | Get-FileHash 

Write-Host "Build finished! All files are here: $BuildPath" -ForegroundColor Green