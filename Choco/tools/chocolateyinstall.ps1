
$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.6.3.0/NETworkManager_v1.6.3.0_Setup.msi'  
$url64      = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.6.3.0/NETworkManager_v1.6.3.0_Setup.msi'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'MSI'
  url           = $url
  url64bit      = $url64

  softwareName  = 'NETworkManager*'

  checksum      = 'B0F837C819AF15A0CAEF01405472B6EBE5972470F23F86437D321322A89C1352'
  checksumType  = 'sha256'
  checksum64    = 'B0F837C819AF15A0CAEF01405472B6EBE5972470F23F86437D321322A89C1352'
  checksumType64= 'sha256'

  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs