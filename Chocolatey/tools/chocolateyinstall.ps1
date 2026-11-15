$ErrorActionPreference = 'Stop'

$packageName = 'NETworkManager'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$packageArgs = @{
  packageName    = $packageName
  unzipLocation  = $toolsDir
  fileType       = 'MSI'
  url            = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/2026.11.15.0/NETworkManager_2026.11.15.0_Setup_win-x64.msi'

  softwareName   = 'NETworkManager*'

  checksum       = '4E6189C2F877EAD8225A32B2F4E1D97D0075A5C5347D0CE5055344F5E894925C'
  checksumType   = 'sha256'

  silentArgs     = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes = @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs
