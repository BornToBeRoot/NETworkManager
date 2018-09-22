using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class Whois
    {
        public Task<string> QueryAsync(string domain, string whoisServer)
        {
            return Task.Run(() => Query(domain, whoisServer));
        }

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

        public static string GetWhoisServer(string domain)
        {
            var domainParts = domain.Split('.');

            var server = string.Empty;

            switch (domainParts[domainParts.Length - 1])
            {
                case "de":
                    server = "de";

                    break;
            }

            return server;
        }
    }
}
