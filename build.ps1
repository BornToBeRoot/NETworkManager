$Version = "2020.9.0"
$IsPreview = $true

$BuildPath = "$PSScriptRoot\Build"

if(Test-Path -Path $BuildPath)
{
    Remove-Item $BuildPath -Recurse
}

# Dotnet restore and build
dotnet restore "$PSScriptRoot\Source\NETworkManager.sln"
dotnet build --configuration Release "$PSScriptRoot\Source\NETworkManager.sln"

# Copy files
Copy-Item -Recurse -Path "$PSScriptRoot\Source\NETworkManager\bin\Release\net5.0-windows" -Destination "$BuildPath\NETworkManager"

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