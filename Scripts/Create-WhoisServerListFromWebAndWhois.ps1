# Filepath in the resources
[string]$OutFilePath = Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "Source\NETworkManager\Resources\WhoisServers.xml"

$IANA_TLDs = (Invoke-WebRequest -Uri "https://data.iana.org/TLD/tlds-alpha-by-domain.txt").Content

# Create xml document
[xml]$Document = New-Object System.Xml.XmlDocument
$Declaration = $Document.CreateXmlDeclaration("1.0", "UTF-8", $null)

[void]$Document.AppendChild($Declaration)

# Description
$Description = @"
Whois servers by domain from IANA
Generated $(Get-Date)
"@

[void]$Document.AppendChild($Document.CreateComment($Description))

# Root node
$RootNode = $Document.CreateNode("element", "Ports", $null)

foreach($Tld in ($IANA_TLDs -split "[`r|`n]"))
{
    $trimTld = $Tld.Trim()

    if($trimTld.StartsWith('#') -or [String]::IsNullOrEmpty($trimTld))
    {
        continue
    }    
        
    $tcpClient = New-Object System.Net.Sockets.TcpClient("whois.iana.org", 43)

    $networkStream= $tcpClient.GetStream()

    $bufferedStream = New-Object System.IO.BufferedStream($networkStream)

    $streamWriter = New-Object System.IO.StreamWriter($bufferedStream)

    $streamWriter.WriteLine("de")
    $streamWriter.Flush()

    $streamReader = New-Object System.IO.StreamReader($bufferedStream)

    $stringBuilder = New-Object System.Text.StringBuilder

    while(!$streamReader.EndOfStream)
    {
        $stringBuilder.Append($streamReader.ReadLine())
    }

    $WhoisServer = (($stringBuilder.ToString() -split "whois:")[1] -split "status:")[0].Trim()
    
    $WhoisServerNode = $Document.CreateNode("element", "WhoisServer", $null)
    
    $TldElement = $Document.CreateElement("TLD")
    $TldElement.InnerText = $tld
    [void]$WhoisServerNode.AppendChild($TldElement)

    $ServerElement = $Document.CreateElement("Server")
    $ServerElement.InnerText = $WhoisServer
    [void]$WhoisServerNode.AppendChild($ServerElement)

    [void]$RootNode.AppendChild($WhoisServerNode)
}           

[void]$Document.AppendChild($RootNode)
$Document.Save($OutFilePath)