using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NETworkManager.Models.Export;
using NETworkManager.Models.Firewall;
using NETworkManager.Models.HostsFileEditor;
using NETworkManager.Models.IPApi;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;
using Newtonsoft.Json.Linq;
using Xunit;
using static NETworkManager.Tests.Export.ExportTestHelper;

namespace NETworkManager.Tests.Export;

/// <summary>
///     Exports of the tools: columns (names and order), the same names in
///     CSV / XML / JSON, one CSV value per column (also with commas and quotes in text) and the shared value formats.
/// </summary>
public class ExportManagerToolTests
{
    private static readonly IPAddress IPAddress = IPAddress.Parse("10.0.0.1");

    private static readonly DateTime Time = new(2026, 11, 8, 10, 15, 30, 123);

    #region Port Scanner

    private static PortScannerPortInfo CreatePort(PortScannerPortChange change = PortScannerPortChange.None)
    {
        return new PortScannerPortInfo(IPAddress, "host, \"a\"", 443,
            new PortLookupInfo(443, TransportProtocol.Tcp, "https", "http protocol over TLS/SSL, secure"),
            PortState.Open) { Change = change };
    }

    [Fact]
    public void PortScanner_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [CreatePort(), CreatePort(PortScannerPortChange.Opened)]));

        AssertConsistent(files, "IPAddress,Hostname,Port,Protocol,Service,Description,State,Change");
        Assert.Equal("host, \"a\"", files.CsvValue("Hostname"));
        Assert.Equal("Open", files.CsvValue("State"));
    }

    [Fact]
    public void PortScanner_Change()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [CreatePort(), CreatePort(PortScannerPortChange.Opened)]));

        // Empty without a change
        Assert.Equal("", files.CsvValue("Change"));
        Assert.Equal(JTokenType.Null, files.JsonValue("Change").Type);
        Assert.Equal("Opened", files.CsvRows[1][7]);
        Assert.Equal("Opened", (string)files.JsonItems[1]["Change"]);
    }

    #endregion

    #region Ping Monitor

    [Fact]
    public void PingMonitor_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
        [
            new PingInfo(Time, IPAddress, "host, a", 32, 4, 128, IPStatus.Success),
            new PingInfo(Time, IPAddress, "host, a", IPStatus.TimedOut)
        ]));

        AssertConsistent(files, "Timestamp,IPAddress,Hostname,Bytes,Time,TTL,Status");
        Assert.Equal("2026-11-08 10:15:30.123", files.CsvValue("Timestamp"));
        Assert.Equal("2026-11-08 10:15:30.123", files.XmlValue("Timestamp"));
        Assert.Equal("2026-11-08 10:15:30.123", (string)files.JsonValue("Timestamp"));
        Assert.Equal("host, a", files.CsvValue("Hostname"));

        // Time in ms as number
        Assert.Equal("4", files.CsvValue("Time"));
        Assert.Equal(4, (long)files.JsonValue("Time"));
        Assert.Equal(128, (int)files.JsonValue("TTL"));
    }

    [Fact]
    public void PingMonitor_NoReply_Empty()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new PingInfo(Time, IPAddress, "host", IPStatus.TimedOut)]));

        Assert.Equal("2026-11-08 10:15:30.123,10.0.0.1,\"host\",,,,TimedOut", files.CsvLines[1]);
        Assert.Equal(JTokenType.Null, files.JsonValue("Time").Type);
        Assert.Equal(JTokenType.Null, files.JsonValue("TTL").Type);
        Assert.Equal(JTokenType.Null, files.JsonValue("Bytes").Type);
    }

    #endregion

    #region Traceroute

    [Fact]
    public void Traceroute_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
        [
            new TracerouteHopInfo(1, IPStatus.Success, 1, IPStatus.TimedOut, 0, IPStatus.TtlExpired, 3, IPAddress,
                "gw, 1", new IPGeolocationResult(CreateGeolocation())),
            new TracerouteHopInfo(2, IPStatus.TimedOut, 0, IPStatus.TimedOut, 0, IPStatus.TimedOut, 0, null, null, null)
        ]));

        AssertConsistent(files,
            "Hop,Status1,Time1,Status2,Time2,Status3,Time3,IPAddress,Hostname,Continent,Country,Region,City,District," +
            "Isp,Org,As,Asname,Hosting,Proxy,Mobile");

        Assert.Equal("1", files.CsvValue("Time1"));
        Assert.Equal("", files.CsvValue("Time2"));
        Assert.Equal("3", files.CsvValue("Time3"));
        Assert.Equal("Example, Inc.", files.CsvValue("Isp"));
        Assert.Equal(JTokenType.Boolean, files.JsonValue("Proxy").Type);

        // Hop without reply and geolocation
        Assert.Equal(JTokenType.Null, files.JsonItems[1]["Time1"]!.Type);
        Assert.Equal(JTokenType.Null, files.JsonItems[1]["Country"]!.Type);
        Assert.Equal(JTokenType.Null, files.JsonItems[1]["Proxy"]!.Type);
    }

    #endregion

    #region Network Interface

    private static NetworkInterfaceInfo CreateNetworkInterface()
    {
        return new NetworkInterfaceInfo
        {
            Id = "{GUID}", Name = "Ethernet, 2", Description = "Intel(R) Ethernet \"I219\"", Type = "Ethernet",
            PhysicalAddress = PhysicalAddress.Parse("00-11-22-AA-BB-CC"), Status = OperationalStatus.Up,
            IsOperational = true, Speed = 1000000000, IPv4ProtocolAvailable = true,
            IPv4Address = [Tuple.Create(IPAddress, IPAddress.Parse("255.255.255.0"))],
            IPv4Gateway = [IPAddress.Parse("10.0.0.254")], DhcpEnabled = true,
            DhcpServer = [IPAddress.Parse("10.0.0.254")], DhcpLeaseObtained = Time, DhcpLeaseExpires = Time.AddDays(1),
            IPv6ProtocolAvailable = true, IPv6Address = [IPAddress.Parse("fd00::1")],
            IPv6AddressLinkLocal = [IPAddress.Parse("fe80::1")], IPv6Gateway = [], DNSAutoconfigurationEnabled = true,
            DNSSuffix = "corp.example.com", DNSServer = [IPAddress.Parse("10.0.0.53"), IPAddress.Parse("10.0.0.54")],
            Profile = NetworkProfile.Private
        };
    }

    [Fact]
    public void NetworkInterface_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [CreateNetworkInterface()]));

        AssertConsistent(files,
            "Id,Name,Description,Type,PhysicalAddress,Status,IsOperational,Speed,Profile,IPv4ProtocolAvailable," +
            "IPv4Address,IPv4Gateway,DhcpEnabled,DhcpServer,DhcpLeaseObtained,DhcpLeaseExpires,IPv6ProtocolAvailable," +
            "IPv6AddressLinkLocal,IPv6Address,IPv6Gateway,DNSAutoconfigurationEnabled,DNSSuffix,DNSServer");
        Assert.Equal("Ethernet, 2", files.CsvValue("Name"));
        Assert.Equal("Private", files.CsvValue("Profile"));
    }

    [Fact]
    public void NetworkInterface_Values()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [CreateNetworkInterface()]));

        // MAC address like in the other tools, status as text (also in JSON)
        Assert.Equal("00:11:22:AA:BB:CC", files.CsvValue("PhysicalAddress"));
        Assert.Equal("00:11:22:AA:BB:CC", (string)files.JsonValue("PhysicalAddress"));
        Assert.Equal("Up", (string)files.JsonValue("Status"));
        Assert.Equal(1000000000, (long)files.JsonValue("Speed"));

        // Lists separated by "; ", empty list empty
        Assert.Equal("10.0.0.1/24", files.CsvValue("IPv4Address"));
        Assert.Equal("10.0.0.53; 10.0.0.54", files.CsvValue("DNSServer"));
        Assert.Equal("10.0.0.53; 10.0.0.54", files.XmlValue("DNSServer"));
        Assert.Equal("10.0.0.53; 10.0.0.54", (string)files.JsonValue("DNSServer"));
        Assert.Equal(JTokenType.Null, files.JsonValue("IPv6Gateway").Type);

        // Date and time like in the other tools
        Assert.Equal("2026-11-08 10:15:30.123", files.CsvValue("DhcpLeaseObtained"));
        Assert.Equal("2026-11-08 10:15:30.123", files.XmlValue("DhcpLeaseObtained"));
        Assert.Equal("2026-11-09 10:15:30.123", (string)files.JsonValue("DhcpLeaseExpires"));
    }

    [Fact]
    public void NetworkInterface_WithoutDhcpAndMACAddress_Empty()
    {
        var networkInterface = CreateNetworkInterface();
        networkInterface.PhysicalAddress = PhysicalAddress.None;
        networkInterface.DhcpEnabled = false;
        networkInterface.DhcpLeaseObtained = new DateTime();
        networkInterface.DhcpLeaseExpires = new DateTime();

        var files = ExportAll((f, t) => ExportManager.Export(f, t, [networkInterface]));

        Assert.Equal("", files.CsvValue("PhysicalAddress"));
        Assert.Equal("", files.CsvValue("DhcpLeaseObtained"));
        Assert.Equal(JTokenType.Null, files.JsonValue("DhcpLeaseExpires").Type);
    }

    #endregion

    #region DNS Lookup

    [Fact]
    public void DNSLookup_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new DNSLookupRecordInfo("example.com", 300, "IN", "TXT", "v=spf1 a, mx \"x\"", "1.1.1.1", "one.one", 53)]));

        AssertConsistent(files,
            "DomainName,TTL,RecordClass,RecordType,Result,NameServerIPAddress,NameServerHostName,NameServerPort");
        Assert.Equal("v=spf1 a, mx \"x\"", files.CsvValue("Result"));
        Assert.Equal(300, (int)files.JsonValue("TTL"));
    }

    #endregion

    #region SNTP Lookup

    [Fact]
    public void SNTPLookup_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
        [
            new SNTPLookupInfo("pool.ntp.org", "1.2.3.4:123",
                new SNTPDateTime
                {
                    LocalStartTime = Time, NetworkTime = Time.AddMilliseconds(5), LocalEndTime = Time.AddMilliseconds(10),
                    RoundTripDelay = 10.5, Offset = -0.25
                })
        ]));

        AssertConsistent(files, "Server,IPEndPoint,NetworkTime,LocalStartTime,LocalEndTime,Offset,RoundTripDelay");
        Assert.Equal("2026-11-08 10:15:30.128", files.CsvValue("NetworkTime"));

        // Numbers without unit (s / ms), decimal point
        Assert.Equal("-0.25", files.CsvValue("Offset"));
        Assert.Equal("10.5", files.XmlValue("RoundTripDelay"));
        Assert.Equal(-0.25, (double)files.JsonValue("Offset"));
        Assert.Equal(JTokenType.Float, files.JsonValue("RoundTripDelay").Type);
    }

    #endregion

    #region Connections, Listeners, Neighbor Table

    [Fact]
    public void Connections_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
        [
            new ConnectionInfo(TransportProtocol.Tcp, IPAddress, 50000, IPAddress.Parse("1.2.3.4"), 443,
                "a, b.example.com", TcpState.Established, 1234, "chrome", @"C:\Program Files\chrome, x.exe")
        ]));

        AssertConsistent(files,
            "Protocol,LocalIPAddress,LocalPort,RemoteIPAddress,RemotePort,RemoteHostname,TcpState,ProcessId," +
            "ProcessName,ProcessPath");
        Assert.Equal("Established", (string)files.JsonValue("TcpState"));
        Assert.Equal(@"C:\Program Files\chrome, x.exe", files.CsvValue("ProcessPath"));
    }

    [Fact]
    public void Listeners_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new ListenerInfo(TransportProtocol.Udp, IPAddress.Parse("::"), 53)]));

        AssertConsistent(files, "Protocol,IPAddress,Port");
        Assert.Equal("Udp,::,53", files.CsvLines[1]);
    }

    [Fact]
    public void NeighborTable_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
        [
            new NeighborInfo(IPAddress, PhysicalAddress.Parse("00-11-22-AA-BB-CC"), false, 12, "Ethernet, 2",
                NeighborState.Reachable, AddressFamily.InterNetwork)
        ]));

        AssertConsistent(files, "IPAddress,MACAddress,InterfaceAlias,InterfaceIndex,State,AddressFamily,IsMulticast");
        Assert.Equal("00:11:22:AA:BB:CC", files.CsvValue("MACAddress"));
        Assert.Equal("00:11:22:AA:BB:CC", (string)files.JsonValue("MACAddress"));
        Assert.Equal("Reachable", (string)files.JsonValue("State"));
        Assert.Equal("Ethernet, 2", files.CsvValue("InterfaceAlias"));
    }

    #endregion

    #region Discovery Protocol

    [Fact]
    public void DiscoveryProtocol_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
        [
            new DiscoveryProtocolPackageInfo
            {
                Device = "sw1", DeviceDescription = "Cisco IOS, Version 15", Port = "Gi0/1", PortDescription = "Uplink",
                Model = "C2960", VLAN = "10", IPAddress = "10.0.0.2", Protocol = "CDP", TimeToLive = "180",
                Management = "10.0.0.2", ChassisId = "00:11:22:33:44:55", LocalConnection = "Ethernet",
                LocalInterface = "Intel(R) Ethernet"
            }
        ]));

        AssertConsistent(files,
            "Device,DeviceDescription,Port,PortDescription,Model,VLAN,IPAddress,Protocol,TimeToLive,Management," +
            "ChassisId,LocalConnection,LocalInterface");
        Assert.Equal("Cisco IOS, Version 15", files.CsvValue("DeviceDescription"));
        Assert.Equal("Intel(R) Ethernet", (string)files.JsonValue("LocalInterface"));
    }

    #endregion

    #region Hosts File Editor, Firewall, IP Geolocation

    [Fact]
    public void HostsFileEditor_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new HostsFileEntry(true, "10.0.0.1", "a.local b.local", "comment, with \"quotes\"")]));

        AssertConsistent(files, "IsEnabled,IPAddress,Hostname,Comment");
        Assert.Equal("comment, with \"quotes\"", files.CsvValue("Comment"));
        Assert.Equal("True", files.CsvValue("IsEnabled"));
        Assert.Equal("true", files.XmlValue("IsEnabled"));
        Assert.Equal(JTokenType.Boolean, files.JsonValue("IsEnabled").Type);
    }

    [Fact]
    public void Firewall_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new FirewallRule { Name = "Rule, 1", Description = "Description \"x\"" }]));

        AssertConsistent(files,
            "Name,IsEnabled,IsManaged,Source,Direction,Action,Protocol,LocalPorts,RemotePorts,LocalAddresses," +
            "RemoteAddresses,NetworkProfiles,InterfaceType,Program,Description");
        Assert.Equal("Rule, 1", files.CsvValue("Name"));
        Assert.Equal("Inbound", (string)files.JsonValue("Direction"));
    }

    [Fact]
    public void IPGeolocation_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [CreateGeolocation()]));

        AssertConsistent(files,
            "Status,Continent,ContinentCode,Country,CountryCode,Region,RegionName,City,District,Zip,Lat,Lon,Timezone," +
            "Offset,Currency,Isp,Org,As,Asname,Reverse,Mobile,Proxy,Hosting,Query");

        // Decimal point in every language
        Assert.Equal("48.1374", files.CsvValue("Lat"));
        Assert.Equal("11.5755", files.XmlValue("Lon"));
        Assert.Equal(48.1374, (double)files.JsonValue("Lat"));
        Assert.Equal("Example, Inc.", files.CsvValue("Isp"));
        Assert.Equal("Org \"Quoted\"", files.CsvValue("Org"));
    }

    #endregion

    #region Subnet Calculator, Bit Calculator, SNMP

    [Fact]
    public void SubnetCalculator_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new IPNetworkInfo(IPNetwork2.Parse("10.0.0.0/24")), new IPNetworkInfo(IPNetwork2.Parse("2001:db8::/32"))]));

        AssertConsistent(files, "Network,Broadcast,Total,Netmask,Cidr,FirstUsable,LastUsable,Usable");
        Assert.Equal("10.0.0.0,10.0.0.255,256,255.255.255.0,24,10.0.0.1,10.0.0.254,254", files.CsvLines[1]);
        Assert.Equal(256, (int)files.JsonValue("Total"));

        // Large numbers (IPv6) as JSON number
        Assert.Equal(JTokenType.Integer, files.JsonItems[1]["Total"]!.Type);
    }

    [Fact]
    public void BitCalculator_DecimalPoint()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new BitCaluclatorInfo(1.5, 0.1875, 0.0015, 0.00018, 1, 2, 3, 4, 5, 6, 7, 8)]));

        AssertConsistent(files,
            "Bits,Bytes,Kilobits,Kilobytes,Megabits,Megabytes,Gigabits,Gigabytes,Terabits,Terabytes,Petabits,Petabytes");
        Assert.Equal("1.5,0.1875,0.0015,0.00018,1,2,3,4,5,6,7,8", files.CsvLines[1]);
        Assert.Equal("0.1875", files.XmlValue("Bytes"));
        Assert.Equal(1.5, (double)files.JsonValue("Bits"));
    }

    [Fact]
    public void SNMP_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
        [
            new SNMPInfo(new Lextm.SharpSnmpLib.ObjectIdentifier("1.3.6.1.2.1.1.1.0"),
                new Lextm.SharpSnmpLib.OctetString("Linux sw1, 5.10 \"x\""))
        ]));

        AssertConsistent(files, "OID,Data");
        Assert.Equal("Linux sw1, 5.10 \"x\"", files.CsvValue("Data"));
    }

    #endregion

    private static IPGeolocationInfo CreateGeolocation()
    {
        return new IPGeolocationInfo
        {
            Status = "success", Continent = "Europe", ContinentCode = "EU", Country = "Germany", CountryCode = "DE",
            Region = "BY", RegionName = "Bavaria", City = "Munich", District = "", Zip = "80331", Lat = 48.1374,
            Lon = 11.5755, Timezone = "Europe/Berlin", Offset = 3600, Currency = "EUR", Isp = "Example, Inc.",
            Org = "Org \"Quoted\"", As = "AS1 Example", Asname = "EXAMPLE", Reverse = "host.example.com",
            Mobile = false, Proxy = true, Hosting = false, Query = "1.2.3.4"
        };
    }
}
