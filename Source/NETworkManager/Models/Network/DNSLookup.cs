using Heijden.DNS;
using System;
using System.Net;
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
        public void LookupAsync(string hostnameOrIPAddress, DNSLookupOptions dnsLookupOptions)
        {
            Task.Run(() =>
            {
                // This will convert the query to an Arpa request
                if (dnsLookupOptions.Type == QType.PTR)
                {
                    IPAddress ip;
                    if (IPAddress.TryParse(hostnameOrIPAddress, out ip))
                        hostnameOrIPAddress = Resolver.GetArpaFromIp(ip);
                }

                if (dnsLookupOptions.Type == QType.NAPTR)
                    hostnameOrIPAddress = Resolver.GetArpaFromEnum(hostnameOrIPAddress);

                if (dnsLookupOptions.UseCustomDNSServer)
                    dnsResolver.DnsServer = dnsLookupOptions.CustomDNSServer;
                
                dnsResolver.Recursion = dnsLookupOptions.Recursion;
                dnsResolver.TransportType = dnsLookupOptions.TransportType;
                dnsResolver.Retries = dnsLookupOptions.Attempts;
                dnsResolver.TimeOut = dnsLookupOptions.Timeout;

                Response dnsResponse = dnsResolver.Query(hostnameOrIPAddress, dnsLookupOptions.Type, dnsLookupOptions.Class);

                // If there was an error... return
                if (!string.IsNullOrEmpty(dnsResponse.Error))
                {
                    OnLookupError(new DNSLookupErrorArgs(dnsResponse.Error, dnsResolver.DnsServer));
                    return;
                }
                
                // Process the results...
                ProcessResponse(dnsResponse);

                // If we get a CNAME back (from a result), do a second request and try to get the A, AAAA etc... 
                if(dnsLookupOptions.Type != QType.CNAME)
                {
                    foreach (RecordCNAME r in dnsResponse.RecordsCNAME)
                    {
                        Response dnsResponse2 = dnsResolver.Query(r.CNAME, dnsLookupOptions.Type, dnsLookupOptions.Class);

                        if(!string.IsNullOrEmpty(dnsResponse2.Error))
                        {
                            OnLookupError(new DNSLookupErrorArgs(dnsResponse2.Error, dnsResolver.DnsServer));
                            continue;
                        }

                        ProcessResponse(dnsResponse2);
                    }
                }

                OnLookupComplete();
            });
        }

        private void ProcessResponse(Response dnsResponse)
        {
            // A
            foreach (RecordA r in dnsResponse.RecordsA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r.ToString().TrimEnd()));

            // AAAA
            foreach (RecordAAAA r in dnsResponse.RecordsAAAA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r.ToString().TrimEnd()));

            // CNAME
            foreach (RecordCNAME r in dnsResponse.RecordsCNAME)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r.ToString().TrimEnd()));

            // MX
            foreach (RecordMX r in dnsResponse.RecordsMX)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r.ToString().TrimEnd()));

            // NS
            foreach (RecordNS r in dnsResponse.RecordsNS)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r.ToString().TrimEnd()));

            // PTR
            foreach (RecordPTR r in dnsResponse.RecordsPTR)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r.ToString().TrimEnd()));

            // SOA
            foreach (RecordSOA r in dnsResponse.RecordsSOA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r.ToString().TrimEnd()));

            // TXT
            foreach (RecordTXT r in dnsResponse.RecordsTXT)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r.ToString().TrimEnd()));
        }
        #endregion
    }
}
