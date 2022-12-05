using DnsClient;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NETworkManager.Utilities
{
    public class DNS : SingletonBase<DNS>
    {
        /// <summary>
        /// 
        /// </summary>
        DNSSettings _settings;

        /// <summary>
        /// 
        /// </summary>
        LookupClient _client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(DNSSettings settings = null)
        {
            _settings = settings;

            Update();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            // Default settings
            if (_settings == null)
            {
                _client = new LookupClient();

                return;
            }

            // Custom DNS servers
            List<NameServer> servers = new();

            foreach (string server in _settings.DNSServers)
                servers.Add(new IPEndPoint(IPAddress.Parse(server), 53));

            LookupClientOptions options = new(servers.ToArray())
            {

            };

            _client = new LookupClient(options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IPAddress> ResolveAAsync(string query)
        {
            var result = await _client.QueryAsync(query, QueryType.A);

            return result.Answers.ARecords().FirstOrDefault().Address;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IPAddress> ResolveAaaaAsync(string query)
        {
            var result = await _client.QueryAsync(query, QueryType.AAAA);

            return result.Answers.AaaaRecords().FirstOrDefault().Address;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<string> ResolveCnameAsync(string query)
        {
            var result = await _client.QueryAsync(query, QueryType.CNAME);

            return result.Answers.CnameRecords().FirstOrDefault().CanonicalName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<string> ResolvePtrAsync(IPAddress ipAddress)
        {
            var result = await _client.QueryReverseAsync(ipAddress);

            return result.Answers.PtrRecords().FirstOrDefault().PtrDomainName;
        }
    }
}
