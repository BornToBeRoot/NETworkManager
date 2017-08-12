using Heijden.DNS;
using NETworkManager.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NETworkManager.Models.Network
{
    public class DNSLookup
    {
        #region Variables
        Resolver dnsResolver = new Resolver();
        #endregion

        #region Events
        /* public event EventHandler<IPScannerHostFoundArgs> RecordReceived;

                protected virtual void OnRecordReceived(IPScannerHostFoundArgs e)
                {
                    RecordReceived?.Invoke(this, e);
                } */

        public event EventHandler LookupComplete;

        protected virtual void OnLookupComplete()
        {
            LookupComplete?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods        
        public void LookupAsync(string hostnameOrIPAddress)
        {
            Task.Run(() =>
            {
                dnsResolver.DnsServer = "2001:4860:4860::8888";
                dnsResolver.TransportType = Heijden.DNS.TransportType.Udp;
                dnsResolver.Recursion = true;
                dnsResolver.TransportType = Heijden.DNS.TransportType.Tcp;
                Response dnsResponse = dnsResolver.Query(hostnameOrIPAddress, QType.ANY, QClass.IN);

                if (!string.IsNullOrEmpty(dnsResponse.Error))
                    MessageBox.Show(dnsResponse.Error);

                foreach (RecordA r in dnsResponse.RecordsA)
                {
                    MessageBox.Show(r.Address.ToString());
                }

                foreach (RecordAAAA r in dnsResponse.RecordsAAAA)
                {
                    MessageBox.Show(r.Address.ToString());
                }

                foreach (RecordCNAME r in dnsResponse.RecordsCNAME)
                {
                    MessageBox.Show(r.CNAME);
                }

                foreach (RecordPTR r in dnsResponse.RecordsPTR)
                {
                    MessageBox.Show(r.PTRDNAME);
                }

                foreach (RecordMX r in dnsResponse.RecordsMX)
                {
                    MessageBox.Show(r.EXCHANGE);
                }

                foreach (RecordNS r in dnsResponse.RecordsNS)
                {
                    MessageBox.Show(r.NSDNAME);

                }

                OnLookupComplete();
            });
        }
        #endregion
    }
}
