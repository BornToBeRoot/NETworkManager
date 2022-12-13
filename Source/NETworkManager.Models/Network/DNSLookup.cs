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
        private DNSLookupSettings _settings;
        #endregion

        #region Constructor
        public DNSLookup(DNSLookupSettings settings)
        {
            _settings = settings;
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
                {
                    dnsServers.Add(new IPEndPoint(IPAddress.Parse(dnsServer.Address), dnsServer.Port));
                }
            }

            return dnsServers;
        }

        /// <summary>
		/// Resolve hostname, fqdn or ip address.
		/// </summary>
		/// <param name="hosts">List of hostnames, fqdns or ip addresses.</param>
		public void ResolveAsync(IEnumerable<string> hosts)
        {
            Task.Run(() =>
            {
                // Get list of dns servers
                IEnumerable<IPEndPoint> dnsServers = GetDnsServer();

                // Foreach host
                foreach (var host in hosts)
                {
                    var query = host;

                    // Append dns suffix to hostname
                    if (_settings.QueryType != QueryType.PTR && _settings.AddDNSSuffix && query.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        var dnsSuffix = _settings.UseCustomDNSSuffix ? _settings.CustomDNSSuffix : IPGlobalProperties.GetIPGlobalProperties().DomainName;

                        if (!string.IsNullOrEmpty(dnsSuffix))
                            query += $".{dnsSuffix}";
                    }

                    // Foreach dns server
                    Parallel.ForEach(dnsServers, dnsServer =>
                    {
                        LookupClientOptions lookupClientOptions = new(dnsServer)
                        {
                            UseTcpOnly = _settings.UseTCPOnly,
                            UseCache = _settings.UseCache,
                            Recursion = _settings.Recursion,
                            Timeout = _settings.Timeout,
                            Retries = _settings.Retries,
                        };

                        LookupClient dnsLookupClient = new(lookupClientOptions);

                        try
                        {
                            // PTR vs A, AAAA, CNAME etc.
                            var dnsResponse = _settings.QueryType == QueryType.PTR ? dnsLookupClient.QueryReverse(IPAddress.Parse(query)) : dnsLookupClient.Query(query, _settings.QueryType, _settings.QueryClass);

                            // If there was an error... return
                            if (dnsResponse.HasError)
                            {
                                OnLookupError(new DNSLookupErrorArgs(dnsResponse.ErrorMessage, new IPEndPoint(IPAddress.Parse(dnsResponse.NameServer.Address), dnsResponse.NameServer.Port)));
                                return;
                            }

                            // Process the results...
                            ProcessDnsQueryResponse(dnsResponse);
                        }
                        catch (Exception ex)
                        {
                            OnLookupError(new DNSLookupErrorArgs(ex.Message, dnsServer));
                        }
                    });
                }

                OnLookupComplete();
            });
        }

        /// <summary>
        /// Process the dns query response.
        /// </summary>
        /// <param name="dnsQueryResponse"><see cref="IDnsQueryResponse"/> to process.</param>
        private void ProcessDnsQueryResponse(IDnsQueryResponse dnsQueryResponse)
        {
            var dnsServer = new IPEndPoint(IPAddress.Parse(dnsQueryResponse.NameServer.Address), dnsQueryResponse.NameServer.Port);

            // A
            foreach (var record in dnsQueryResponse.Answers.ARecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.Address.ToString(), dnsServer));

            // AAAA
            foreach (var record in dnsQueryResponse.Answers.AaaaRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.Address.ToString(), dnsServer));

            // CNAME
            foreach (var record in dnsQueryResponse.Answers.CnameRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.CanonicalName, dnsServer));

            // MX
            foreach (var record in dnsQueryResponse.Answers.MxRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.Exchange, dnsServer));

            // NS
            foreach (var record in dnsQueryResponse.Answers.NsRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.NSDName, dnsServer));

            // PTR
            foreach (var record in dnsQueryResponse.Answers.PtrRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.PtrDomainName, dnsServer));

            // SOA
            foreach (var record in dnsQueryResponse.Answers.SoaRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, record.MName + ", " + record.RName, dnsServer));

            // TXT
            foreach (var record in dnsQueryResponse.Answers.TxtRecords())
                OnRecordReceived(new DNSLookupRecordArgs(record.DomainName, record.TimeToLive, record.RecordClass, record.RecordType, string.Join(", ", record.Text), dnsServer));

            // ToDo: implement more
        }
        #endregion
    }
}
