$BuildPath = "$PSScriptRoot\Build"

Set-Location -Path $PSScriptRoot

if (Test-Path -Path $BuildPath) {
    Remove-Item -Path $BuildPath -Recurse -ErrorAction Stop
}

# Set the version based on the current date (e.g. 2021.2.15.0)
$Date = Get-Date
$Patch = 0
$VersionString = "$($Date.Year).$($Date.Month).$($Date.Day).$Patch"

# Set assembly version
$PatternVersion = '\[assembly: AssemblyVersion\("(.*)"\)\]'
$PatternFileVersion = '\[assembly: AssemblyFileVersion\("(.*)"\)\]'

$AssemblyFile = "$PSScriptRoot\Source\GlobalAssemblyInfo.cs"

$AssemlbyContent = Get-Content -Path $AssemblyFile -Encoding utf8
$AssemlbyContent = $AssemlbyContent -replace $PatternVersion,"[assembly: AssemblyVersion(""$($VersionString)"")]"
$AssemlbyContent = $AssemlbyContent -replace $PatternFileVersion,"[assembly: AssemblyFileVersion(""$($VersionString)"")]"
$AssemlbyContent | Set-Content -Path $AssemblyFile -Encoding utf8

# Set inno setup version
$PatternSetupVersion = '#define MyAppVersion "(.*)"'

$InnoSetupFile = "$PSScriptRoot\InnoSetup.iss"

$SetupContent = Get-Content -Path $InnoSetupFile -Encoding utf8
$SetupContent = $SetupContent -replace $PatternSetupVersion,"#define MyAppVersion ""$($VersionString)"""
$SetupContent | Set-Content -Path $InnoSetupFile -Encoding utf8

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

# Cleanup .pdb files
Get-ChildItem -Recurse | Where-Object {$_.Name.EndsWith(".pdb")} | Remove-Item

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

# SHA256 file hash
foreach($hash in Get-ChildItem -Path $BuildPath | Where-Object {$_.Name.EndsWith(".zip") -or $_.Name.EndsWith(".exe")} | Get-FileHash)
{
    "$($hash.Algorithm) | $($hash.Hash) | $([System.IO.Path]::GetFileName($hash.Path))" | Out-File -FilePath "$BuildPath\NETworkManager_$($Version)_Hash.txt" -Encoding utf8 -Append
}

Write-Host "Build finished! All files are here: $BuildPath" -ForegroundColor Green