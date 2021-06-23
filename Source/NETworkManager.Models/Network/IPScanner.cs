using DnsClient;
using NETworkManager.Models.Lookup;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
	public class IPScanner
	{
		#region Variables
		private int _progressValue;

		public int Threads = 256;
		public int ICMPTimeout = 4000;
		public byte[] ICMPBuffer = new byte[32];
		public int ICMPAttempts = 2;
		public bool ResolveHostname = true;

		public bool UseCustomDNSServer = false;
		public IPAddress CustomDNSServer;
		public int CustomDNSPort = 53;
		public bool DNSUseTCPOnly = false;
		public bool DNSUseCache = true;
		public bool DNSRecursion = true;
		public TimeSpan DNSTimeout = TimeSpan.FromSeconds(2);
		public int DNSRetries = 3;
		public bool DNSShowErrorMessage = false;

		public bool ResolveMACAddress = false;
		public bool ShowScanResultForAllIPAddresses = false;

		private LookupClient DnsLookupClient;
		#endregion

		#region Events
		public event EventHandler<HostFoundArgs> HostFound;

		protected virtual void OnHostFound(HostFoundArgs e)
		{
			HostFound?.Invoke(this, e);
		}

		public event EventHandler ScanComplete;

		protected virtual void OnScanComplete()
		{
			ScanComplete?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler<ProgressChangedArgs> ProgressChanged;

		protected virtual void OnProgressChanged()
		{
			ProgressChanged?.Invoke(this, new ProgressChangedArgs(_progressValue));
		}

		public event EventHandler UserHasCanceled;

		protected virtual void OnUserHasCanceled()
		{
			UserHasCanceled?.Invoke(this, EventArgs.Empty);
		}
		#endregion

		#region Methods
		public void ScanAsync(IPAddress[] ipAddresses, CancellationToken cancellationToken)
		{
			// Start the scan in a separat task
			Task.Run(() =>
			{
				_progressValue = 0;

				// Create dns client and set options

				if (ResolveHostname)
				{
					DnsLookupClient = UseCustomDNSServer ? new LookupClient(new IPEndPoint(CustomDNSServer, CustomDNSPort)) : new LookupClient();
					DnsLookupClient.UseCache = DNSUseCache;
					DnsLookupClient.Recursion = DNSRecursion;
					DnsLookupClient.Timeout = DNSTimeout;
					DnsLookupClient.Retries = DNSRetries;
					DnsLookupClient.UseTcpOnly = DNSUseTCPOnly;
				}

				// Modify the ThreadPool for better performance
				ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
				ThreadPool.SetMinThreads(workerThreads + Threads, completionPortThreads + Threads);

				try
				{
					var parallelOptions = new ParallelOptions
					{
						CancellationToken = cancellationToken,
						MaxDegreeOfParallelism = Threads
					};

					Parallel.ForEach(ipAddresses, parallelOptions, ipAddress =>
					 {
						 var pingInfo = new PingInfo();
						 var pingable = false;

						 // PING
						 using (var ping = new System.Net.NetworkInformation.Ping())
						 {
							 for (var i = 0; i < ICMPAttempts; i++)
							 {
								 try
								 {
									 var pingReply = ping.Send(ipAddress, ICMPTimeout, ICMPBuffer);

									 if (pingReply != null && IPStatus.Success == pingReply.Status)
									 {
										 if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
											 pingInfo = new PingInfo(pingReply.Address, pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Options.Ttl, pingReply.Status);
										 else
											 pingInfo = new PingInfo(pingReply.Address, pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Status);

										 pingable = true;
										 break; // Continue with the next checks...
									 }

									 if (pingReply != null)
										 pingInfo = new PingInfo(ipAddress, pingReply.Status);
								 }
								 catch (PingException)
								 { }

								 // Don't scan again, if the user has canceled (when more than 1 attempt)
								 if (cancellationToken.IsCancellationRequested)
									 break;
							 }
						 }

						 if (pingable || ShowScanResultForAllIPAddresses)
						 {
							 // DNS
							 var hostname = string.Empty;

							 if (ResolveHostname)
							 {
								 try
								 {
									 var dnsQueryResponse = DnsLookupClient.QueryReverse(ipAddress);

									 if (dnsQueryResponse != null && !dnsQueryResponse.HasError)
										 hostname = dnsQueryResponse.Answers.PtrRecords().FirstOrDefault()?.PtrDomainName;
									 else
										 hostname = DNSShowErrorMessage ? dnsQueryResponse.ErrorMessage : "";
								 }
								 catch (Exception ex)
								 {
									 hostname = DNSShowErrorMessage ? ex.Message : "";
								 }
							 }

							 // ARP
							 PhysicalAddress macAddress = null;
							 var vendor = string.Empty;

							 if (ResolveMACAddress)
							 {
								 // Get info from arp table
								 var arpTableInfo = ARP.GetTable().FirstOrDefault(p => p.IPAddress.ToString() == ipAddress.ToString());

								 if (arpTableInfo != null)
									 macAddress = arpTableInfo.MACAddress;

								 // Check if it is the local mac
								 if (macAddress == null)
								 {
									 var networkInferfaceInfo = NetworkInterface.GetNetworkInterfaces().FirstOrDefault(p => p.IPv4Address.Any(x => x.Item1.Equals(ipAddress)));

									 if (networkInferfaceInfo != null)
										 macAddress = networkInferfaceInfo.PhysicalAddress;
								 }

								 // Vendor lookup
								 if (macAddress != null)
								 {
									 var info = OUILookup.Lookup(macAddress.ToString()).FirstOrDefault();

									 if (info != null)
										 vendor = info.Vendor;
								 }
							 }

							 OnHostFound(new HostFoundArgs(pingInfo, hostname, macAddress, vendor));
						 }

						 IncreaseProcess();
					 });


					OnScanComplete();
				}
				catch (OperationCanceledException)  // If user has canceled
				{
					// Check if the scan is already complete...
					if (ipAddresses.Length == _progressValue)
						OnScanComplete();
					else
						OnUserHasCanceled();
				}
				finally
				{
					// Reset the ThreadPool to default
					ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
					ThreadPool.SetMinThreads(workerThreads - Threads, completionPortThreads - Threads);
				}
			}, cancellationToken);
		}

		private void IncreaseProcess()
		{
			// Increase the progress                        
			Interlocked.Increment(ref _progressValue);
			OnProgressChanged();
		}
		#endregion
	}
}
