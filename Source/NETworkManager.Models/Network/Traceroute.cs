using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Models.IPApi;

namespace NETworkManager.Models.Network;

public class Traceroute
{
    #region Variables

    private readonly TracerouteOptions _options;

    #endregion

    #region Events

    public event EventHandler<TracerouteHopReceivedArgs> HopReceived;

    protected virtual void OnHopReceived(TracerouteHopReceivedArgs e)
    {
        HopReceived?.Invoke(this, e);
    }

    public event EventHandler TraceComplete;

    protected virtual void OnTraceComplete()
    {
        TraceComplete?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<MaximumHopsReachedArgs> MaximumHopsReached;

    protected virtual void OnMaximumHopsReached(MaximumHopsReachedArgs e)
    {
        MaximumHopsReached?.Invoke(this, e);
    }

    public event EventHandler<TracerouteErrorArgs> TraceError;

    protected virtual void OnTraceError(TracerouteErrorArgs e)
    {
        TraceError?.Invoke(this, e);
    }

    public event EventHandler UserHasCanceled;

    protected virtual void OnUserHasCanceled()
    {
        UserHasCanceled?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Constructor

    public Traceroute(TracerouteOptions options)
    {
        _options = options;
    }

    #endregion

    #region Methods

    public void TraceAsync(IPAddress ipAddress, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            try
            {
                for (var i = 1; i < _options.MaximumHops + 1; i++)
                {
                    var tasks = new List<Task<Tuple<PingReply, long>>>();

                    // Send 3 pings
                    for (var y = 0; y < 3; y++)
                    {
                        var i1 = i;

                        tasks.Add(Task.Run(() =>
                        {
                            var stopwatch = new Stopwatch();

                            PingReply pingReply;

                            using (var ping = new System.Net.NetworkInformation.Ping())
                            {
                                stopwatch.Start();

                                pingReply = ping.Send(ipAddress, _options.Timeout, _options.Buffer,
                                    new PingOptions { Ttl = i1, DontFragment = _options.DontFragment });

                                stopwatch.Stop();
                            }

                            return Tuple.Create(pingReply, stopwatch.ElapsedMilliseconds);
                        }, cancellationToken));
                    }

                    try
                    {
                        Task.WaitAll(tasks.ToArray());
                    }
                    catch (AggregateException ex)
                    {
                        // Remove duplicate messages
                        OnTraceError(new TracerouteErrorArgs(string.Join(", ",
                            ex.Flatten().InnerExceptions.Select(s => s.Message).Distinct())));
                        return;
                    }

                    // Check results -> Get IP on success or TTL expired
                    var ipAddressHop = (from task in tasks
                        where task.Result.Item1.Status != IPStatus.TimedOut
                        where task.Result.Item1.Status is IPStatus.TtlExpired or IPStatus.Success
                        select task.Result.Item1.Address).FirstOrDefault();

                    // Resolve Hostname
                    var hostname = string.Empty;

                    if (_options.ResolveHostname && ipAddressHop != null)
                    {
                        var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(ipAddressHop);

                        if (!dnsResult.HasError)
                            hostname = dnsResult.Value;
                    }

                    IPGeolocationResult ipGeolocationResult = null;

                    // Get IP geolocation info
                    if (_options.CheckIPApiIPGeolocation && ipAddressHop != null && !IPAddressHelper.IsPrivateIPAddress(ipAddressHop))
                        ipGeolocationResult =
                            await IPGeolocationService.GetInstance().GetIPGeolocationAsync($"{ipAddressHop}");

                    OnHopReceived(new TracerouteHopReceivedArgs(new TracerouteHopInfo(i,
                        tasks[0].Result.Item1.Status, tasks[0].Result.Item2,
                        tasks[1].Result.Item1.Status, tasks[1].Result.Item2,
                        tasks[2].Result.Item1.Status, tasks[2].Result.Item2,
                        ipAddressHop, hostname, ipGeolocationResult ?? new IPGeolocationResult())));

                    // Check if finished
                    if (ipAddressHop != null && ipAddress.ToString() == ipAddressHop.ToString())
                    {
                        OnTraceComplete();
                        return;
                    }

                    // Check for cancel
                    if (!cancellationToken.IsCancellationRequested)
                        continue;

                    OnUserHasCanceled();
                    return;
                }

                // Max hops reached...
                OnMaximumHopsReached(new MaximumHopsReachedArgs(_options.MaximumHops));
            }
            catch (Exception ex)
            {
                OnTraceError(new TracerouteErrorArgs(ex.Message));
            }
        }, cancellationToken);
    }

    #endregion
}
