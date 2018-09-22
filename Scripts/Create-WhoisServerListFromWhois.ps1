$tcpClient = New-Object System.Net.Sockets.TcpClient("whois.iana.org", 43)

$networkStream= $tcpClient.GetStream()

$bufferedStream = New-Object System.IO.BufferedStream($networkStream)

$streamWriter = New-Object System.IO.StreamWriter($bufferedStream)

$streamWriter.WriteLine(".de")
$streamWriter.Flush()

$streamReader = New-Object System.IO.StreamReader($bufferedStream)

$stringBuilder = New-Object System.Text.StringBuilder

while(!$streamReader.EndOfStream)
{
    $stringBuilder.Append($streamReader.ReadLine())
}

$stringBuilder.ToString()


