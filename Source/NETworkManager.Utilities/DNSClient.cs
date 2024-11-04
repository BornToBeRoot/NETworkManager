﻿using DnsClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace NETworkManager.Utilities;

public class DNSClient : SingletonBase<DNSClient>
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(DNSClient));
    
    /// <summary>
    ///     Error message which is returned when the DNS client is not configured.
    /// </summary>
    private const string NotConfiguredMessage = "DNS client is not configured. Call Configure() first.";

    /// <summary>
    ///     Hold the current instance of the LookupClient.
    /// </summary>
    private LookupClient _client;

    /// <summary>
    ///     Indicates if the DNS client is configured.
    /// </summary>
    private bool _isConfigured;

    /// <summary>
    ///     Store the current DNS settings.
    /// </summary>
    private DNSClientSettings _settings;

    /// <summary>
    ///     Method to configure the DNS client.
    /// </summary>
    /// <param name="settings"></param>
    public void Configure(DNSClientSettings settings)
    {
        _settings = settings;

        Log.Debug("Configuring DNS client...");
        
        if (_settings.UseCustomDNSServers)
        {
            Log.Debug("Using custom DNS servers...");
            
            // Setup custom DNS servers
            List<NameServer> servers = [];

            foreach (var (server, port) in _settings.DNSServers)
            {
                Log.Debug($"Adding custom DNS server: {server}:{port}");
                servers.Add(new IPEndPoint(IPAddress.Parse(server), port));
            }

            Log.Debug("Creating LookupClient with custom DNS servers...");
            _client = new LookupClient(new LookupClientOptions(servers.ToArray()));
        }
        else
        {
            Log.Debug("Creating LookupClient with Windows default DNS servers...");
            _client = new LookupClient();
        }
        
        Log.Debug("DNS client configured.");
        _isConfigured = true;
    }

    /// <summary>
    ///     Method to update the (Windows) name servers of the DNS client
    ///     when they may have changed due to a network update.
    /// </summary>
    public void UpdateWindowsDNSSever()
    {
        Log.Debug("Recreating LookupClient with with Windows default DNS servers...");
        _client = new LookupClient();
    }

    /// <summary>
    ///     Resolve an IPv4 address from a hostname or FQDN.
    /// </summary>
    /// <param name="query">Hostname or FQDN as string like "example.com".</param>
    /// <returns><see cref="IPAddress" /> of the host.</returns>
    public async Task<DNSClientResultIPAddress> ResolveAAsync(string query)
    {
        if (!_isConfigured)
            throw new DNSClientNotConfiguredException(NotConfiguredMessage);

        try
        {
            var result = await _client.QueryAsync(query, QueryType.A);

            // Pass the error we got from the lookup client (dns server).
            if (result.HasError)
                return new DNSClientResultIPAddress(result.HasError, result.ErrorMessage, $"{result.NameServer}");

            // Validate result because of https://github.com/BornToBeRoot/NETworkManager/issues/1934
            var record = result.Answers.ARecords().FirstOrDefault();

            return record != null
                ? new DNSClientResultIPAddress(record.Address, $"{result.NameServer}")
                : new DNSClientResultIPAddress(true,
                    $"IP address for \"{query}\" could not be resolved and the DNS server did not return an error. Try to check your DNS server with: dig @{result.NameServer.Address} {query}",
                    $"{result.NameServer}");
        }
        catch (DnsResponseException ex)
        {
            return new DNSClientResultIPAddress(true, ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error($"Error while resolving A record (Query string is \"{query}\".", ex);
            return new DNSClientResultIPAddress(true, ex.Message);
        }
    }

    /// <summary>
    ///     Resolve an IPv6 address from a hostname or FQDN.
    /// </summary>
    /// <param name="query">Hostname or FQDN as string like "example.com".</param>
    /// <returns><see cref="IPAddress" /> of the host.</returns>
    public async Task<DNSClientResultIPAddress> ResolveAaaaAsync(string query)
    {
        if (!_isConfigured)
            throw new DNSClientNotConfiguredException(NotConfiguredMessage);

        try
        {
            var result = await _client.QueryAsync(query, QueryType.AAAA);

            // Pass the error we got from the lookup client (dns server).
            if (result.HasError)
                return new DNSClientResultIPAddress(result.HasError, result.ErrorMessage, $"{result.NameServer}");

            // Validate result because of https://github.com/BornToBeRoot/NETworkManager/issues/1934
            var record = result.Answers.AaaaRecords().FirstOrDefault();

            return record != null
                ? new DNSClientResultIPAddress(record.Address, $"{result.NameServer}")
                : new DNSClientResultIPAddress(true,
                    $"IP address for \"{query}\" could not be resolved and the DNS server did not return an error. Try to check your DNS server with: dig @{result.NameServer.Address} {query}",
                    $"{result.NameServer}");
        }
        catch (DnsResponseException ex)
        {
            return new DNSClientResultIPAddress(true, ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error($"Error while resolving AAAA record (Query string is \"{query}\".", ex);
            return new DNSClientResultIPAddress(true, ex.Message);
        }
    }

    /// <summary>
    ///     Resolve a CNAME from a hostname or FQDN.
    /// </summary>
    /// <param name="query">Hostname or FQDN as string like "example.com".</param>
    /// <returns>CNAME of the host.</returns>
    public async Task<DNSClientResultString> ResolveCnameAsync(string query)
    {
        if (!_isConfigured)
            throw new DNSClientNotConfiguredException(NotConfiguredMessage);

        try
        {
            var result = await _client.QueryAsync(query, QueryType.CNAME);

            // Pass the error we got from the lookup client (dns server).
            if (result.HasError)
                return new DNSClientResultString(result.HasError, result.ErrorMessage, $"{result.NameServer}");

            // Validate result because of https://github.com/BornToBeRoot/NETworkManager/issues/1934
            var record = result.Answers.CnameRecords().FirstOrDefault();

            return record != null
                ? new DNSClientResultString(record.CanonicalName, $"{result.NameServer}")
                : new DNSClientResultString(true,
                    $"CNAME for \"{query}\" could not be resolved and the DNS server did not return an error. Try to check your DNS server with: dig @{result.NameServer.Address} {query}",
                    $"{result.NameServer}");
        }
        catch (DnsResponseException ex)
        {
            return new DNSClientResultString(true, ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error($"Error while resolving CNAME record (Query string is \"{query}\".", ex);
            return new DNSClientResultString(true, ex.Message);
        }
    }

    /// <summary>
    ///     Resolve a PTR for an IP address.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <returns>PTR domain name.</returns>
    public async Task<DNSClientResultString> ResolvePtrAsync(IPAddress ipAddress)
    {
        if (!_isConfigured)
            throw new DNSClientNotConfiguredException(NotConfiguredMessage);

        try
        {
            var result = await _client.QueryReverseAsync(ipAddress);

            // Pass the error we got from the lookup client (dns server).
            if (result.HasError)
                return new DNSClientResultString(result.HasError, result.ErrorMessage, $"{result.NameServer}");

            // Validate result because of https://github.com/BornToBeRoot/NETworkManager/issues/1934
            var record = result.Answers.PtrRecords().FirstOrDefault();

            return record != null
                ? new DNSClientResultString(record.PtrDomainName, $"{result.NameServer}")
                : new DNSClientResultString(true,
                    $"PTR for \"{ipAddress}\" could not be resolved and the DNS server did not return an error. Try to check your DNS server with: dig @{result.NameServer.Address} -x {ipAddress}",
                    $"{result.NameServer}");
        }
        catch (DnsResponseException ex)
        {
            return new DNSClientResultString(true, ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error($"Error while resolving PTR record (IP address is \"{ipAddress}\".", ex);
            return new DNSClientResultString(true, ex.Message);
        }
    }
}
