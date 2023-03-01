using DnsClient;
using DnsClient.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class DNSLookup
    {
        #region Variables        
        private readonly DNSLookupSettings _settings;
        
        private readonly IEnumerable<IPEndPoint> _dnsServers;

        private readonly string _dnsSuffix;

        private readonly bool _addSuffix;
        #endregion

        #region Constructor
        public DNSLookup(DNSLookupSettings settings, IEnumerable<ServerConnectionInfo> dnsServers = null)
        {
            _settings = settings;

            _dnsServers = GetDnsServer(dnsServers);

            _dnsSuffix = _settings.UseCustomDNSSuffix ? _settings.CustomDNSSuffix : IPGlobalProperties.GetIPGlobalProperties().DomainName;
            _addSuffix = _settings.AddDNSSuffix && !string.IsNullOrEmpty(_dnsSuffix);
        }
        #endregion

        #region Events
        public event EventHandler<DNSLookupRecordArgs> RecordReceived;

        protected virtual void OnRecordReceived(DNSLookupRecordArgs e)
        {
            RecordReceived?.Invoke(this, e);
        }

        public event EventHandler<DNSLookupErrorArgs> LookupError;

        protected virtual void OnLookupError(DNSLookupErrorArgs e)
        {
            LookupError?.Invoke(this, e);
        }

        public event EventHandler LookupComplete;

        protected virtual void OnLookupComplete()
        {
            LookupComplete?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get the DNS servers from Windows or get custom DNS servers from <see cref="DNSLookupSettings"/>.
        /// </summary>
        /// <returns>List of DNS servers as <see cref="IPEndPoint"/>.</returns>
        private IEnumerable<IPEndPoint> GetDnsServer(IEnumerable<ServerConnectionInfo> dnsServers = null)
        {
            List<IPEndPoint> servers = new();

            // Use windows dns servers
            if(dnsServers == null)
            {
                foreach (var dnsServer in NameServer.ResolveNameServers(true, false))
                    servers.Add(new IPEndPoint(IPAddress.Parse(dnsServer.Address), dnsServer.Port));
            }
            else
            {
                foreach (var dnsServer in dnsServers)
                    servers.Add(new IPEndPoint(IPAddress.Parse(dnsServer.Server), dnsServer.Port));
            }           

            return servers;
        }

        /// <summary>
        /// Append DNS suffix to hostname if not set.
        /// </summary>
        /// <param name="hosts">List of hosts</param>
        /// <returns>List of host with DNS suffix</returns>
        private IEnumerable<string> GetHostWithSuffix(IEnumerable<string> hosts)
        {
            List<string> queries = new();
            
            foreach (var host in hosts)
            {
                if (_settings.QueryType != QueryType.PTR && !host.Contains('.', StringComparison.OrdinalIgnoreCase))
                    queries.Add($"{host}.{_dnsSuffix}");
            }

            return queries;
        }

        /// <summary>
		/// Resolve hostname, fqdn or ip address.
		/// </summary>
		/// <param name="hosts">List of hostnames, FQDNs or ip addresses.</param>
		public void ResolveAsync(IEnumerable<string> hosts)
        {
            Task.Run(() =>
            {
                // Append dns suffix to hostname, if option is set, otherwiese just copy the list
                IEnumerable<string> queries = _addSuffix ? GetHostWithSuffix(hosts) : hosts;

                // Foreach dns server
                Parallel.ForEach(_dnsServers, dnsServer =>
                {
                    // Init each dns server once
                    LookupClientOptions lookupClientOptions = new(dnsServer)
                    {
                        UseTcpOnly = _settings.UseTCPOnly,
                        UseCache = _settings.UseCache,
                        Recursion = _settings.Recursion,
                        Timeout = _settings.Timeout,
                        Retries = _settings.Retries,
                    };

                    LookupClient lookupClient = new(lookupClientOptions);

                    // Foreach host
                    Parallel.ForEach(queries, query =>
                    {
                        try
                        {
                            // Resovle A, AAAA, CNAME, PTR, etc.
                            var dnsResponse = _settings.QueryType == QueryType.PTR ? lookupClient.QueryReverse(IPAddress.Parse(query)) : lookupClient.Query(query, _settings.QueryType, _settings.QueryClass);

                            // Pass the error we got from the lookup client (dns server).
                            if (dnsResponse.HasError)
                            {
                                OnLookupError(new DNSLookupErrorArgs(query, $"{dnsServer.Address}", $"{dnsServer.Address}:{dnsServer.Port}", dnsResponse.ErrorMessage));
                                return; // continue
                            }

                            if (dnsResponse.Answers.Count == 0)
                            {
                                var digAdditionalCommand = _settings.QueryType == QueryType.PTR ? " -x " : " ";
                                OnLookupError(new DNSLookupErrorArgs(query, $"{dnsServer.Address}", $"{dnsServer.Address}:{dnsServer.Port}", $"No DNS resource records received for query \"{query}\" (Query type: \"{_settings.QueryType}\") and the DNS server did not return an error. Try to check your DNS server with: dig @{dnsServer.Address}{digAdditionalCommand}{query}"));
                                return; // continue
                            }

                            // Process the results...
                            ProcessDnsAnswers(dnsResponse.Answers, dnsResponse.NameServer);
                        }
                        catch (Exception ex)
                        {
                            OnLookupError(new DNSLookupErrorArgs(query, $"{dnsServer.Address}", $"{dnsServer.Address}:{dnsServer.Port}", ex.Message));
                        }
                    });
                });

                OnLookupComplete();
            });
        }

        /// <summary>
        /// Process the dns query response.
        /// </summary>
        /// <param name="dnsQueryResponse"><see cref="IDnsQueryResponse"/> to process.</param>
        private void ProcessDnsAnswers(IEnumerable<DnsResourceRecord> answers, NameServer nameServer)
        {
            // A
            foreach (var record in answers.ARecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, $"{record.Address}", $"{nameServer.Address}", $"{nameServer.Address}:{nameServer.Port}"));

            // AAAA
            foreach (var record in answers.AaaaRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, $"{record.Address}", $"{nameServer.Address}", $"{nameServer.Address}:{nameServer.Port}"));

            // CNAME
            foreach (var record in answers.CnameRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.CanonicalName, $"{nameServer.Address}", $"{nameServer.Address}:{nameServer.Port}"));

            // MX
            foreach (var record in answers.MxRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.Exchange, $"{nameServer.Address}", $"{nameServer.Address}:{nameServer.Port}"));

            // NS
            foreach (var record in answers.NsRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.NSDName, $"{nameServer.Address}", $"{nameServer.Address}:{nameServer.Port}"));

            // PTR
            foreach (var record in answers.PtrRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.PtrDomainName, $"{nameServer.Address}", $"{nameServer.Address}:{nameServer.Port}"));

            // SOA
            foreach (var record in answers.SoaRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.MName + ", " + record.RName, $"{nameServer.Address}", $"{nameServer.Address}:{nameServer.Port}"));

            // TXT
            foreach (var record in answers.TxtRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, string.Join(", ", record.Text), $"{nameServer.Address}", $"{nameServer.Address}:{nameServer.Port}"));

            // ToDo: implement more
        }
        #endregion
    }
}
