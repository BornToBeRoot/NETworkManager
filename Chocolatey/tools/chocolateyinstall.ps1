$ErrorActionPreference = 'Stop'

$packageName = 'NETworkManager'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$packageArgs = @{
  packageName    = $packageName
  unzipLocation  = $toolsDir
  fileType       = 'MSI'
  url            = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/2026.2.22.0/NETworkManager_2026.2.22.0_Setup.msi'

  softwareName   = 'NETworkManager*'

  checksum       = 'AD3A107D76B9391C88C4427717F243C64309F4D66FEA9775AAB75C1CD51A2D4E'
  checksumType   = 'sha256'

  silentArgs     = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes = @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs
