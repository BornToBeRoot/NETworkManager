$ErrorActionPreference = 'Stop'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/2024.1.7.0/NETworkManager_2024.1.7.0_Setup.exe'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'EXE'
  url           = $url
  softwareName  = 'NETworkManager*'

  checksum      = 'FE15739018AA90674FC0E0AE1BDBFB8DC015517020439C7151C3634D936FA681'
  checksumType  = 'sha256'
    
  silentArgs   = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-'
  validExitCodes= @(0)
}

Install-ChocolateyPackage @packageArgs
