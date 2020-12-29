$ErrorActionPreference = 'Stop'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/2020.12.2/NETworkManager_2020.12.2_Setup.exe'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'EXE'
  url           = $url
  softwareName  = 'NETworkManager*'

  checksum      = '4F0EA9AB5969021901AA107A40B0F1C649AB2A39FFC55565DDEF318D983021F9'
  checksumType  = 'sha256'
    
  silentArgs   = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-'
  validExitCodes= @(0)
}

Install-ChocolateyPackage @packageArgs