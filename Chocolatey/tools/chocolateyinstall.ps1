$ErrorActionPreference = 'Stop'

$packageName = 'NETworkManager'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$packageArgs = @{
  packageName    = $packageName
  unzipLocation  = $toolsDir
  fileType       = 'MSI'
  url            = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/2024.5.27.0/NETworkManager_2024.5.27.0_Setup.msi'

  softwareName   = 'NETworkManager*'

  checksum       = 'BE6A466ACD8912B21702A8D07CC45D7DEF7D177A6BFC3BDE2AB97E1DBE8E9CEA'
  checksumType   = 'sha256'

  silentArgs     = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes = @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs
