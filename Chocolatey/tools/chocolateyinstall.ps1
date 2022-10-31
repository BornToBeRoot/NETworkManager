$ErrorActionPreference = 'Stop'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/2022.10.31.0/NETworkManager_2022.10.31.0_Setup.exe'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'EXE'
  url           = $url
  softwareName  = 'NETworkManager*'

  checksum      = '18A9528247BFA1880AF61CEAA1401FCA7C8271BC6635B40BA72D77AB065867A0'
  checksumType  = 'sha256'
    
  silentArgs   = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-'
  validExitCodes= @(0)
}

Install-ChocolateyPackage @packageArgs
