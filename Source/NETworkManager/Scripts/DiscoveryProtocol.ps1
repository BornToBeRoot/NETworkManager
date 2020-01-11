<#
    Original
    --------
    Autor:   lahell
    Date:    04.12.2019
	Version: 1.1.0
    Source:  https://github.com/lahell/PSDiscoveryProtocol or https://www.powershellgallery.com/packages/PSDiscoveryProtocol
    License: MIT
    

    Modified
    --------
    Autor:   BornToBeRoot
    Date:    02.01.2020
#>

<#
    Build Action: Embedded Resource --> Script is executed as admin and should no be changed by anyone :)
#>

class DiscoveryProtocolPacket
{
    [string]$MachineName
    [datetime]$TimeCreated
    [int]$FragmentSize
    [byte[]]$Fragment

    DiscoveryProtocolPacket([string]$MachineName, [datetime]$TimeCreated, [int]$FragmentSize, [byte[]]$Fragment)
    {
        $this.MachineName  = $MachineName
        $this.TimeCreated  = $TimeCreated
        $this.FragmentSize = $FragmentSize
        $this.Fragment     = $Fragment

        Add-Member -InputObject $this -MemberType ScriptProperty -Name IsDiscoveryProtocolPacket -Value {
            if (
                [UInt16]0x2000 -eq [BitConverter]::ToUInt16($this.Fragment[21..20], 0) -or
                [UInt16]0x88CC -eq [BitConverter]::ToUInt16($this.Fragment[13..12], 0)
            ) { return [bool]$true } else { return [bool]$false }
        }

        Add-Member -InputObject $this -MemberType ScriptProperty -Name DiscoveryProtocolType -Value {
            if ([UInt16]0x2000 -eq [BitConverter]::ToUInt16($this.Fragment[21..20], 0)) {
                return [string]'CDP'
            }
            elseif ([UInt16]0x88CC -eq [BitConverter]::ToUInt16($this.Fragment[13..12], 0)) {
                return [string]'LLDP'
            }
            else {
                return [string]::Empty
            }
        }

        Add-Member -InputObject $this -MemberType ScriptProperty -Name SourceAddress -Value {
            [PhysicalAddress]::new($this.Fragment[6..11]).ToString()
        }
    }
}

<#
PS C:\> Invoke-DiscoveryProtocolCapture

IsDiscoveryProtocolPacket : True
DiscoveryProtocolType     : LLDP
SourceAddress             : 001122334455
MachineName               : COMPUTER1
TimeCreated               : 01.01.2020 18:31:25
FragmentSize              : 143
Fragment                  : {1, 128, 194, 0...}
#>

function Invoke-DiscoveryProtocolCapture {
    [CmdletBinding()]
    [OutputType('DiscoveryProtocolPacket')]    
    param(
        [Parameter(Position=0, Mandatory=$true)]
        [String]$NetAdapter,

        [Parameter(Position=1)]
        [Int16]$Duration = $(if ($Type -eq 'LLDP') { 32 } else { 62 }),

        [Parameter(Position=2)]
        [ValidateSet('CDP', 'LLDP')]
        [String]$Type
    )

    begin {
        $Identity = [Security.Principal.WindowsIdentity]::GetCurrent()
        $Principal = New-Object Security.Principal.WindowsPrincipal $Identity

        if (-not $Principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)) {
            throw 'Invoke-DiscoveryProtocolCapture requires elevation. Please run PowerShell as administrator.'
        }

        if ($MyInvocation.InvocationName -ne $MyInvocation.MyCommand) {
            if ($MyInvocation.InvocationName -eq 'Capture-CDPPacket') { 
                $Type = 'CDP' 
            }

            if ($MyInvocation.InvocationName -eq 'Capture-LLDPPacket') { 
                $Type = 'LLDP' 
            }

            $Warning = '{0} has been deprecated, please use {1}' -f $MyInvocation.InvocationName, $MyInvocation.MyCommand
            Write-Warning $Warning
        }
    }

    process {

            $TempFile = New-TemporaryFile
            $ETLFile = Rename-Item -Path $TempFile.FullName -NewName $TempFile.FullName.Replace('.tmp', '.etl') -PassThru                      

            $Adapter = Get-NetAdapter -Name $NetAdapter | Select-Object -First 1 Name, MacAddress

            $MACAddress = [PhysicalAddress]::Parse($Adapter.MacAddress).ToString()

            if ($Adapter) {
                $SessionName = 'Capture-{0}' -f (Get-Date).ToString('s')
                New-NetEventSession -Name $SessionName -LocalFilePath $($ETLFile.FullName) -CaptureMode SaveToFile | Out-Null

                $LinkLayerAddress = switch ($Type) {
                    'CDP'   { '01-00-0c-cc-cc-cc' }
                    'LLDP'  { '01-80-c2-00-00-0e', '01-80-c2-00-00-03', '01-80-c2-00-00-00' }
                    Default { '01-00-0c-cc-cc-cc', '01-80-c2-00-00-0e', '01-80-c2-00-00-03', '01-80-c2-00-00-00' }
                }

                $PacketCaptureParams = @{
                    SessionName      = $SessionName
                    TruncationLength = 0
                    CaptureType      = 'Physical'                   
                    LinkLayerAddress = $LinkLayerAddress
                }

                Add-NetEventPacketCaptureProvider @PacketCaptureParams | Out-Null
                Add-NetEventNetworkAdapter -Name $Adapter.Name -PromiscuousMode $True | Out-Null

                Start-NetEventSession -Name $SessionName

                Start-Sleep -Seconds $Duration               

                Stop-NetEventSession -Name $SessionName

                $Events = Invoke-Command -ScriptBlock {
                    $Events = Get-WinEvent -Path $($ETLFile.FullName) -Oldest -FilterXPath "*[System[EventID=1001]]"

                    [string[]]$XpathQueries = @(
                        "Event/EventData/Data[@Name='FragmentSize']"
                        "Event/EventData/Data[@Name='Fragment']"
                    )

                    $PropertySelector = [System.Diagnostics.Eventing.Reader.EventLogPropertySelector]::new($XpathQueries)

                    foreach ($Event in $Events) {
                        $EventData = $Event | Select-Object MachineName, TimeCreated
                        $EventData | Add-Member -NotePropertyName FragmentSize -NotePropertyValue $null
                        $EventData | Add-Member -NotePropertyName Fragment -NotePropertyValue $null
                        $EventData.FragmentSize, $EventData.Fragment = $Event.GetPropertyValues($PropertySelector)
                        $EventData
                    }
                }

                $FoundPacket = $null

                foreach ($Event in $Events) {
                    $Packet = [DiscoveryProtocolPacket]::new(
                        $Event.MachineName,
                        $Event.TimeCreated,
                        $Event.FragmentSize,
                        $Event.Fragment
                    )

                    if ($Packet.IsDiscoveryProtocolPacket -and $Packet.SourceAddress -ne $MACAddress) {
                        $FoundPacket = $Packet
                        break
                    }
                }

                Remove-NetEventSession -Name $SessionName

                Remove-Item -Path $($ETLFile.FullName) -Force                                               

                if ($FoundPacket) {
                    $FoundPacket
                }
            } else {
                Write-Error "Unable to find network adapter!"
                return
            }
        }
    end {}
}

function Get-DiscoveryProtocolData {

<#
    PS C:\> $Packet = Invoke-DiscoveryProtocolCapture
    PS C:\> Get-DiscoveryProtocolData -Packet $Packet

    Port        : FastEthernet0/1
    Device      : SWITCH1.domain.example
    Model       : cisco WS-C2960-48TT-L
    IPAddress   : 192.0.2.10
    VLAN        : 10
    TimeCreated : 01.01.2020 18:31:25
    Computer    : COMPUTER1
    Type        : CDP
#>

    [CmdletBinding()]
    [Alias('Parse-CDPPacket', 'Parse-LLDPPacket')]
    param(
        [Parameter(Position=0,
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true)]
        [DiscoveryProtocolPacket[]]
        $Packet
    )

    begin {
        if ($MyInvocation.InvocationName -ne $MyInvocation.MyCommand) {
            $Warning = '{0} has been deprecated, please use {1}' -f $MyInvocation.InvocationName, $MyInvocation.MyCommand
            Write-Warning $Warning
        }
    }

    process {
        foreach ($item in $Packet) {
            switch ($item.DiscoveryProtocolType) {
                'CDP'   { $PacketData = ConvertFrom-CDPPacket -Packet $item.Fragment }
                'LLDP'  { $PacketData = ConvertFrom-LLDPPacket -Packet $item.Fragment }
                Default { throw 'No valid CDP or LLDP found in $Packet' }
            }

            $PacketData | Add-Member -NotePropertyName TimeCreated -NotePropertyValue $item.TimeCreated
            $PacketData | Add-Member -NotePropertyName Computer -NotePropertyValue $item.MachineName
            $PacketData | Add-Member -NotePropertyName Type -NotePropertyValue $item.DiscoveryProtocolType
            $PacketData
        }
    }

    end {}
}


function ConvertFrom-CDPPacket {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,
            Mandatory=$true)]
        [byte[]]$Packet
    )

    begin {}

    process {

        $Offset = 26
        $Hash = @{}

        while ($Offset -lt ($Packet.Length - 4)) {

            $Type   = [BitConverter]::ToUInt16($Packet[($Offset + 1)..$Offset], 0)
            $Length = [BitConverter]::ToUInt16($Packet[($Offset + 3)..($Offset + 2)], 0)

            switch ($Type)
            {
                1  { $Hash.Add('Device',    [System.Text.Encoding]::ASCII.GetString($Packet[($Offset + 4)..($Offset + $Length)])) }
                3  { $Hash.Add('Port',      [System.Text.Encoding]::ASCII.GetString($Packet[($Offset + 4)..($Offset + $Length)])) }
                6  { $Hash.Add('Model',     [System.Text.Encoding]::ASCII.GetString($Packet[($Offset + 4)..($Offset + $Length)])) }
                10 { $Hash.Add('VLAN',      [BitConverter]::ToUInt16($Packet[($Offset + 5)..($Offset + 4)], 0)) }
                22 { $Hash.Add('IPAddress', ([System.Net.IPAddress][byte[]]$Packet[($Offset + 13)..($Offset + 16)]).IPAddressToString) }
            }

            if ($Length -eq 0 ) {
                $Offset = $Packet.Length
            }

            $Offset = $Offset + $Length

        }

        return [PSCustomObject]$Hash

    }

    end {}

}

function ConvertFrom-LLDPPacket {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,
            Mandatory=$true)]
        [byte[]]$Packet
    )

    begin {
        $TlvType = @{
            PortId               = 2
            PortDescription      = 4
            SystemName           = 5
            ManagementAddress    = 8
            OrganizationSpecific = 127
        }
    }

    process {

        $Destination = [PhysicalAddress]::new($Packet[0..5])
        $Source      = [PhysicalAddress]::new($Packet[6..11])
        $LLDP        = [BitConverter]::ToUInt16($Packet[13..12], 0)

        Write-Verbose "Destination: $Destination"
        Write-Verbose "Source: $Source"
        Write-Verbose "LLDP: $LLDP"

        $Offset = 14
        $Mask = 0x01FF
        $Hash = @{}

        while ($Offset -lt $Packet.Length)
        {
            $Type = $Packet[$Offset] -shr 1
            $Length = [BitConverter]::ToUInt16($Packet[($Offset + 1)..$Offset], 0) -band $Mask
            $Offset += 2

            switch ($Type)
            {
                $TlvType.PortId {
                    $Subtype = $Packet[($Offset)]

                    if ($SubType -in (1, 2, 5, 6, 7)) {
                        $Hash.Add('Port', [System.Text.Encoding]::ASCII.GetString($Packet[($Offset + 1)..($Offset + $Length - 1)]))
                    }

                    if ($Subtype -eq 3) {
                        $Hash.Add('Port', [PhysicalAddress]::new($Packet[($Offset + 1)..($Offset + $Length - 1)]))
                    }

                    $Offset += $Length
                    break
                }

                $TlvType.PortDescription {
                    $Hash.Add('Description', [System.Text.Encoding]::ASCII.GetString($Packet[$Offset..($Offset + $Length - 1)]))
                    $Offset += $Length
                    break
                }

                $TlvType.SystemName {
                    $Hash.Add('Device', [System.Text.Encoding]::ASCII.GetString($Packet[$Offset..($Offset + $Length - 1)]))
                    $Offset += $Length
                    break
                }

                $TlvType.ManagementAddress {
                    $AddrLen = $Packet[($Offset)]
                    $Subtype = $Packet[($Offset + 1)]

                    if ($Subtype -eq 1)
                    {
                        $Hash.Add('IPAddress', ([System.Net.IPAddress][byte[]]$Packet[($Offset + 2)..($Offset + $AddrLen)]).IPAddressToString)
                    }

                    $Offset += $Length
                    break
                }

                $TlvType.OrganizationSpecific {
                    $OUI = [System.BitConverter]::ToString($Packet[($Offset)..($Offset + 2)])

                    if ($OUI -eq '00-12-BB') {
                        $Subtype = $Packet[($Offset + 3)]
                        if ($Subtype -eq 10) {
                            $Hash.Add('Model', [System.Text.Encoding]::ASCII.GetString($Packet[($Offset + 4)..($Offset + $Length - 1)]))
                            $Offset += $Length
                            break
                        }
                    }

                    if ($OUI -eq '00-80-C2') {
                        $Subtype = $Packet[($Offset + 3)]
                        if ($Subtype -eq 1) {
                            $Hash.Add('VLAN', [BitConverter]::ToUInt16($Packet[($Offset + 5)..($Offset + 4)], 0))
                            $Offset += $Length
                            break
                        }
                    }

                    $Tlv = [PSCustomObject] @{
                        Type = $Type
                        Value = [System.Text.Encoding]::ASCII.GetString($Packet[$Offset..($Offset + $Length)])
                    }
                    Write-Verbose $Tlv
                    $Offset += $Length
                    break
                }

                default {
                    $Tlv = [PSCustomObject] @{
                        Type = $Type
                        Value = [System.Text.Encoding]::ASCII.GetString($Packet[$Offset..($Offset + $Length)])
                    }
                    Write-Verbose $Tlv
                    $Offset += $Length
                    break
                }
            }
        }
        [PSCustomObject]$Hash
    }

    end {}
}

function Export-Pcap {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true)]
        [DiscoveryProtocolPacket[]]$Packet,

        [Parameter(Mandatory=$true)]
        [ValidateScript({
            if ([System.IO.Path]::IsPathRooted($_)) {
                $AbsolutePath = $_
            } else {
                $AbsolutePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($_)
            }
            if (-not(Test-Path (Split-Path $AbsolutePath -Parent))) {
                throw "Folder does not exist"
            }
            if ($_ -notmatch '\.pcap$') {
                throw "Extension must be pcap"
            }
            return $true
        })]
        [System.IO.FileInfo]$Path,

        [Parameter(Mandatory=$false)]
        [switch]$Invoke
    )

    begin {
        [uint32]$magicNumber = '0xa1b2c3d4'
        [uint16]$versionMajor = 2
        [uint16]$versionMinor = 4
        [int32] $thisZone = 0
        [uint32]$sigFigs = 0
        [uint32]$snapLen = 65536
        [uint32]$network = 1

        $stream = New-Object System.IO.MemoryStream
        $writer = New-Object System.IO.BinaryWriter $stream

        $writer.Write($magicNumber)
        $writer.Write($versionMajor)
        $writer.Write($versionMinor)
        $writer.Write($thisZone)
        $writer.Write($sigFigs)
        $writer.Write($snapLen)
        $writer.Write($network)
    }

    process {
        foreach ($item in $Packet) {
            [uint32]$tsSec = ([DateTimeOffset]$item.TimeCreated).ToUnixTimeSeconds()
            [uint32]$tsUsec = $item.TimeCreated.Millisecond
            [uint32]$inclLen = $item.FragmentSize
            [uint32]$origLen = $inclLen

            $writer.Write($tsSec)
            $writer.Write($tsUsec)
            $writer.Write($inclLen)
            $writer.Write($origLen)
            $writer.Write($item.Fragment)
        }
    }

    end {
        $bytes = $stream.ToArray()

        $stream.Dispose()
        $writer.Dispose()

        if (-not([System.IO.Path]::IsPathRooted($Path))) {
            $Path = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Path)
        }

        [System.IO.File]::WriteAllBytes($Path, $bytes)

        if ($Invoke) {
            Invoke-Item -Path $Path
        }
    }
}