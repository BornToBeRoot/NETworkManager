using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using NETworkManager.Models.Cloudflare;
using NETworkManager.Models.IPApi;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;

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

    #region Network Interface

    /// <summary>
    /// Creates the network adapters of the Network Interface tool (instead of the real adapters): a connected Ethernet
    /// adapter (the computer in the home network, DHCP) and a disconnected Wi-Fi adapter. The MAC addresses are locally
    /// administered (02-...), so they don't belong to a real vendor.
    /// </summary>
    public static List<NetworkInterfaceInfo> CreateNetworkInterfaces()
    {
        var now = DateTime.Now;

        return
        [
            new NetworkInterfaceInfo
            {
                Id = "{6F1C2D3E-0000-4000-8000-000000000001}",
                Name = "Ethernet",
                Description = "Example Gigabit Network Connection",
                Type = "Ethernet",
                PhysicalAddress = PhysicalAddress.Parse("02-00-5E-10-00-0A"),
                Status = OperationalStatus.Up,
                IsOperational = true,
                Speed = 1_000_000_000,
                IPv4ProtocolAvailable = true,
                IPv4Address =
                [
                    new Tuple<IPAddress, IPAddress>(IPAddress.Parse(ComputerIPv4), IPAddress.Parse("255.255.255.0"))
                ],
                IPv4Gateway = [IPAddress.Parse(RouterIPv4)],
                DhcpEnabled = true,
                DhcpServer = [IPAddress.Parse(RouterIPv4)],
                DhcpLeaseObtained = now.AddHours(-2),
                DhcpLeaseExpires = now.AddHours(22),
                IPv6ProtocolAvailable = true,
                IPv6Address = [IPAddress.Parse(ComputerIPv6)],
                IPv6AddressLinkLocal = [IPAddress.Parse("fe80::10")],
                IPv6Gateway = [IPAddress.Parse(RouterIPv6)],
                DNSAutoconfigurationEnabled = true,
                DNSSuffix = "home.arpa",
                DNSServer = [IPAddress.Parse(RouterIPv4)],
                Profile = NetworkProfile.Private
            },
            new NetworkInterfaceInfo
            {
                Id = "{6F1C2D3E-0000-4000-8000-000000000002}",
                Name = "Wi-Fi",
                Description = "Example Wi-Fi 6 Wireless Adapter",
                Type = "Wireless80211",
                PhysicalAddress = PhysicalAddress.Parse("02-00-5E-10-00-0B"),
                Status = OperationalStatus.Down,
                IsOperational = false,
                Speed = -1,
                IPv4ProtocolAvailable = true,
                IPv4Address = [],
                IPv4Gateway = [],
                DhcpEnabled = true,
                DhcpServer = [],
                IPv6ProtocolAvailable = true,
                IPv6Address = [],
                IPv6AddressLinkLocal = [],
                IPv6Gateway = [],
                DNSAutoconfigurationEnabled = true,
                DNSServer = []
            }
        ];
    }

    /// <summary>
    /// Creates the samples of the bandwidth chart (one per second, ending now): a download with a short peak and a
    /// smaller upload, so the chart is filled without a measurement.
    /// </summary>
    /// <param name="count">Number of samples (seconds).</param>
    public static List<BandwidthMeterSpeedArgs> CreateBandwidthSamples(int count)
    {
        var start = DateTime.Now.AddSeconds(-count);
        var samples = new List<BandwidthMeterSpeedArgs>();

        long totalReceived = 48_213_557_120; // ~48 GB since boot
        long totalSent = 6_902_114_304;

        for (var i = 0; i < count; i++)
        {
            // Bytes per second: ~3-6 MB/s download with a peak in the middle, ~0.3-0.6 MB/s upload
            var peak = Math.Exp(-Math.Pow((i - count * 0.6) / (count * 0.08), 2));
            var received = (long)(4_000_000 + 1_500_000 * Math.Sin(i / 4.0) + 9_000_000 * peak);
            var sent = (long)(450_000 + 150_000 * Math.Cos(i / 3.0) + 900_000 * peak);

            totalReceived += received;
            totalSent += sent;

            samples.Add(new BandwidthMeterSpeedArgs(start.AddSeconds(i), totalReceived, totalSent, received, sent));
        }

        return samples;
    }

    #endregion

    #region LLDP / CDP Capture

    /// <summary>
    /// Creates the LLDP packet of the switch the computer is connected to (instead of a capture).
    /// </summary>
    public static LLDPCDPPackageInfo CreateLLDPCDPPackage()
    {
        const string switchIPv4 = "192.168.1.2";

        return new LLDPCDPPackageInfo
        {
            Device = "switch-01.home.arpa",
            DeviceDescription = "Example Switch OS 4.2.1, Example GS-24P 24-Port Gigabit PoE Switch",
            Port = "gi1/0/12",
            PortDescription = "Office - PC-01",
            Model = "Example GS-24P",
            VLAN = "10",
            IPAddress = switchIPv4,
            Management = switchIPv4,
            ChassisId = "02005E1000AB",
            Protocol = "LLDP",
            TimeToLive = "120",
            LocalConnection = "Ethernet",
            LocalInterface = "Example Gigabit Network Connection"
        };
    }

    #endregion

    #region mDNS Discovery

    /// <summary>
    /// Creates the network interface of the mDNS Discovery tool (instead of the real adapters).
    /// </summary>
    public static IReadOnlyList<MDNSInterfaceInfo> CreateMDNSInterfaces()
    {
        return [new MDNSInterfaceInfo("screenshot", "Ethernet", IPAddress.Parse(ComputerIPv4), 12, 12)];
    }

    /// <summary>
    /// Services of a home network: (instance, type, host, port, IPv4 address, TXT entries).
    /// </summary>
    private static readonly (string Instance, string Type, string Host, ushort Port, string IPv4, string[] Text)[]
        MDNSServices =
        [
            ("Office Printer", "_ipp._tcp", "office-printer", 631, "192.168.1.20",
                ["txtvers=1", "ty=Example LaserJet 400", "rp=ipp/print", "pdl=application/pdf,image/urf", "Color=T", "Duplex=T"]),
            ("Office Printer", "_ipps._tcp", "office-printer", 631, "192.168.1.20",
                ["txtvers=1", "ty=Example LaserJet 400", "rp=ipp/print", "TLS=1.3"]),
            ("Office Printer", "_uscan._tcp", "office-printer", 80, "192.168.1.20",
                ["txtvers=1", "ty=Example LaserJet 400", "rs=eSCL", "vers=2.63", "is=platen,adf"]),
            ("NAS", "_smb._tcp", "nas", 445, "192.168.1.40", []),
            ("NAS", "_http._tcp", "nas", 5000, "192.168.1.40", ["path=/"]),
            ("NAS", "_ssh._tcp", "nas", 22, "192.168.1.40", []),
            ("Living Room TV", "_googlecast._tcp", "living-room-tv", 8009, "192.168.1.30",
                ["md=Example TV", "fn=Living Room TV", "ca=465413", "st=0"]),
            ("Living Room Speaker", "_airplay._tcp", "living-room-speaker", 7000, "192.168.1.31",
                ["model=ExampleSpeaker1,1", "srcvers=770.8.1", "flags=0x4"]),
            ("Router", "_http._tcp", "router", 80, RouterIPv4, ["path=/"]),
            ("Hallway Light", "_hap._tcp", "hallway-light", 5541, "192.168.1.50",
                ["c#=2", "md=Example Light", "sf=0", "ci=5"])
        ];

    /// <summary>
    /// Creates the services and the DNS records (PTR, SRV, TXT, A, AAAA and the service type enumeration) of the
    /// mDNS Discovery tool, consistent with each other.
    /// </summary>
    public static MDNSDiscoverySnapshot CreateMDNSDiscoverySnapshot(string networkInterface)
    {
        const uint sharedTTL = 4498; // PTR (4500 s), announced a moment ago
        const uint uniqueTTL = 118; // SRV / TXT / A / AAAA (120 s)

        var services = new List<MDNSServiceInfo>();
        var records = new List<MDNSRecordInfo>();

        void AddRecord(string name, string type, string value, bool cacheFlush, uint ttl) =>
            records.Add(new MDNSRecordInfo($"screenshot|{records.Count}", name, type, value, networkInterface, cacheFlush, ttl));

        foreach (var service in MDNSServices)
        {
            var type = service.Type + ".local.";
            var fullName = $"{service.Instance}.{type}";
            var host = service.Host + ".local.";
            var ipv6 = $"fe80::{service.IPv4.Split('.')[^1]}%12";

            services.Add(new MDNSServiceInfo(fullName, service.Instance, type, networkInterface, host, service.Port, 0, 0,
                $"{service.IPv4}; {ipv6}", string.Join(Environment.NewLine, service.Text), sharedTTL));

            AddRecord(type, "PTR", fullName, false, sharedTTL);
            AddRecord(fullName, "SRV", $"0 0 {service.Port} {host}", true, uniqueTTL);

            foreach (var text in service.Text.DefaultIfEmpty(string.Empty))
                AddRecord(fullName, "TXT", text, true, uniqueTTL);
        }

        foreach (var service in MDNSServices.DistinctBy(x => x.Host))
        {
            AddRecord(service.Host + ".local.", "A", service.IPv4, true, uniqueTTL);
            AddRecord(service.Host + ".local.", "AAAA", $"fe80::{service.IPv4.Split('.')[^1]}%12", true, uniqueTTL);
        }

        foreach (var type in MDNSServices.Select(x => x.Type + ".local.").Distinct())
            AddRecord(MDNSServiceType.Enumeration, "PTR", type, false, sharedTTL);

        return new MDNSDiscoverySnapshot(services, records);
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
