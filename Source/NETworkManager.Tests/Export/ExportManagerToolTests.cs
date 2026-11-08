using System;
using System.Net;
using System.Net.NetworkInformation;
using NETworkManager.Models.Export;
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
}
