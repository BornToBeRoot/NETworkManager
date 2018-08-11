
$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/v1.7.0.0/NETworkManager_v1.7.0.0_Setup.msi'  

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'MSI'
  url           = $url  

  softwareName  = 'NETworkManager*'

  checksum      = '15EB077FA5C98A83DE0AA2DBEB688D481FEEB39083DBD2C6F37CD5A858D2499F'
  checksumType  = 'sha256'

  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs