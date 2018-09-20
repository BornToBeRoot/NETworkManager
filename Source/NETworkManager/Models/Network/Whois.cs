using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class Whois
    {
        public string Query(string domain, string whoisServer)
        {
            var tcpClient = new TcpClient(whoisServer, 43);

            var networkStream = tcpClient.GetStream();

            var bufferedStream = new BufferedStream(networkStream);

            var streamWriter = new StreamWriter(bufferedStream);

            streamWriter.WriteLine(domain);
            streamWriter.Flush();

            var streamReader =  new StreamReader(bufferedStream);

            var stringBuilder = new StringBuilder();
            
            while (!streamReader.EndOfStream)
                stringBuilder.AppendLine(streamReader.ReadLine());

            return stringBuilder.ToString();
        }
    }
}
