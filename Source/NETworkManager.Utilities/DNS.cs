using DnsClient;
using System;
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
        private DNSSettings _settings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private LookupClient _client { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(DNSSettings settings = null)
        {
            _settings = settings;

            if (_settings == null)
            {
                UpdateFromWindows();
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
        public void UpdateFromWindows()
        {
            // Default (Windows) settings
            if (_settings == null)
            {
                _client = new LookupClient();

                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<DNSResultIPAddress> ResolveAAsync(string query)
        {
            try
            {
                var result = await _client.QueryAsync(query, QueryType.A);

                if (result.HasError)
                    return new DNSResultIPAddress(result.HasError, $"{result.NameServer}: {result.ErrorMessage}");

                return new DNSResultIPAddress(result.Answers.ARecords().FirstOrDefault().Address);
            }
            catch (DnsResponseException ex)
            {
                return new DNSResultIPAddress(true, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<DNSResultIPAddress> ResolveAaaaAsync(string query)
        {

            try
            {
                var result = await _client.QueryAsync(query, QueryType.AAAA);

                if (result.HasError)
                    return new DNSResultIPAddress(result.HasError, $"{result.NameServer}: {result.ErrorMessage}");

                return new DNSResultIPAddress(result.Answers.AaaaRecords().FirstOrDefault().Address);
            }
            catch (DnsResponseException ex)
            {
                return new DNSResultIPAddress(true, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<DNSResultString> ResolveCnameAsync(string query)
        {
            try
            {
                var result = await _client.QueryAsync(query, QueryType.CNAME);

                if (result.HasError)
                    return new DNSResultString(result.HasError, $"{result.NameServer}: {result.ErrorMessage}");

                return new DNSResultString(result.Answers.CnameRecords().FirstOrDefault().CanonicalName);
            }
            catch (DnsResponseException ex)
            {
                return new DNSResultString(true, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<DNSResultString> ResolvePtrAsync(IPAddress ipAddress)
        {
            try
            {
                var result = await _client.QueryReverseAsync(ipAddress);

                if (result.HasError)
                    return new DNSResultString(result.HasError, $"{result.NameServer}: {result.ErrorMessage}");

                return new DNSResultString(result.Answers.PtrRecords().FirstOrDefault().PtrDomainName);
            }
            catch (DnsResponseException ex)
            {
                return new DNSResultString(true, ex.Message);
            }
        }
    }
}
