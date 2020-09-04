$Branch = "net5-develop"
$Version = "2020.9.0"
$IsPreview = $true

$Uri = "https://github.com/BornToBeRoot/NETworkManager/archive/$Branch.zip"
$TempFolder = "$PSScriptRoot\Temp"
$DownloadedFile = "$TempFolder\download.zip"
$BuildPath = "$PSScriptRoot\Build"

# Download from Github
New-Item -ItemType Directory -Path $TempFolder 
Invoke-WebRequest -Uri $Uri -UseBasicParsing -OutFile $DownloadedFile -ErrorAction Stop

# Expand archive
Expand-Archive -Path $DownloadedFile -DestinationPath $TempFolder

# Dotnet restore and build
dotnet restore "$TempFolder\NETworkManager-$Branch\Source\NETworkManager.sln"
dotnet build --configuration Release "$TempFolder\NETworkManager-$Branch\Source\NETworkManager.sln"

# Copy files
Copy-Item -Recurse -Path "$TempFolder\NETworkManager-$Branch\Source\NETworkManager\bin\Release\net5.0-windows" -Destination "$PSScriptRoot\Build\NETworkManager"

Remove-Item -Recurse -Path "$PSScriptRoot\Temp"

# Is preview?
if($IsPreview)
{
    New-Item -Path "$BuildPath\NETworkManager" -Name "IsPreview.settings" -ItemType File
}

# Archiv Build
Compress-Archive -Path "$BuildPath\NETworkManager" -DestinationPath "$BuildPath\NETworkManager_$($Version)_Archiv.zip"

# Portable Build
New-Item -Path "$BuildPath\NETworkManager" -Name "IsPortable.settings" -ItemType File
Compress-Archive -Path "$BuildPath\NETworkManager" -DestinationPath "$PSScriptRoot\Build\NETworkManager_$($Version)_Portable.zip"
Remove-Item -Path "$BuildPath\NETworkManager\IsPortable.settings"