using DnsClient;
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
        public DNSLookup(DNSLookupSettings settings)
        {
            _settings = settings;

            _dnsServers = GetDnsServer();

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
        private IEnumerable<IPEndPoint> GetDnsServer()
        {
            List<IPEndPoint> dnsServers = new();

            if (_settings.UseCustomDNSServer)
            {
                foreach (var dnsServer in _settings.CustomDNSServer.Servers)
                    dnsServers.Add(new IPEndPoint(IPAddress.Parse(dnsServer), _settings.CustomDNSServer.Port));
            }
            else
            {
                foreach (var dnsServer in NameServer.ResolveNameServers(true, false))
                    dnsServers.Add(new IPEndPoint(IPAddress.Parse(dnsServer.Address), dnsServer.Port));
            }

            return dnsServers;
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
                            // PTR vs A, AAAA, CNAME etc.
                            var dnsResponse = _settings.QueryType == QueryType.PTR ? lookupClient.QueryReverse(IPAddress.Parse(query)) : lookupClient.Query(query, _settings.QueryType, _settings.QueryClass);

                            // If there was an error... return
                            if (dnsResponse.HasError)
                            {
                                OnLookupError(new DNSLookupErrorArgs($"{dnsServer.Address}", $"{dnsServer.Address}:{dnsServer.Port}", dnsResponse.ErrorMessage));
                                return; // continue
                            }

                            // Process the results...
                            ProcessDnsQueryResponse(dnsResponse);
                        }
                        catch (Exception ex)
                        {
                            OnLookupError(new DNSLookupErrorArgs($"{dnsServer.Address}", $"{dnsServer.Address}:{dnsServer.Port}", ex.Message));
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
        private void ProcessDnsQueryResponse(IDnsQueryResponse dnsQueryResponse)
        {
            // A
            foreach (var record in dnsQueryResponse.Answers.ARecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, $"{record.Address}", $"{dnsQueryResponse.NameServer.Address}", $"{dnsQueryResponse.NameServer.Address}:{dnsQueryResponse.NameServer.Port}"));

            // AAAA
            foreach (var record in dnsQueryResponse.Answers.AaaaRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, $"{record.Address}", $"{dnsQueryResponse.NameServer.Address}", $"{dnsQueryResponse.NameServer.Address}:{dnsQueryResponse.NameServer.Port}"));

            // CNAME
            foreach (var record in dnsQueryResponse.Answers.CnameRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.CanonicalName, $"{dnsQueryResponse.NameServer.Address}", $"{dnsQueryResponse.NameServer.Address}:{dnsQueryResponse.NameServer.Port}"));

            // MX
            foreach (var record in dnsQueryResponse.Answers.MxRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.Exchange, $"{dnsQueryResponse.NameServer.Address}", $"{dnsQueryResponse.NameServer.Address}:{dnsQueryResponse.NameServer.Port}"));

            // NS
            foreach (var record in dnsQueryResponse.Answers.NsRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.NSDName, $"{dnsQueryResponse.NameServer.Address}", $"{dnsQueryResponse.NameServer.Address}:{dnsQueryResponse.NameServer.Port}"));

            // PTR
            foreach (var record in dnsQueryResponse.Answers.PtrRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.PtrDomainName, $"{dnsQueryResponse.NameServer.Address}", $"{dnsQueryResponse.NameServer.Address}:{dnsQueryResponse.NameServer.Port}"));

            // SOA
            foreach (var record in dnsQueryResponse.Answers.SoaRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.MName + ", " + record.RName, $"{dnsQueryResponse.NameServer.Address}", $"{dnsQueryResponse.NameServer.Address}:{dnsQueryResponse.NameServer.Port}"));

            // TXT
            foreach (var record in dnsQueryResponse.Answers.TxtRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, string.Join(", ", record.Text), $"{dnsQueryResponse.NameServer.Address}", $"{dnsQueryResponse.NameServer.Address}:{dnsQueryResponse.NameServer.Port}"));

            // ToDo: implement more
        }
        #endregion
    }
}
