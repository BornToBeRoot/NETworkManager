#$LatestOUI = Get-Content -Path "$PSScriptRoot\oui_from_web.txt"
$LatestOUI = (Invoke-WebRequest -Uri "http://linuxnet.ca/ieee/oui.txt").Content

$Output = ""

foreach($Line in $LatestOUI -split '[\r\n]')
{
    if($Line -match "^[A-F0-9]{6}")
    {        
        # Line looks like: 2405F5     (base 16)		Integrated Device Technology (Malaysia) Sdn. Bhd.
        $Output += ($Line -replace '\s+', ' ').Replace(' (base 16) ', '|').Trim() + "`n"
    }
}

Out-File -InputObject $Output -FilePath (Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "Source\NETworkManager\Resources\oui.txt")