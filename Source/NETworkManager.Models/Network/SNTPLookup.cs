using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class SNTPLookup
    {
        #region Variables
        private SNTPLookupSettings _settings;
        #endregion

        #region Constructor
        public SNTPLookup(SNTPLookupSettings settings)
        {
            _settings = settings;
        }
        #endregion

        #region Events
        public event EventHandler<SNTPLookupResultArgs> ResultReceived;

        protected virtual void OnResultReceived(SNTPLookupResultArgs e)
        {
            ResultReceived?.Invoke(this, e);
        }

        public event EventHandler<SNTPLookupErrorArgs> LookupError;

        protected virtual void OnLookupError(SNTPLookupErrorArgs e)
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
        public static SNTPDateTime GetNetworkTimeRfc2030(IPEndPoint server, int timeout = 4000)
        {
            var ntpData = new byte[48]; // RFC 2030
            ntpData[0] = 0x1B; // LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            var udpClient = new UdpClient(server.AddressFamily);
            udpClient.Client.SendTimeout = timeout;
            udpClient.Client.ReceiveTimeout = timeout;
            udpClient.Connect(server);

            var localStartTime = DateTime.Now.ToUniversalTime();

            udpClient.Send(ntpData, ntpData.Length);
            ntpData = udpClient.Receive(ref server);

            var localEndTime = DateTime.Now.ToUniversalTime();

            udpClient.Close();

            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

            var milliseconds = (intPart * 1000) + (fractPart * 1000 / 0x100000000L);
            var networkTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

            // Calculate local offset with local start/end time and network time in seconds            
            var roundTripDelayTicks = localEndTime.Ticks - localStartTime.Ticks;
            var offsetInSeconds = (localStartTime.Ticks + (roundTripDelayTicks / 2) - networkTime.Ticks) / TimeSpan.TicksPerSecond;

            return new SNTPDateTime()
            {
                LocalStartTime = localStartTime,
                LocalEndTime = localEndTime,
                NetworkTime = networkTime,
                RoundTripDelay = roundTripDelayTicks / TimeSpan.TicksPerMillisecond,
                Offset = offsetInSeconds
            };
        }

        public void QueryAsync(IEnumerable<ServerInfo> servers)
        {
            Task.Run(() =>
            {
                Parallel.ForEach(servers, server =>
                {
                    // NTP requires an IP address to connect to
                    IPAddress serverIP = null;

                    if (Regex.IsMatch(server.Server, RegexHelper.IPv4AddressRegex) || Regex.IsMatch(server.Server, RegexHelper.IPv6AddressRegex))
                    {
                        serverIP = IPAddress.Parse(server.Server);
                    }
                    else
                    {
                        using var dnsResolverTask = DNSClientHelper.ResolveAorAaaaAsync(server.Server, true);

                        // Wait for task inside a Parallel.Foreach
                        dnsResolverTask.Wait();

                        if (dnsResolverTask.Result.HasError)
                        {
                            OnLookupError(new SNTPLookupErrorArgs(DNSClientHelper.FormatDNSClientResultError(server.Server, dnsResolverTask.Result), true));
                            return;
                        }

                        serverIP = dnsResolverTask.Result.Value;
                    }

                    try
                    {
                        SNTPDateTime dateTime = GetNetworkTimeRfc2030(new(serverIP, server.Port), _settings.Timeout);

                        OnResultReceived(new SNTPLookupResultArgs(server.Server, $"{serverIP}:{server.Port}", dateTime));
                    }
                    catch (Exception ex)
                    {
                        OnLookupError(new SNTPLookupErrorArgs(server.Server, $"{serverIP}:{server.Port}", ex.Message));
                    }
                });

                OnLookupComplete();
            });
        }
        #endregion                
    }
}
