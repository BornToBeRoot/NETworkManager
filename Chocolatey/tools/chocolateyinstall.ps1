$ErrorActionPreference = 'Stop'

$packageName = 'NETworkManager'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$packageArgs = @{
  packageName    = $packageName
  unzipLocation  = $toolsDir
  fileType       = 'MSI'
  url            = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/2025.1.18.0/NETworkManager_2025.1.18.0_Setup.msi'

  softwareName   = 'NETworkManager*'

  checksum       = '0E8722AB821FDFC5079C8E914CDBC293F373F551924EFBA14CA4AF6E68B678F9'
  checksumType   = 'sha256'

  silentArgs     = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes = @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs
