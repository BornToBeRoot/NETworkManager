
$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.6.2.0/NETworkManager_v1.6.2.0_Setup.msi'  
$url64      = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.6.2.0/NETworkManager_v1.6.2.0_Setup.msi'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'MSI'
  url           = $url
  url64bit      = $url64

  softwareName  = 'NETworkManager*'

  checksum      = '9DBED1062CCC8B99E682227C31B7E272CBA126B700CE5A0095D1AAD9ABE4CDDB'
  checksumType  = 'sha256'
  checksum64    = '9DBED1062CCC8B99E682227C31B7E272CBA126B700CE5A0095D1AAD9ABE4CDDB'
  checksumType64= 'sha256'

  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs