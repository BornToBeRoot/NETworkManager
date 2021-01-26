$IsPreview = $false

$BuildPath = "$PSScriptRoot\Build"

Set-Location -Path $PSScriptRoot

if (Test-Path -Path $BuildPath) {
    Remove-Item -Path $BuildPath -Recurse -ErrorAction Stop
}

# Dotnet clean, restore, build and publish
dotnet clean "$PSScriptRoot\Source\NETworkManager.sln"
dotnet restore "$PSScriptRoot\Source\NETworkManager.sln"
# dotnet build --configuration Release "$PSScriptRoot\Source\NETworkManager.sln"
dotnet publish --configuration Release --framework net5.0-windows10.0.17763.0 --runtime win10-x64 --self-contained false --output "$BuildPath\NETworkManager" "$PSScriptRoot\Source\NETworkManager\NETworkManager.csproj" 

# Test if release build is available
if(-not(Test-Path -Path "$BuildPath\NETworkManager\NETworkManager.exe"))
{
    Write-Error "Could not find dotnet release build. Is .NET SDK 5.0 or later installed?" -ErrorAction Stop
}

# Get NETworkManager File Version
$Version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$BuildPath\NETworkManager\NETworkManager.exe").FileVersion

# Cleanup WebView2Loader.dll (https://github.com/MicrosoftEdge/WebView2Feedback/issues/461)
Remove-Item "$BuildPath\NETworkManager\arm64" -Recurse
Remove-Item "$BuildPath\NETworkManager\x64" -Recurse
Remove-Item "$BuildPath\NETworkManager\x86" -Recurse

# Cleanup .pdb files
Get-ChildItem -Recurse | Where-Object {$_.Name.EndsWith(".pdb")} | Remove-Item

# Is preview?
if ($IsPreview) {
    New-Item -Path "$BuildPath\NETworkManager" -Name "IsPreview.settings" -ItemType File
}

# Archiv Build
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
