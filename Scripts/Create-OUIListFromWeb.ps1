# Filepath in the resources...
[string]$OutFilePath = Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "Source\NETworkManager.Models\Resources\OUI.xml"

# Download cleanup IEEE oui version from linuxnet
$LatestOUIs = (Invoke-WebRequest -Uri "http://linuxnet.ca/ieee/oui.txt").Content

# Create xml document
[xml]$Document = New-Object System.Xml.XmlDocument
$Declaration = $Document.CreateXmlDeclaration("1.0", "UTF-8", $null)

[void]$Document.AppendChild($Declaration)

# Description
$Description = @"
Organizationally unique identifier
Generated $(Get-Date)
"@

[void]$Document.AppendChild($Document.CreateComment($Description))

# Root node
$RootNode = $Document.CreateNode("element", "OUIs", $null)

foreach($Line in $LatestOUIs -split '[\r\n]')
{
    if($Line -match "^[A-F0-9]{6}")
    {   
        # Line looks like: 2405F5     (base 16)		Integrated Device Technology (Malaysia) Sdn. Bhd.
        $OUIData = ($Line -replace '\s+', ' ').Replace(' (base 16) ', '|').Trim().Split('|')
                        
        $OUINode = $Document.CreateNode("element", "OUI", $null)

        $MACAddressElement = $Document.CreateElement("MACAddress")
        $MACAddressElement.InnerText = $OUIData[0]
        [void]$OUINode.AppendChild($MACAddressElement)

        $VendorElement = $Document.CreateElement("Vendor")
        $VendorElement.InnerText = $OUIData[1]
        [void]$OUINode.AppendChild($VendorElement)

        [void]$RootNode.AppendChild($OUINode)
    }
}

[void]$Document.AppendChild($RootNode)
$Document.Save($OutFilePath)