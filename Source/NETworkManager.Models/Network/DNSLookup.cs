using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;

namespace NETworkManager.Models.Network;

public sealed class DNSLookup
{
    #region Constructor

    public DNSLookup(DNSLookupSettings settings, IEnumerable<ServerConnectionInfo> dnsServers = null)
    {
        _settings = settings;

        _servers = GetDnsServer(dnsServers);

        // Get the dns suffix from windows or use custom dns suffix from settings if enabled
        if (_settings.AddDNSSuffix)
        {
            _suffix = _settings.UseCustomDNSSuffix
                ? _settings.CustomDNSSuffix
                : IPGlobalProperties.GetIPGlobalProperties().DomainName;

            _addSuffix = !string.IsNullOrEmpty(_suffix);
        }
    }

    #endregion

    #region Variables

    /// <summary>
    ///     DNS lookup settings to use for the DNS lookup.
    /// </summary>
    private readonly DNSLookupSettings _settings;

    /// <summary>
    ///     List of Windows DNS servers or custom DNS servers from the settings to use for the DNS lookup.
    /// </summary>
    private readonly IEnumerable<IPEndPoint> _servers;

    /// <summary>
    ///     Indicates whether the DNS suffix should be appended to the hostname.
    /// </summary>
    private readonly bool _addSuffix;

    /// <summary>
    ///     DNS suffix to append to hostname.
    /// </summary>
    private readonly string _suffix;

    #endregion

    #region Events

    public event EventHandler<DNSLookupRecordReceivedArgs> RecordReceived;

    private void OnRecordReceived(DNSLookupRecordReceivedArgs e)
    {
        RecordReceived?.Invoke(this, e);
    }

    public event EventHandler<DNSLookupErrorArgs> LookupError;

    private void OnLookupError(DNSLookupErrorArgs e)
    {
        LookupError?.Invoke(this, e);
    }

    public event EventHandler LookupComplete;

    private void OnLookupComplete()
    {
        LookupComplete?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Get the DNS servers from Windows or get custom DNS servers from <see cref="DNSLookupSettings" />.
    /// </summary>
    /// <returns>List of DNS servers as <see cref="IPEndPoint" />.</returns>
    private IEnumerable<IPEndPoint> GetDnsServer(IEnumerable<ServerConnectionInfo> dnsServers = null)
    {
        List<IPEndPoint> servers = [];

        // Use windows dns servers
        servers.AddRange(dnsServers == null
            ? NameServer.ResolveNameServers(true, false).Select(dnsServer =>
                new IPEndPoint(IPAddress.Parse(dnsServer.Address), dnsServer.Port))
            : dnsServers.Select(dnsServer => new IPEndPoint(IPAddress.Parse(dnsServer.Server), dnsServer.Port)));

        return servers;
    }

    /// <summary>
    ///     Append DNS suffix to hostname if not set.
    /// </summary>
    /// <param name="hosts">List of hosts</param>
    /// <returns>List of host with DNS suffix</returns>
    private IEnumerable<string> GetHostWithSuffix(IEnumerable<string> hosts)
    {
        return hosts.Select(host => host.Contains('.') ? host : $"{host}.{_suffix}").ToList();
    }

    /// <summary>
    ///     Resolve hostname, fqdn or ip address.
    /// </summary>
    /// <param name="hosts">List of hostnames, FQDNs or ip addresses.</param>
    public void ResolveAsync(IEnumerable<string> hosts)
    {
        Task.Run(() =>
        {
            // Append dns suffix to hostname, if option is set, otherwise just copy the list
            var queries = _addSuffix && _settings.QueryType != QueryType.PTR ? GetHostWithSuffix(hosts) : hosts;

            // For each dns server
            Parallel.ForEach(_servers, dnsServer =>
            {
                // Init each dns server once
                LookupClientOptions lookupClientOptions = new(dnsServer)
                {
                    UseTcpOnly = _settings.UseTCPOnly,
                    UseCache = _settings.UseCache,
                    Recursion = _settings.Recursion,
                    Timeout = _settings.Timeout,
                    Retries = _settings.Retries
                };

                LookupClient lookupClient = new(lookupClientOptions);
                
                // Get the dns server hostname for some additional information
                var dnsServerHostName = string.Empty;

                try
                {
                    var result = lookupClient.QueryReverse(dnsServer.Address);

                    if (!result.HasError)
                    {
                        var record = result.Answers.PtrRecords().FirstOrDefault();
                        
                        if (record != null)
                            dnsServerHostName = record.PtrDomainName;
                    }
                }
                catch { 
                    // ignored
                }

                // For each host
                Parallel.ForEach(queries, query =>
                {
                    try
                    {
                        // Resolve A, AAAA, CNAME, PTR, etc.
                        var dnsResponse = _settings.QueryType == QueryType.PTR
                            ? lookupClient.QueryReverse(IPAddress.Parse(query))
                            : lookupClient.Query(query, _settings.QueryType, _settings.QueryClass);

                        // Pass the error we got from the lookup client (dns server).
                        if (dnsResponse.HasError)
                        {
                            OnLookupError(new DNSLookupErrorArgs(query, $"{dnsServer.Address}",
                                $"{dnsServer.Address}:{dnsServer.Port}", dnsResponse.ErrorMessage));
                            
                            return; // continue
                        }

                        if (dnsResponse.Answers.Count == 0)
                        {
                            var digAdditionalCommand = _settings.QueryType == QueryType.PTR ? " -x " : " ";
                            
                            OnLookupError(new DNSLookupErrorArgs(query, $"{dnsServer.Address}",
                                $"{dnsServer.Address}:{dnsServer.Port}",
                                $"No DNS resource records received for query \"{query}\" (Query type: \"{_settings.QueryType}\") and the DNS server did not return an error. Try to check your DNS server with: dig @{dnsServer.Address}{digAdditionalCommand}{query}"));
                           
                            return; // continue
                        }

                        // Process the results...
                        ProcessDnsAnswers(dnsResponse.Answers, dnsResponse.NameServer, dnsServerHostName);
                    }
                    catch (Exception ex)
                    {
                        OnLookupError(new DNSLookupErrorArgs(query, $"{dnsServer.Address}",
                            $"{dnsServer.Address}:{dnsServer.Port}", ex.Message));
                    }
                });
            });

            OnLookupComplete();
        });
    }

    /// <summary>
    ///     Process the DNS answers and raise the <see cref="RecordReceived" /> event.
    /// </summary>
    /// <param name="answers">List of DNS resource records.</param>
    /// <param name="nameServer">DNS name server that answered the query.</param>
    /// <param name="nameServerHostname">DNS name server hostname.</param>
    private void ProcessDnsAnswers(IEnumerable<DnsResourceRecord> answers, NameServer nameServer,
        string nameServerHostname = null)
    {
        if (answers is not DnsResourceRecord[] dnsResourceRecords)
            return;

        // A
        foreach (var record in dnsResourceRecords.ARecords())
            OnRecordReceived(new DNSLookupRecordReceivedArgs(
                new DNSLookupRecordInfo(
                    record.DomainName, record.TimeToLive, $"{record.RecordClass}", $"{record.RecordType}",
                    $"{record.Address}", $"{nameServer.Address}", nameServerHostname, nameServer.Port)));

        // AAAA
        foreach (var record in dnsResourceRecords.AaaaRecords())
            OnRecordReceived(new DNSLookupRecordReceivedArgs(
                new DNSLookupRecordInfo(
                    record.DomainName, record.TimeToLive, $"{record.RecordClass}", $"{record.RecordType}",
                    $"{record.Address}", $"{nameServer.Address}", nameServerHostname, nameServer.Port)));

        // CNAME
        foreach (var record in dnsResourceRecords.CnameRecords())
            OnRecordReceived(new DNSLookupRecordReceivedArgs(
                new DNSLookupRecordInfo(
                    record.DomainName, record.TimeToLive, $"{record.RecordClass}", $"{record.RecordType}",
                    record.CanonicalName, $"{nameServer.Address}", nameServerHostname, nameServer.Port)));

        // MX
        foreach (var record in dnsResourceRecords.MxRecords())
            OnRecordReceived(new DNSLookupRecordReceivedArgs(
                new DNSLookupRecordInfo(
                    record.DomainName, record.TimeToLive, $"{record.RecordClass}", $"{record.RecordType}",
                    record.Exchange, $"{nameServer.Address}", nameServerHostname, nameServer.Port)));

        // NS
        foreach (var record in dnsResourceRecords.NsRecords())
            OnRecordReceived(new DNSLookupRecordReceivedArgs(
                new DNSLookupRecordInfo(
                    record.DomainName, record.TimeToLive, $"{record.RecordClass}", $"{record.RecordType}",
                    record.NSDName, $"{nameServer.Address}", nameServerHostname, nameServer.Port)));

        // PTR
        foreach (var record in dnsResourceRecords.PtrRecords())
            OnRecordReceived(new DNSLookupRecordReceivedArgs(
                new DNSLookupRecordInfo(
                    record.DomainName, record.TimeToLive, $"{record.RecordClass}", $"{record.RecordType}",
                    record.PtrDomainName, $"{nameServer.Address}", nameServerHostname, nameServer.Port)));

        // SOA
        foreach (var record in dnsResourceRecords.SoaRecords())
            OnRecordReceived(new DNSLookupRecordReceivedArgs(
                new DNSLookupRecordInfo(
                    record.DomainName, record.TimeToLive, $"{record.RecordClass}", $"{record.RecordType}",
                    record.MName + ", " + record.RName, $"{nameServer.Address}", nameServerHostname, nameServer.Port)));

        // TXT
        foreach (var record in dnsResourceRecords.TxtRecords())
            OnRecordReceived(new DNSLookupRecordReceivedArgs(
                new DNSLookupRecordInfo(
                    record.DomainName, record.TimeToLive, $"{record.RecordClass}", $"{record.RecordType}",
                    string.Join(", ", record.Text), $"{nameServer.Address}", nameServerHostname, nameServer.Port)));

        // ToDo: implement more
    }

    #endregion
}