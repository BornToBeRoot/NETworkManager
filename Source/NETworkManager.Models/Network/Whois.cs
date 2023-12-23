using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NETworkManager.Models.Network;

public static class Whois
{
    #region Constructor

    static Whois()
    {
        var document = new XmlDocument();
        document.Load(WhoisServerFilePath);

        var whoisServerList = (from XmlNode node in document.SelectNodes("/WhoisServers/WhoisServer")!
            where node != null
            select new WhoisServerInfo(node.SelectSingleNode("Server")?.InnerText,
                node.SelectSingleNode("TLD")?.InnerText)).ToList();

        WhoisServers = (Lookup<string, WhoisServerInfo>)whoisServerList.ToLookup(x => x.Tld);
    }

    #endregion

    #region Variables

    private static readonly string WhoisServerFilePath =
        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!, "Resources", "WhoisServers.xml");

    private static readonly Lookup<string, WhoisServerInfo> WhoisServers;

    #endregion

    #region Methods

    public static Task<string> QueryAsync(string domain, string whoisServer)
    {
        return Task.Run(() => Query(domain, whoisServer));
    }

    private static string Query(string domain, string whoisServer)
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
        var domainParts = domain.TrimEnd('.').Split('.');

        // TLD to upper because the lookup is case sensitive
        return WhoisServers[domainParts[^1].ToUpper()].FirstOrDefault()?.Server;
    }

    #endregion
}