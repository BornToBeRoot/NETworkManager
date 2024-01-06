$ErrorActionPreference = 'Stop'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/2024.1.6.0/NETworkManager_2024.1.6.0_Setup.exe'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'EXE'
  url           = $url
  softwareName  = 'NETworkManager*'

  checksum      = '6D5B590ACA3085A4D8BF849D2404C471B0B1AA606E20453FF7C312D99C1845FC'
  checksumType  = 'sha256'
    
  silentArgs   = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-'
  validExitCodes= @(0)
}

Install-ChocolateyPackage @packageArgs
