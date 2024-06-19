$ErrorActionPreference = 'Stop'

$packageName = 'NETworkManager'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$packageArgs = @{
  packageName    = $packageName
  unzipLocation  = $toolsDir
  fileType       = 'MSI'
  url            = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/2024.6.15.0/NETworkManager_2024.6.15.0_Setup.msi'

  softwareName   = 'NETworkManager*'

  checksum       = 'CB78E596EF0247808522B6F5883CD154BF0A8630496E2545AAF99E1B324E3D9B'
  checksumType   = 'sha256'

  silentArgs     = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes = @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs
