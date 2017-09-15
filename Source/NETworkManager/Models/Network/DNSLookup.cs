using Heijden.DNS;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class DNSLookup
    {
        #region Variables
        Resolver DNSResolver = new Resolver();
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

        public event EventHandler<DNSLookupCompleteArgs> LookupComplete;

        protected virtual void OnLookupComplete(DNSLookupCompleteArgs e)
        {
            LookupComplete?.Invoke(this, e);
        }
        #endregion

        #region Methods        
        public void LookupAsync(string hostnameOrIPAddress, DNSLookupOptions DNSLookupOptions)
        {
            Task.Run(() =>
            {
                // This will convert the query to an Arpa request
                if (DNSLookupOptions.Type == QType.PTR)
                {
                    if (IPAddress.TryParse(hostnameOrIPAddress, out IPAddress ip))
                        hostnameOrIPAddress = Resolver.GetArpaFromIp(ip);
                }

                if (DNSLookupOptions.Type == QType.NAPTR)
                    hostnameOrIPAddress = Resolver.GetArpaFromEnum(hostnameOrIPAddress);

                if (DNSLookupOptions.UseCustomDNSServer)
                    DNSResolver.DnsServer = DNSLookupOptions.CustomDNSServer;
                
                DNSResolver.Recursion = DNSLookupOptions.Recursion;
                DNSResolver.TransportType = DNSLookupOptions.TransportType;
                DNSResolver.Retries = DNSLookupOptions.Attempts;
                DNSResolver.TimeOut = DNSLookupOptions.Timeout;

                Response DNSResponse = DNSResolver.Query(hostnameOrIPAddress, DNSLookupOptions.Type, DNSLookupOptions.Class);

                // If there was an error... return
                if (!string.IsNullOrEmpty(DNSResponse.Error))
                {
                    OnLookupError(new DNSLookupErrorArgs(DNSResponse.Error, DNSResolver.DnsServer));
                    return;
                }
                
                // Process the results...
                ProcessResponse(DNSResponse);

                // If we get a CNAME back (from a result), do a second request and try to get the A, AAAA etc... 
                if(DNSLookupOptions.ResolveCNAME && DNSLookupOptions.Type != QType.CNAME)
                {
                    foreach (RecordCNAME r in DNSResponse.RecordsCNAME)
                    {
                        Response DNSResponse2 = DNSResolver.Query(r.CNAME, DNSLookupOptions.Type, DNSLookupOptions.Class);

                        if(!string.IsNullOrEmpty(DNSResponse2.Error))
                        {
                            OnLookupError(new DNSLookupErrorArgs(DNSResponse2.Error, DNSResolver.DnsServer));
                            continue;
                        }

                        ProcessResponse(DNSResponse2);
                    }
                }

                OnLookupComplete(new DNSLookupCompleteArgs(DNSResponse.RecordsA.Length + DNSResponse.RecordsAAAA.Length + DNSResponse.RecordsCNAME.Length + DNSResponse.RecordsMX.Length + DNSResponse.RecordsNS.Length + DNSResponse.RecordsPTR.Length + DNSResponse.RecordsSOA.Length + DNSResponse.RecordsTXT.Length));
            });
        }

        private void ProcessResponse(Response DNSResponse)
        {
            // A
            foreach (RecordA r in DNSResponse.RecordsA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r));

            // AAAA
            foreach (RecordAAAA r in DNSResponse.RecordsAAAA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r));

            // CNAME
            foreach (RecordCNAME r in DNSResponse.RecordsCNAME)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r));

            // MX
            foreach (RecordMX r in DNSResponse.RecordsMX)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r));

            // NS
            foreach (RecordNS r in DNSResponse.RecordsNS)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r));

            // PTR
            foreach (RecordPTR r in DNSResponse.RecordsPTR)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r));

            // SOA
            foreach (RecordSOA r in DNSResponse.RecordsSOA)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r));

            // TXT
            foreach (RecordTXT r in DNSResponse.RecordsTXT)
                OnRecordReceived(new DNSLookupRecordArgs(r.RR, r));
        }
        #endregion
    }
}
