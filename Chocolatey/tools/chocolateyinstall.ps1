$ErrorActionPreference = 'Stop'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/BornToBeRoot/NETworkManager/releases/download/2021.1.2/NETworkManager_2021.1.2_Setup.exe'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'EXE'
  url           = $url
  softwareName  = 'NETworkManager*'

  checksum      = '600C5F6F57ECE66E87FD9AC5FA8024A2CE7DF156D1FA7D5D17C1809E89CC0548'
  checksumType  = 'sha256'
    
  silentArgs   = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /TASKS="desktopicon" /SP-'
  validExitCodes= @(0)
}

Install-ChocolateyPackage @packageArgs