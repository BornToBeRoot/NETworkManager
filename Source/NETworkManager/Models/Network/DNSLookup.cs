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
        public bool UseCustomDNSServer = false;
        public DNSServerInfo CustomDNSServer;
        public bool AddDNSSuffix = true;
        public bool UseCustomDNSSuffix = false;
        public string CustomDNSSuffix;
        public QueryClass QueryClass = QueryClass.IN;
        public QueryType QueryType = QueryType.ANY;

        public bool UseCache = false;
        public bool Recursion = true;
        public bool UseTCPOnly = false;
        public int Retries = 3;
        public TimeSpan Timeout = new TimeSpan(2000);
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
        private List<IPEndPoint> GetDnsServer()
        {
            List<IPEndPoint> dnsServers = new List<IPEndPoint>();

            if (UseCustomDNSServer)
            {
                foreach (var dnsServer in CustomDNSServer.Servers)
                    dnsServers.Add(new IPEndPoint(IPAddress.Parse(dnsServer), CustomDNSServer.Port));
            }
            else
            {
                foreach (var dnsServer in NameServer.ResolveNameServers(true, false))
                    dnsServers.Add(dnsServer);
            }

            return dnsServers;
        }

        public void ResolveAsync(List<string> hosts)
        {
            Task.Run(() =>
            {
                // Foreach host
                foreach (var host in hosts)
                {
                    // Default
                    var name = host;

                    var dnsSuffix = string.Empty;

                    if (name.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        if (AddDNSSuffix)
                            dnsSuffix = UseCustomDNSSuffix ? CustomDNSSuffix : IPGlobalProperties.GetIPGlobalProperties().DomainName;
                    }

                    // Append dns suffix to hostname
                    if (!string.IsNullOrEmpty(dnsSuffix))
                        name += $".{dnsSuffix}";

                    Parallel.ForEach(GetDnsServer(), dnsServer =>
                    {
                        LookupClient dnsLookupClient = new LookupClient(dnsServer);
                        dnsLookupClient.UseTcpOnly = UseTCPOnly;
                        dnsLookupClient.UseCache = UseCache;
                        dnsLookupClient.Recursion = Recursion;
                        dnsLookupClient.Timeout = Timeout;
                        dnsLookupClient.Retries = Retries;

                        // PTR vs A, AAAA, CNAME etc.
                        var dnsResponse = QueryType == QueryType.PTR ? dnsLookupClient.QueryReverse(IPAddress.Parse(host)) : dnsLookupClient.Query(host, QueryType, QueryClass);

                        // If there was an error... return
                        if (dnsResponse.HasError)
                        {
                            OnLookupError(new DNSLookupErrorArgs(dnsResponse.ErrorMessage, dnsResponse.NameServer.Endpoint));
                            return;
                        }

                        // Process the results...
                        ProcessDnsQueryResponse(dnsResponse);
                    });
                }

                OnLookupComplete();
            });
        }

        private void ProcessDnsQueryResponse(IDnsQueryResponse dnsQueryResponse)
        {
            var dnsServer = dnsQueryResponse.NameServer.Endpoint;

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
