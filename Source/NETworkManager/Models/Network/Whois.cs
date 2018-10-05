using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NETworkManager.Models.Settings;

namespace NETworkManager.Models.Network
{
    public class Whois
    {
        #region Variables
        private static readonly string WhoisServerFilePath =
            Path.Combine(ConfigurationManager.Current.ExecutionPath, "Resources", "WhoisServers.xml");

        private static readonly List<WhoisServerInfo> WhoisServerList;
        private static readonly Lookup<string, WhoisServerInfo> WhoisServers;
        #endregion

        #region Constructor

        static Whois()
        {
            var document = new XmlDocument();
            document.Load(WhoisServerFilePath);

            WhoisServerList = new List<WhoisServerInfo>();

            foreach (XmlNode node in document.SelectNodes("/WhoisServers/WhoisServer"))
            {
                if (node == null)
                    continue;

                WhoisServerList.Add(new WhoisServerInfo(node.SelectSingleNode("Server")?.InnerText, node.SelectSingleNode("TLD")?.InnerText));
            }

            WhoisServers = (Lookup<string, WhoisServerInfo>)WhoisServerList.ToLookup(x => x.Tld);
        }
        #endregion

        #region Methods
        public static Task<string> QueryAsync(string domain, string whoisServer)
        {
            return Task.Run(() => Query(domain, whoisServer));
        }

        public static string Query(string domain, string whoisServer)
        {
            var tcpClient = new TcpClient(whoisServer, 43);

            var networkStream = tcpClient.GetStream();

            var bufferedStream = new BufferedStream(networkStream);

            var streamWriter = new StreamWriter(bufferedStream);

            streamWriter.WriteLine(domain);
            streamWriter.Flush();

            var streamReader = new StreamReader(bufferedStream);

            var stringBuilder = new StringBuilder();

            while (!streamReader.EndOfStream)
                stringBuilder.AppendLine(streamReader.ReadLine());

            return stringBuilder.ToString();
        }

        public static string GetWhoisServer(string domain)
        {
            var domainParts = domain.Split('.');

            // TLD to upper because the lookup is case sensitive
            return WhoisServers[domainParts[domainParts.Length - 1].ToUpper()].FirstOrDefault()?.Server;
        }
        #endregion
    }
}
