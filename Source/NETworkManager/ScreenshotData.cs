using System;
using System.Collections.Generic;
using NETworkManager.Models.Cloudflare;
using NETworkManager.Models.IPApi;
using NETworkManager.Settings;

namespace NETworkManager;

/// <summary>
/// Sample data shown instead of real (personal) data when the application is started with
/// <c>--screenshot</c> (<see cref="CommandLineInfo.Screenshot" />), e.g. for the screenshots of the documentation.
/// </summary>
/// <remarks>
/// Public values use the address ranges, domains and AS numbers reserved for documentation. They are not routed on
/// the internet and never belong to a real host or provider, so no real address or provider is shown:
/// <list type="bullet">
///     <item><c>192.0.2.0/24</c> (TEST-NET-1), <c>198.51.100.0/24</c> (TEST-NET-2), <c>203.0.113.0/24</c> (TEST-NET-3):
///     IPv4 documentation ranges (RFC 5737), e.g. the public IPv4 address <c>203.0.113.10</c> and the DNS resolver
///     <c>198.51.100.53</c>.</item>
///     <item><c>2001:db8::/32</c>: IPv6 documentation prefix (RFC 3849), e.g. the public IPv6 address
///     <c>2001:db8::10</c> and the computer IPv6 address <c>2001:db8:1::10</c>.</item>
///     <item><c>AS64496</c> - <c>AS64511</c>: AS numbers for documentation (RFC 5398), e.g. the AS <c>AS64496</c> of
///     the IP geolocation.</item>
///     <item><c>example.com</c> / <c>example.net</c> / <c>example.org</c>: domains for documentation (RFC 2606), e.g.
///     the reverse DNS name <c>host-203-0-113-10.example.net</c>.</item>
///     <item><c>home.arpa</c>: domain for home networks (RFC 8375), e.g. <c>pc-01.home.arpa</c> and
///     <c>router.home.arpa</c>.</item>
/// </list>
/// Local addresses (computer, router) use a typical private network (<c>192.168.1.0/24</c>, RFC 1918) and the IPv6
/// link-local address <c>fe80::1</c> of the router, so they look like a real home network.
/// </remarks>
public static class ScreenshotData
{
    /// <summary>
    /// Indicates if the application was started with <c>--screenshot</c>.
    /// </summary>
    public static bool IsEnabled => CommandLineManager.Current.Screenshot;

    #region Network connection

    public const string ComputerIPv4 = "192.168.1.10";
    public const string ComputerIPv6 = "2001:db8:1::10";
    public const string ComputerDNS = "pc-01.home.arpa";

    public const string RouterIPv4 = "192.168.1.1";
    public const string RouterIPv6 = "fe80::1";
    public const string RouterDNS = "router.home.arpa";

    public const string InternetIPv4 = "203.0.113.10";
    public const string InternetIPv6 = "2001:db8::10";
    public const string InternetDNS = "host-203-0-113-10.example.net";

    private const string Isp = "Example ISP";

    #endregion

    #region IP API

    /// <summary>
    /// Creates the result of the DNS resolver widget.
    /// </summary>
    public static DNSResolverResult CreateDNSResolverResult()
    {
        return new DNSResolverResult(new DNSResolverInfo("198.51.100.53", $"Germany - {Isp}", "203.0.113.0",
            $"Germany - {Isp}"));
    }

    /// <summary>
    /// Creates the result of the IP geolocation widget.
    /// </summary>
    public static IPGeolocationResult CreateIPGeolocationResult()
    {
        return new IPGeolocationResult(new IPGeolocationInfo
        {
            Status = "success",
            Continent = "Europe",
            ContinentCode = "EU",
            Country = "Germany",
            CountryCode = "DE",
            Region = "HE",
            RegionName = "Hesse",
            City = "Frankfurt am Main",
            District = "",
            Zip = "60311",
            Lat = 50.1109,
            Lon = 8.6821,
            Timezone = "Europe/Berlin",
            Offset = 3600,
            Currency = "EUR",
            Isp = Isp,
            Org = Isp,
            As = $"AS64496 {Isp}",
            Asname = "EXAMPLE-AS",
            Reverse = InternetDNS,
            Mobile = false,
            Proxy = false,
            Hosting = false,
            Query = InternetIPv4
        });
    }

    #endregion

    #region Speed test

    /// <summary>
    /// Creates the result of the speed test widget.
    /// </summary>
    public static SpeedTestResult CreateSpeedTestResult()
    {
        return new SpeedTestResult
        {
            DownloadMbps = 487.3,
            UploadMbps = 48.6,
            LatencyMs = 12.4,
            JitterMs = 1.8,
            Isp = Isp,
            ClientCity = "Frankfurt am Main",
            ClientCountry = "DE",
            ServerCity = "Frankfurt",
            ServerCountry = "DE",
            ServerIata = "FRA"
        };
    }

    /// <summary>
    /// Creates the download samples (Mbps) of the speed test sparkline: ramps up with the larger requests and
    /// varies a little, like a real test (same number of samples).
    /// </summary>
    public static IEnumerable<double> CreateSpeedTestDownloadSamples()
    {
        return CreateSamples(32, 487.3);
    }

    /// <summary>
    /// Creates the upload samples (Mbps) of the speed test sparkline, see <see cref="CreateSpeedTestDownloadSamples" />.
    /// </summary>
    public static IEnumerable<double> CreateSpeedTestUploadSamples()
    {
        return CreateSamples(25, 48.6);
    }

    /// <summary>
    /// Creates <paramref name="count" /> deterministic samples that ramp up to about <paramref name="max" />.
    /// </summary>
    private static IEnumerable<double> CreateSamples(int count, double max)
    {
        for (var i = 0; i < count; i++)
        {
            var ramp = 1 - 0.7 * Math.Exp(-i / 5.0);
            var variation = 0.04 * Math.Sin(i * 1.7) + 0.02 * Math.Cos(i * 3.1);

            yield return Math.Round(max * (ramp + variation) * 0.97, 1);
        }
    }

    #endregion
}
