#[xml]$LatestPorts = Get-Content -Path "$PSScriptRoot\Service Name and Transport Protocol Port Number Registry.xml"
[xml]$LatestPorts = (Invoke-WebRequest -Uri "https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xml").Content

$Output = ""

foreach($record in $LatestPorts.ChildNodes.record)
{
    if([string]::IsNullOrEmpty($record.number) -or ([string]::IsNullOrEmpty($record.protocol)))
    {        
        continue   
    }
    
    $Description = ($record.description -replace '`n','') -replace '\s+',' '

    $Number = $record.number

    if($Number -like "*-*")
    {
        $NumberArr = $Number.Split('-')

        foreach($Number1 in $NumberArr[0]..$NumberArr[1])
        {
            $Output += "$Number1|$($record.protocol)|$($record.name)|$Description`n"   
        }
    }
    else 
    {
        $Output += "$Number|$($record.protocol)|$($record.name)|$Description`n"   
    }    
}

Out-File -InputObject $Output -FilePath (Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "Source\NETworkManager\Resources\ports.txt")