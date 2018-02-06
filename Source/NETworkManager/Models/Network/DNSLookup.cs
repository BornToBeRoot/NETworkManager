using Heijden.DNS;
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
        Resolver dnsResolver = new Resolver();
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
        public void LookupAsync(List<string> hosts, DNSLookupOptions dnsLookupOptions)
        {
            Task.Run(() =>
            {
                // DNS server list
                List<string> dnsServers = new List<string>();

                if (dnsLookupOptions.UseCustomDNSServer)
                    dnsLookupOptions.CustomDNSServers.ForEach(x => dnsServers.Add(x));
                else // Use windows default dns server, but filter/remove IPv6 site local addresses (fec0:0:0:0:ffff::)
                    dnsResolver.DnsServers.Where(x => !x.Address.ToString().StartsWith(@"fec0")).Select(y => y.Address.ToString()).ToList().ForEach(x => dnsServers.Add(x));

                // Foreach host
                foreach (string host in hosts)
                {
                    // Default
                    string name = host.Trim();

                    string dnsSuffix = string.Empty;

                    if (name.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        if (dnsLookupOptions.AddDNSSuffix)
                        {
                            if (dnsLookupOptions.UseCustomDNSSuffix)
                                dnsSuffix = dnsLookupOptions.CustomDNSSuffix;
                            else
                                dnsSuffix = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                        }
                    }

                    // Append dns suffix to hostname
                    if (!string.IsNullOrEmpty(dnsSuffix))
                        name += string.Format(".{0}", dnsSuffix);

                    // PTR
                    if (dnsLookupOptions.Type == QType.PTR)
                    {
                        if (IPAddress.TryParse(name, out IPAddress ip))
                            name = Resolver.GetArpaFromIp(ip);
                    }

                    // NAPTR
                    if (dnsLookupOptions.Type == QType.NAPTR)
                        name = Resolver.GetArpaFromEnum(name);

                    int port = dnsLookupOptions.UseCustomDNSServer ? dnsLookupOptions.Port : Resolver.DefaultPort;

                    Parallel.ForEach(dnsServers, dnsServer =>
                    {
                        // Create a new for each request
                        Resolver resolver = new Resolver(dnsServer, port)
                        {
                            Recursion = dnsLookupOptions.Recursion,
                            TransportType = dnsLookupOptions.TransportType,
                            Retries = dnsLookupOptions.Attempts,
                            TimeOut = dnsLookupOptions.Timeout
                        };

                        Response dnsResponse = resolver.Query(name, dnsLookupOptions.Type, dnsLookupOptions.Class);

                        // If there was an error... return
                        if (!string.IsNullOrEmpty(dnsResponse.Error))
                        {
                            // OnLookupError(new DNSLookupErrorArgs(dnsResponse.Error, resolver.DnsServer));
                            // return;
                        }

                        // Process the results...
                        ProcessResponse(dnsResponse);

                        // If we get a CNAME back (from a result), do a second request and try to get the A, AAAA etc... 
                        if (dnsLookupOptions.ResolveCNAME && dnsLookupOptions.Type != QType.CNAME)
                        {
                            foreach (RecordCNAME r in dnsResponse.RecordsCNAME)
                            {
                                Response dnsResponse2 = resolver.Query(r.CNAME, dnsLookupOptions.Type, dnsLookupOptions.Class);

                                if (!string.IsNullOrEmpty(dnsResponse2.Error))
                                {
                                    OnLookupError(new DNSLookupErrorArgs(dnsResponse2.Error, resolver.DnsServer));
                                    continue;
                                }

                                ProcessResponse(dnsResponse2);
                            }
                        }
                    });
                }

                OnLookupComplete();
            });
        }

        private void ProcessResponse(Response dnsResponse)
        {
            string dnsServer = dnsResponse.Server.Address.ToString();
            int port = dnsResponse.Server.Port;

            // A
            foreach (RecordA r in dnsResponse.RecordsA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // AAAA
            foreach (RecordAAAA r in dnsResponse.RecordsAAAA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // CNAME
            foreach (RecordCNAME r in dnsResponse.RecordsCNAME)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // MX
            foreach (RecordMX r in dnsResponse.RecordsMX)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // NS
            foreach (RecordNS r in dnsResponse.RecordsNS)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // PTR
            foreach (RecordPTR r in dnsResponse.RecordsPTR)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // SOA
            foreach (RecordSOA r in dnsResponse.RecordsSOA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));

            // TXT
            foreach (RecordTXT r in dnsResponse.RecordsTXT)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r, dnsServer, port));
        }
        #endregion
    }
}
