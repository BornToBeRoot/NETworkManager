using Heijden.DNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TransportType = Heijden.DNS.TransportType;

namespace NETworkManager.Models.Network
{
    public class DNSLookup
    {
        #region Variables
        public bool UseCustomDNSServer = false;
        public List<string> CustomDNSServers = new List<string> { "1.1.1.1", "1.0.0.1" };
        public int Port = 53;
        public bool AddDNSSuffix = true;
        public bool UseCustomDNSSuffix = false;
        public string CustomDNSSuffix;
        public QClass Class = QClass.IN;
        public QType Type = QType.ANY;
        public bool Recursion = true;
        public bool UseResolverCache = false;
        public TransportType TransportType = TransportType.Udp;
        public int Attempts = 3;
        public int Timeout = 2000;
        public bool ResolveCNAME = true;

        private readonly Resolver _dnsResolver = new Resolver();
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
        public Task<Tuple<IPAddress, List<string>>> ResolvePTRAsync(IPAddress host)
        {
            return Task.Run(() => ResolvePTR(host));
        }

        public Tuple<IPAddress, List<string>> ResolvePTR(IPAddress host)
        {
            // DNS server list
            var dnsServers = new List<string>();

            if (UseCustomDNSServer)
                CustomDNSServers.ForEach(x => dnsServers.Add(x));
            else // Use windows default dns server, but filter/remove IPv6 site local addresses (fec0:0:0:0:ffff::)
                _dnsResolver.DnsServers.Where(x => !x.Address.ToString().StartsWith(@"fec0")).Select(y => y.Address.ToString()).ToList().ForEach(x => dnsServers.Add(x));

            // PTR
            var name = Resolver.GetArpaFromIp(host);

            var port = UseCustomDNSServer ? Port : Resolver.DefaultPort;

            var dnsServerIPAddress = string.Empty;
            var ptrResults = new List<string>();

            foreach (var dnsServer in dnsServers)
            {
                dnsServerIPAddress = dnsServer;

                // Create a new for each request
                var resolver = new Resolver(dnsServer, port)
                {
                    Recursion = Recursion,
                    TransportType = TransportType,
                    UseCache = UseResolverCache,
                    Retries = Attempts,
                    TimeOut = Timeout
                };

                var dnsResponse = resolver.Query(name, QType.PTR);

                if (!string.IsNullOrEmpty(dnsResponse.Error)) // On error... try next dns server...
                    continue;

                // PTR
                ptrResults.AddRange(dnsResponse.RecordsPTR.Select(r => r.PTRDNAME));

                // If we got results, break... else --> check next dns server
                if (ptrResults.Count > 0)
                    break;
            }

            return new Tuple<IPAddress, List<string>>(IPAddress.Parse(dnsServerIPAddress), ptrResults);
        }

        public void ResolveAsync(List<string> hosts)
        {
            Task.Run(() =>
            {
                // DNS server list
                var dnsServers = new List<string>();

                if (UseCustomDNSServer)
                    CustomDNSServers.ForEach(x => dnsServers.Add(x));
                else // Use windows default dns server, but filter/remove IPv6 site local addresses (fec0:0:0:0:ffff::)
                    _dnsResolver.DnsServers.Where(x => !x.Address.ToString().StartsWith(@"fec0")).Select(y => y.Address.ToString()).ToList().ForEach(x => dnsServers.Add(x));

                // Foreach host
                foreach (var host in hosts)
                {
                    // Default
                    var name = host;

                    var dnsSuffix = string.Empty;

                    if (name.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        if (AddDNSSuffix)
                        {
                            dnsSuffix = UseCustomDNSSuffix ? CustomDNSSuffix : IPGlobalProperties.GetIPGlobalProperties().DomainName;
                        }
                    }

                    // Append dns suffix to hostname
                    if (!string.IsNullOrEmpty(dnsSuffix))
                        name += $".{dnsSuffix}";

                    switch (Type)
                    {
                        // PTR
                        case QType.PTR:
                            if (IPAddress.TryParse(name, out IPAddress ip))
                                name = Resolver.GetArpaFromIp(ip);
                            break;
                        // NAPTR
                        case QType.NAPTR:
                            name = Resolver.GetArpaFromEnum(name);
                            break;
                    }

                    var port = UseCustomDNSServer ? Port : Resolver.DefaultPort;

                    Parallel.ForEach(dnsServers, dnsServer =>
                    {
                        // Create a new for each request
                        var resolver = new Resolver(dnsServer, port)
                        {
                            Recursion = Recursion,
                            TransportType = TransportType,
                            UseCache = UseResolverCache,
                            Retries = Attempts,
                            TimeOut = Timeout
                        };

                        var dnsResponse = resolver.Query(name, Type, Class);

                        // If there was an error... return
                        if (!string.IsNullOrEmpty(dnsResponse.Error))
                        {
                            OnLookupError(new DNSLookupErrorArgs(dnsResponse.Error, resolver.DnsServer));
                            return;
                        }

                        // Process the results...
                        ProcessResponse(dnsResponse);

                        // If we get a CNAME back (from an ANY result), do a second request and try to get the A, AAAA etc... 
                        if (!ResolveCNAME || Type != QType.ANY)
                            return;

                        foreach (var record in dnsResponse.RecordsCNAME)
                        {
                            var dnsResponse2 = resolver.Query(record.CNAME, Type, Class);

                            if (!string.IsNullOrEmpty(dnsResponse2.Error))
                            {
                                OnLookupError(new DNSLookupErrorArgs(dnsResponse2.Error, resolver.DnsServer));
                                continue;
                            }

                            ProcessResponse(dnsResponse2);
                        }
                    });
                }

                OnLookupComplete();
            });
        }

        private void ProcessResponse(Response dnsResponse)
        {
            var dnsServer = dnsResponse.Server.Address.ToString();
            var port = dnsResponse.Server.Port;

            // A
            foreach (var r in dnsResponse.RecordsA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // AAAA
            foreach (var r in dnsResponse.RecordsAAAA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // CNAME
            foreach (var r in dnsResponse.RecordsCNAME)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // MX
            foreach (var r in dnsResponse.RecordsMX)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // NS
            foreach (var r in dnsResponse.RecordsNS)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // PTR
            foreach (var r in dnsResponse.RecordsPTR)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // SOA
            foreach (var r in dnsResponse.RecordsSOA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // TXT
            foreach (var r in dnsResponse.RecordsTXT)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));
        }
        #endregion
    }
}
