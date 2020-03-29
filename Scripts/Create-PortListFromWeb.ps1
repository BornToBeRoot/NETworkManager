# Filepath in the resources
[string]$OutFilePath = Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "Source\NETworkManager.Models\Resources\Ports.xml"

# Download from IANA...
[xml]$ServiceNamePortNumbers = (Invoke-WebRequest -Uri "https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xml").Content

# Create xml document
[xml]$Document = New-Object System.Xml.XmlDocument
$Declaration = $Document.CreateXmlDeclaration("1.0", "UTF-8", $null)

[void]$Document.AppendChild($Declaration)

# Description
$Description = @"
Service Name and Transport Protocol Port Number Registry
Generated $(Get-Date)
"@

[void]$Document.AppendChild($Document.CreateComment($Description))

# Root node
$RootNode = $Document.CreateNode("element", "Ports", $null)

# Create node for each port
foreach($Record in $ServiceNamePortNumbers.ChildNodes.record)
{
    if([string]::IsNullOrEmpty($Record.number) -or ([string]::IsNullOrEmpty($Record.protocol)))
    {        
        continue   
    }       

    $Description = ($Record.description -replace '`n','') -replace '\s+',' '
    $Number = $Record.number

    if($Number -like "*-*")
    {
        $NumberArr = $Number.Split('-')

        foreach($Number1 in $NumberArr[0]..$NumberArr[1])
        {      
            $PortNode = $Document.CreateNode("element", "Port", $null)
            
            $NumberElement = $Document.CreateElement("Number")
            $NumberElement.InnerText = $Number1
            [void]$PortNode.AppendChild($NumberElement)

            $ProtocolElement = $Document.CreateElement("Protocol")
            $ProtocolElement.InnerText = $record.protocol
            [void]$PortNode.AppendChild($ProtocolElement)

            $NumberName = $Document.CreateElement("Name")
            $NumberName.InnerText = $Record.name
            [void]$PortNode.AppendChild($NumberName)

            $NumberDescription = $Document.CreateElement("Description")
            $NumberDescription.InnerText = $Description
            [void]$PortNode.AppendChild($NumberDescription)

            [void]$RootNode.AppendChild($PortNode)
        }
    }
    else 
    {
        $PortNode = $Document.CreateNode("element", "Port", $null)
        
        $NumberElement = $Document.CreateElement("Number")
        $NumberElement.InnerText = $Number
        [void]$PortNode.AppendChild($NumberElement)

        $ProtocolElement = $Document.CreateElement("Protocol")
        $ProtocolElement.InnerText = $record.protocol
        [void]$PortNode.AppendChild($ProtocolElement)

        $NumberName = $Document.CreateElement("Name")
        $NumberName.InnerText = $Record.name
        [void]$PortNode.AppendChild($NumberName)

        $NumberDescription = $Document.CreateElement("Description")
        $NumberDescription.InnerText = $Description
        [void]$PortNode.AppendChild($NumberDescription)

        [void]$RootNode.AppendChild($PortNode)
    }    
}          

[void]$Document.AppendChild($RootNode)
$Document.Save($OutFilePath)