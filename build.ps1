$Version = "2020.12.0"
$IsPreview = $true

$BuildPath = "$PSScriptRoot\Build"

if(Test-Path -Path $BuildPath)
{
    Remove-Item $BuildPath -Recurse
}

# Dotnet clean, restore and build
dotnet clean "$PSScriptRoot\Source\NETworkManager.sln"
dotnet restore "$PSScriptRoot\Source\NETworkManager.sln"
dotnet build --configuration Release "$PSScriptRoot\Source\NETworkManager.sln"

# Copy files
Copy-Item -Recurse -Path "$PSScriptRoot\Source\NETworkManager\bin\Release\net5.0-windows10.0.17763.0" -Destination "$BuildPath\NETworkManager"

# Is preview?
if($IsPreview)
{
    New-Item -Path "$BuildPath\NETworkManager" -Name "IsPreview.settings" -ItemType File
}

# Archiv Build
Compress-Archive -Path "$BuildPath\NETworkManager" -DestinationPath "$BuildPath\NETworkManager_$($Version)_Archiv.zip"

# Portable Build
New-Item -Path "$BuildPath\NETworkManager" -Name "IsPortable.settings" -ItemType File
Compress-Archive -Path "$BuildPath\NETworkManager" -DestinationPath "$BuildPath\NETworkManager_$($Version)_Portable.zip"
Remove-Item -Path "$BuildPath\NETworkManager\IsPortable.settings"

# Installer Build...

Write-Host "Your build is here: $BuildPath "