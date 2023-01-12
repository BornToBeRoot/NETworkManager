$ErrorActionPreference = "Stop"

# Set current directory to script directory
Set-Location -Path $PSScriptRoot

# Output path where the build and all generated files is stored
$BuildPath = "$PSScriptRoot\Build"

# Build configuration
$TargetFramework = "net6.0-windows10.0.17763.0"
$RuntimeIdentifier = "win-x64"

# Remove old build and generated files
if (Test-Path -Path $BuildPath) {
    Remove-Item -Path $BuildPath -Recurse -Force -ErrorAction Stop
}

# Run a cleanup before the build
& ".\cleanup.ps1"

# Update the version based on the current date (e.g. 2021.2.15.0)
$Date = Get-Date
$Patch = 0
$VersionString = "$($Date.Year).$($Date.Month).$($Date.Day).$Patch"

# Update assembly version
$PatternVersion = '\[assembly: AssemblyVersion\("(.*)"\)\]'
$PatternFileVersion = '\[assembly: AssemblyFileVersion\("(.*)"\)\]'

$AssemblyFile = "$PSScriptRoot\Source\GlobalAssemblyInfo.cs"

$AssemlbyContent = Get-Content -Path $AssemblyFile -Encoding utf8
$AssemlbyContent = $AssemlbyContent -replace $PatternVersion,"[assembly: AssemblyVersion(""$($VersionString)"")]"
$AssemlbyContent = $AssemlbyContent -replace $PatternFileVersion,"[assembly: AssemblyFileVersion(""$($VersionString)"")]"
$AssemlbyContent | Set-Content -Path $AssemblyFile -Encoding utf8

# Update inno setup version
$PatternSetupVersion = '#define MyAppVersion "(.*)"'

$InnoSetupFile = "$PSScriptRoot\InnoSetup.iss"

$SetupContent = Get-Content -Path $InnoSetupFile -Encoding utf8
$SetupContent = $SetupContent -replace $PatternSetupVersion,"#define MyAppVersion ""$($VersionString)"""
$SetupContent | Set-Content -Path $InnoSetupFile -Encoding utf8

### Warnings ###
# CS4014 - Call is not awaited
# NU1701 - Target framework is .NET Framework
# CS1591 - Missing XML comment

# Try to get msbuild path with vswhere (See: https://www.meziantou.net/locating-msbuild-on-a-machine.htm)
$VSwherePath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

if(-not(Test-Path -Path $VSwherePath -PathType Leaf))
{
    Write-Error "Could not find VSwhere. Is Visual Studio installed?" -ErrorAction Stop
}

$VSwhere = & $VSwherePath -version "[16.0,18.0)" -products * -requires Microsoft.Component.MSBuild -prerelease -latest -utf8 -format json | ConvertFrom-Json
$MSBuildPath = Join-Path $VSwhere[0].installationPath "MSBuild" "Current" "Bin" "MSBuild.exe"

# Test if we found msbuild
if(-not(Test-Path -Path $MSBuildPath -PathType Leaf))
{
    Write-Error "Could not find msbuild. Is Visual Studio installed?" -ErrorAction Stop
}

# Build with msbuild
Start-Process -FilePath $MSBuildPath -ArgumentList "$PSScriptRoot\Source\NETworkManager.sln /restore /t:Clean,Build /p:Configuration=Release /p:TargetFramework=$TargetFramework /p:RuntimeIdentifier=$RuntimeIdentifier" -Wait -NoNewWindow

# Test if build is available
if(-not(Test-Path -Path "$PSScriptRoot\Source\NETworkManager\bin\Release\$TargetFramework\$RuntimeIdentifier\NETworkManager.exe" -PathType Leaf))
{
    Write-Error "Could not find release build. Is .NET SDK 6.0 or later installed?" -ErrorAction Stop
}

# Create folder
New-Item -Path "$BuildPath\NETworkManager" -ItemType Directory -Force

# Copy files
Copy-Item -Path "$PSScriptRoot\Source\NETworkManager\bin\Release\$TargetFramework\$RuntimeIdentifier\*" -Destination "$BuildPath\NETworkManager" -Recurse -Force

# Get NETworkManager file version
$Version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$BuildPath\NETworkManager\NETworkManager.exe").FileVersion

# Remove .pdb files
Get-ChildItem -Path "$BuildPath\NETworkManager" | Where-Object {$_.Name.EndsWith(".pdb")} | Remove-Item

# Cleanup some other files
Remove-Item -Path "$BuildPath\NETworkManager\WebView2Loader.dll"

# Create archive
Compress-Archive -Path "$BuildPath\NETworkManager" -DestinationPath "$BuildPath\NETworkManager_$($Version)_Archive.zip"

# Create portable archive
New-Item -Path "$BuildPath\NETworkManager" -Name "IsPortable.settings" -ItemType File
Compress-Archive -Path "$BuildPath\NETworkManager" -DestinationPath "$BuildPath\NETworkManager_$($Version)_Portable.zip"
Remove-Item -Path "$BuildPath\NETworkManager\IsPortable.settings"

# Create installer with InnoSetup
$InnoSetupPath = "${env:ProgramFiles(x86)}\Inno Setup 6"

# Check if additional language files are available for InnoSetup
$InnoSetupLanguageMissing = $false

foreach($File in @("ChineseSimplified.isl", "ChineseTraditional.isl", "Hungarian.isl", "Korean.isl")) {
    if(-not(Test-Path -Path "$InnoSetupPath\Languages\$File" -PathType Leaf))
    {
        Write-Host "$File not found in InnoSetup language folder." -ForegroundColor Yellow
        $InnoSetupLanguageMissing = $true
    }
}

if($InnoSetupLanguageMissing) {
    Write-Host "You can download the language files here: https://github.com/jrsoftware/issrc/blob/main/Files/Languages/" -ForegroundColor Yellow
}

# Check if InnoSetup is installed
$InnoSetupCompiler = "$InnoSetupPath\ISCC.exe"

if(-not(Test-Path -Path $InnoSetupCompiler -PathType Leaf) -or $InnoSetupLanguageMissing)
{
    Write-Host "InnoSetup is not installed correctly. Skip installer build..." -ForegroundColor Cyan
}
else
{
    Start-Process -FilePath $InnoSetupCompiler -ArgumentList """$PSScriptRoot\InnoSetup.iss""" -NoNewWindow -Wait
}

# Create SHA256 file hashes
foreach($hash in Get-ChildItem -Path $BuildPath | Where-Object {$_.Name.EndsWith(".zip") -or $_.Name.EndsWith(".exe")} | Get-FileHash)
{
    "$($hash.Algorithm) | $($hash.Hash) | $([System.IO.Path]::GetFileName($hash.Path))" | Out-File -FilePath "$BuildPath\NETworkManager_$($Version)_Hash.txt" -Encoding utf8 -Append
}

# Build finished
Write-Host "`n`nBuild finished! The following files have been created under ""$BuildPath""`n" -ForegroundColor Green

Get-Content "$BuildPath\NETworkManager_$($Version)_Hash.txt"
