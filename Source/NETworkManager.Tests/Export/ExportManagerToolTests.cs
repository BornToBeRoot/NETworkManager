using System;
using System.Net;
using System.Net.NetworkInformation;
using NETworkManager.Models.Export;
using NETworkManager.Models.IPApi;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;
using Newtonsoft.Json.Linq;
using Xunit;
using static NETworkManager.Tests.Export.ExportTestHelper;

namespace NETworkManager.Tests.Export;

/// <summary>
///     Exports of the tools: columns (released columns must not move, new ones are appended at the end), the same names in
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
