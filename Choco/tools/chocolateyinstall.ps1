
$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.11.0.0/NETworkManager_v1.11.0.0_Setup.msi'  

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'MSI'
  url           = $url  

  softwareName  = 'NETworkManager*'

  checksum      = '5000362E3379D40F62D71B3E1042F6B56F1FF1892D528E2E3BEF6BEF7DE86AC4'
  checksumType  = 'sha256'

  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs