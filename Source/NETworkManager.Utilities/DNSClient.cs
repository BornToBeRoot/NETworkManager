using DnsClient;
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
    ///     Immutable snapshot of everything a resolve call needs (lookup client + settings it was configured
    ///     with), published as a single reference so concurrent resolves during a <see cref="Configure" />
    ///     call never observe a torn combination (e.g. the old suffix flag with the new lookup client).
    ///     Relies on <see cref="DNSClientSettings" /> not being mutated after being passed to
    ///     <see cref="Configure" /> - see the note on <see cref="DNSClientSettings" /> itself.
    /// </summary>
    private sealed record ResolverState(LookupClient Client, bool AddSuffix, DNSClientSettings Settings);

    /// <summary>
    ///     Indicates if the DNS client is configured.
    /// </summary>
    private bool _isConfigured;

    /// <summary>
    ///     Current resolver state (lookup client + DNS suffix behavior), swapped atomically on configure.
    /// </summary>
    private ResolverState _state;

    /// <summary>
    ///     Method to configure the DNS client.
    /// </summary>
    /// <param name="settings"></param>
    public void Configure(DNSClientSettings settings)
    {
        Log.Debug("Configure - Configuring DNS client...");

        LookupClient client;

        if (settings.UseCustomDNSServers)
        {
            Log.Debug("Configure - Using custom DNS servers...");

            // Setup custom DNS servers
            List<NameServer> servers = [];

            foreach (var (server, port) in settings.DNSServers)
            {
                Log.Debug($"Configure - Adding custom DNS server: {server}:{port}");
                servers.Add(new IPEndPoint(IPAddress.Parse(server), port));
            }

            Log.Debug("Configure - Creating LookupClient with custom DNS servers...");
            client = new LookupClient(new LookupClientOptions([.. servers]));
        }
        else
        {
            Log.Debug("Configure - Creating LookupClient with Windows default DNS servers...");
            client = new LookupClient();
        }

        var addSuffix = settings.AddDNSSuffix && !string.IsNullOrEmpty(settings.DNSSuffix);
        Log.Debug(addSuffix
            ? $"Configure - DNS suffix will be added to hostnames without a dot: {settings.DNSSuffix}"
            : "Configure - DNS suffix will NOT be added to hostnames without a dot.");

        _state = new ResolverState(client, addSuffix, settings);

        Log.Debug("Configure - DNS client configured.");
        _isConfigured = true;
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

        var state = _state;

        query = AddDNSSuffixIfConfigured(query, state);

        try
        {
            var result = await state.Client.QueryAsync(query, QueryType.A);

            // Pass the error we got from the lookup client (dns server).
            // NXDOMAIN is not a real failure like a timeout - flag it via IsNotFound so callers can
            // treat it as "no record". SERVFAIL/REFUSED are not included here: for a forward lookup
            // they mean the resolver actually failed or refused the query, not "record does not exist".
            if (result.HasError)
                return new DNSClientResultIPAddress(result.HasError, result.ErrorMessage, $"{result.NameServer}")
                { IsNotFound = IsNotFoundResponseCode(result.Header.ResponseCode) };

            // Validate result because of https://github.com/BornToBeRoot/NETworkManager/issues/1934
            var record = result.Answers.ARecords().FirstOrDefault();

            return record != null
                ? new DNSClientResultIPAddress(record.Address, $"{result.NameServer}")
                : new DNSClientResultIPAddress(true,
                    $"IP address for \"{query}\" could not be resolved and the DNS server did not return an error. Try to check your DNS server with: dig @{result.NameServer.Address} {query}",
                    $"{result.NameServer}")
                { IsNotFound = true };
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

        var state = _state;

        query = AddDNSSuffixIfConfigured(query, state);

        try
        {
            var result = await state.Client.QueryAsync(query, QueryType.AAAA);

            // Pass the error we got from the lookup client (dns server).
            // NXDOMAIN is not a real failure like a timeout - flag it via IsNotFound so callers can
            // treat it as "no record". SERVFAIL/REFUSED are not included here: for a forward lookup
            // they mean the resolver actually failed or refused the query, not "record does not exist".
            if (result.HasError)
                return new DNSClientResultIPAddress(result.HasError, result.ErrorMessage, $"{result.NameServer}")
                { IsNotFound = IsNotFoundResponseCode(result.Header.ResponseCode) };

            // Validate result because of https://github.com/BornToBeRoot/NETworkManager/issues/1934
            var record = result.Answers.AaaaRecords().FirstOrDefault();

            return record != null
                ? new DNSClientResultIPAddress(record.Address, $"{result.NameServer}")
                : new DNSClientResultIPAddress(true,
                    $"IP address for \"{query}\" could not be resolved and the DNS server did not return an error. Try to check your DNS server with: dig @{result.NameServer.Address} {query}",
                    $"{result.NameServer}")
                { IsNotFound = true };
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
            var result = await _state.Client.QueryAsync(query, QueryType.CNAME);

            // Pass the error we got from the lookup client (dns server).
            // NXDOMAIN is not a real failure like a timeout - flag it via IsNotFound so callers can
            // treat it as "no record". SERVFAIL/REFUSED are not included here: for a forward lookup
            // they mean the resolver actually failed or refused the query, not "record does not exist".
            if (result.HasError)
                return new DNSClientResultString(result.HasError, result.ErrorMessage, $"{result.NameServer}")
                { IsNotFound = IsNotFoundResponseCode(result.Header.ResponseCode) };

            // Validate result because of https://github.com/BornToBeRoot/NETworkManager/issues/1934
            var record = result.Answers.CnameRecords().FirstOrDefault();

            return record != null
                ? new DNSClientResultString(record.CanonicalName, $"{result.NameServer}")
                : new DNSClientResultString(true,
                    $"CNAME for \"{query}\" could not be resolved and the DNS server did not return an error. Try to check your DNS server with: dig @{result.NameServer.Address} {query}",
                    $"{result.NameServer}")
                { IsNotFound = true };
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
            var result = await _state.Client.QueryReverseAsync(ipAddress);

            // Pass the error we got from the lookup client (dns server).
            // NXDOMAIN is always a clean "no record". For private/ULA IP ranges (the common case for
            // router/computer PTR lookups) many resolvers also return SERVFAIL/REFUSED instead of a
            // clean NXDOMAIN when there is no reverse zone, so those are treated as "not found" too -
            // but only for private ranges, since for a public IP a SERVFAIL/REFUSED usually means the
            // resolver actually failed or refused the query.
            if (result.HasError)
                return new DNSClientResultString(result.HasError, result.ErrorMessage, $"{result.NameServer}")
                {
                    IsNotFound = IsNotFoundResponseCode(result.Header.ResponseCode,
                        IPAddressHelper.IsPrivateIPAddress(ipAddress))
                };

            // Validate result because of https://github.com/BornToBeRoot/NETworkManager/issues/1934
            var record = result.Answers.PtrRecords().FirstOrDefault();

            return record != null
                ? new DNSClientResultString(record.PtrDomainName, $"{result.NameServer}")
                : new DNSClientResultString(true,
                    $"PTR for \"{ipAddress}\" could not be resolved and the DNS server did not return an error. Try to check your DNS server with: dig @{result.NameServer.Address} -x {ipAddress}",
                    $"{result.NameServer}")
                { IsNotFound = true };
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

    /// <summary>
    ///     Appends the configured DNS suffix to a hostname without a dot (forward lookups only).
    ///     FQDNs (containing a dot) are returned unchanged.
    /// </summary>
    /// <param name="query">Hostname or FQDN as string like "example.com".</param>
    /// <param name="state">Resolver state snapshot captured at the start of the resolve call.</param>
    /// <returns>Query with the DNS suffix appended, if configured and applicable.</returns>
    private static string AddDNSSuffixIfConfigured(string query, ResolverState state)
    {
        // Exclude IP literals - an IPv6 address like "2001:db8::1" has no dot and would otherwise be
        // mistaken for a bare hostname (e.g. by the profile "Resolve" action, which allows IP literals).
        return state.AddSuffix && !string.IsNullOrEmpty(query) && !query.Contains('.') &&
               !IPAddress.TryParse(query, out _)
            ? $"{query}.{state.Settings.DNSSuffix}"
            : query;
    }

    /// <summary>
    ///     Determines whether a DNS response code means "no record" rather than a real failure.
    ///     NXDOMAIN is always a clean "does not exist". SERVFAIL and REFUSED are only treated as
    ///     "no record" when <paramref name="treatServerErrorsAsNotFound" /> is set, since outside of
    ///     that case they usually indicate the resolver actually failed or refused the query rather
    ///     than a confirmed absence of the record.
    /// </summary>
    /// <param name="responseCode">The DNS response code to check.</param>
    /// <param name="treatServerErrorsAsNotFound">
    ///     Whether SERVFAIL/REFUSED should also count as "no record" - true for reverse (PTR) lookups
    ///     on private/RFC1918/ULA IP ranges, where many resolvers return them instead of a clean
    ///     NXDOMAIN when there is no reverse zone.
    /// </param>
    private static bool IsNotFoundResponseCode(DnsHeaderResponseCode responseCode,
        bool treatServerErrorsAsNotFound = false)
    {
        if (responseCode is DnsHeaderResponseCode.NotExistentDomain)
            return true;

        return treatServerErrorsAsNotFound
               && responseCode is DnsHeaderResponseCode.ServerFailure or DnsHeaderResponseCode.Refused;
    }
}
