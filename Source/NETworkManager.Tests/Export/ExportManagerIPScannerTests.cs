using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Models.Lookup;
using Newtonsoft.Json.Linq;
using Xunit;
using static NETworkManager.Tests.Export.ExportTestHelper;

namespace NETworkManager.Tests.Export;

public class ExportManagerIPScannerTests
{
    /// <summary>
    ///     Columns of the CSV export (names and order), grouped by topic.
    /// </summary>
    private const string CsvHeader =
        "IsReachable,DetectedBy,IPAddress,Hostname,DNSHostname,MDNSHostname,MACAddress,Vendor,Role,OSGuess," +
        "PingStatus,Timestamp,Time,TTL,Bytes," +
        "PortStatus,Ports," +
        "NetBIOSIsReachable,NetBIOSIPAddress,NetBIOSComputerName,NetBIOSUserName,NetBIOSGroupName,NetBIOSMACAddress," +
        "NetBIOSVendor," +
        "WebURL,WebStatusCode,WebTitle,WebServer,WebPoweredBy,WebRealm,WebRedirectURL,WebCertificateSubject," +
        "WebCertificateIssuer,WebCertificateNotAfter,WebCertificateSelfSigned," +
        "SNMPName,SNMPDescription,SNMPLocation,SNMPError," +
        "WindowsOS,WindowsEdition,WindowsVersion,WindowsRole,WindowsComputerName,WindowsDomain,WindowsTimeOffset," +
        "WindowsUsers,WindowsDescription,WindowsError,SMBDialect,SMBSigningRequired,SMBServerGuid," +
        "Change,PreviousMACAddress";

    private static readonly IPScannerResultOptions AllEnabled = new(true, true, true, true, true, true, true, true, true, true);

    private static string Export(ExportFileType fileType, params IPScannerHostInfo[] hosts)
    {
        return Export(fileType, AllEnabled, hosts);
    }

    private static string Export(ExportFileType fileType, IPScannerResultOptions options,
        params IPScannerHostInfo[] hosts)
    {
        var filePath = Path.GetTempFileName();

        try
        {
            ExportManager.Export(filePath, fileType, hosts, options);

            return File.ReadAllText(filePath);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private static IPScannerHostInfo CreateHost()
    {
        var ipAddress = IPAddress.Parse("10.0.0.1");

        return new IPScannerHostInfo(ipAddress, true, new PingInfo(DateTime.Now, ipAddress, IPStatus.Success), false,
            [], new NetBIOSInfo(ipAddress));
    }

    [Fact]
    public void Csv_Header()
    {
        Assert.Equal(CsvHeader, Export(ExportFileType.Csv).Split(Environment.NewLine)[0]);
    }

    [Fact]
    public void Csv_OneValuePerColumn()
    {
        var row = Export(ExportFileType.Csv, CreateHost()).Split(Environment.NewLine)[1];

        // No comma in the values of the host (empty values are quoted or empty)
        Assert.Equal(CsvHeader.Split(',').Length, row.Split(',').Length);
    }

    [Fact]
    public void Xml_SameElementsAsCsv()
    {
        var host = XDocument.Parse(Export(ExportFileType.Xml, CreateHost())).Descendants(nameof(IPScannerHostInfo))
            .Single();

        // Without ports: no element for the list (one element per port)
        Assert.Equal(CsvHeader.Split(',').Where(x => x != "Ports"), host.Elements().Select(x => x.Name.LocalName));
    }

    [Fact]
    public void Json_WebPageWithoutPortScan_Empty()
    {
        // Web pages are only read with the port scan - the values are empty (null), like the ones of a disabled probe
        var options = AllEnabled with { PortScanEnabled = false };

        var host = (JObject)JArray.Parse(Export(ExportFileType.Json, options, CreateHost())).Single();

        Assert.Equal(JTokenType.Null, host["WebTitle"]!.Type);
        Assert.Equal(JTokenType.Null, host["WebURL"]!.Type);
    }

    private static IPScannerHostInfo CreateWindowsHost()
    {
        var host = CreateHost();

        var ntlm = new NTLMChallengeInfo(new Version(10, 0, 26200), "CLIENT01", "CORP", "client01.corp.example.com",
            "corp.example.com", "example.com", null);
        var smb = new SMBInfo("3.1.1", true, Guid.Parse("8c5b6f4e-3d2a-4b1c-9e8f-7a6b5c4d3e2f"), DateTime.UtcNow,
            TimeSpan.FromSeconds(-185.5), ntlm);
        var netApi = new WindowsNetApiInfo(0, WindowsHostRole.Workstation, true, "Reception, 1st floor", "CORP",
            [@"CORP\alice", @"CORP\bob"], 0, null);

        // The value is set by the scan (internal setter)
        typeof(IPScannerHostInfo).GetProperty(nameof(IPScannerHostInfo.Windows))!
            .SetValue(host, new WindowsHostInfo(smb, netApi));

        return host;
    }

    [Fact]
    public void Json_WindowsValues()
    {
        var host = (JObject)JArray.Parse(Export(ExportFileType.Json, CreateWindowsHost())).Single();

        Assert.Equal("Windows 11 25H2", (string)host["WindowsOS"]);
        Assert.Equal("10.0.26200", (string)host["WindowsVersion"]);
        Assert.Equal("Workstation", (string)host["WindowsRole"]);
        Assert.Equal("corp.example.com", (string)host["WindowsDomain"]);
        Assert.Equal(-185.5, (double)host["WindowsTimeOffset"]);
        Assert.Equal(JTokenType.Boolean, host["SMBSigningRequired"]!.Type);
        Assert.Equal(@"CORP\alice; CORP\bob", (string)host["WindowsUsers"]);
        Assert.Equal(JTokenType.Null, host["WindowsError"]!.Type);
        Assert.Equal("3.1.1", (string)host["SMBDialect"]);
        Assert.Equal("8c5b6f4e-3d2a-4b1c-9e8f-7a6b5c4d3e2f", (string)host["SMBServerGuid"]);
    }

    [Fact]
    public void Json_WindowsDisabled_Empty()
    {
        var options = AllEnabled with { WindowsEnabled = false };

        var host = (JObject)JArray.Parse(Export(ExportFileType.Json, options, CreateWindowsHost())).Single();

        Assert.Equal(JTokenType.Null, host["WindowsOS"]!.Type);
        Assert.Equal(JTokenType.Null, host["SMBServerGuid"]!.Type);
    }

    [Fact]
    public void Csv_WindowsValues_OneValuePerColumn()
    {
        // The description contains a comma (quoted), the time offset has a decimal point in every culture
        var culture = System.Globalization.CultureInfo.CurrentCulture;

        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");

            var row = Export(ExportFileType.Csv, CreateWindowsHost()).Split(Environment.NewLine)[1];

            Assert.Contains("\"Reception, 1st floor\"", row);
            Assert.Contains(",-185.5,", row);
            Assert.Equal(CsvHeader.Split(',').Length, row.Replace("\"Reception, 1st floor\"", "x").Split(',').Length);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public void Json_SameKeysAsCsv()
    {
        var host = (JObject)JArray.Parse(Export(ExportFileType.Json, CreateHost())).Single();

        Assert.Equal(CsvHeader.Split(','), host.Properties().Select(x => x.Name));
    }

    private static IPScannerHostInfo CreateHostWithPorts()
    {
        var ipAddress = IPAddress.Parse("10.0.0.1");

        return new IPScannerHostInfo(ipAddress, true,
            new PingInfo(new DateTime(2026, 11, 8, 10, 15, 30, 123), ipAddress, null, 32, 4, 64, IPStatus.Success), true,
            [
                new PortInfo(80, new PortLookupInfo(80, TransportProtocol.Tcp, "http", "World Wide Web, HTTP"),
                    PortState.Open),
                new PortInfo(443, new PortLookupInfo(443, TransportProtocol.Tcp, "https", "TLS"), PortState.Closed)
            ], new NetBIOSInfo(ipAddress));
    }

    [Fact]
    public void Ping_TimeInMilliseconds()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [CreateHostWithPorts()], AllEnabled));

        Assert.Equal("2026-11-08 10:15:30.123", files.CsvValue("Timestamp"));
        Assert.Equal("4", files.CsvValue("Time"));
        Assert.Equal("4", files.XmlValue("Time"));
        Assert.Equal(4, (long)files.JsonValue("Time"));
        Assert.Equal(64, (int)files.JsonValue("TTL"));
    }

    [Fact]
    public void Ping_NoReply_TimeTTLBytesEmpty()
    {
        var ipAddress = IPAddress.Parse("10.0.0.2");
        var host = new IPScannerHostInfo(ipAddress, false, new PingInfo(DateTime.Now, ipAddress, IPStatus.TimedOut),
            false, [], new NetBIOSInfo(ipAddress));

        var files = ExportAll((f, t) => ExportManager.Export(f, t, [host], AllEnabled));

        foreach (var name in new[] { "Time", "TTL", "Bytes" })
        {
            Assert.Equal("", files.CsvValue(name));
            Assert.Equal("", files.XmlValue(name));
            Assert.Equal(JTokenType.Null, files.JsonValue(name).Type);
        }

        Assert.Equal("TimedOut", files.CsvValue("PingStatus"));
    }

    [Fact]
    public void Ports_AllFormats()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [CreateHostWithPorts()], AllEnabled));

        Assert.Equal(CsvHeader.Split(',').Length, files.CsvRows[0].Count);
        Assert.Equal("80/Tcp/http/World Wide Web, HTTP/Open;443/Tcp/https/TLS/Closed", files.CsvValue("Ports"));

        // XML: one element per port (no element for the list)
        var ports = files.XmlItems[0].Elements(nameof(PortInfo)).ToList();
        Assert.Equal(2, ports.Count);
        Assert.Equal(["Port", "Protocol", "Service", "Description", "State"],
            ports[0].Elements().Select(x => x.Name.LocalName));
        Assert.Equal("Tcp", ports[0].Element("Protocol")!.Value);

        // JSON: array of objects
        var json = (JArray)files.JsonValue("Ports");
        Assert.Equal(2, json.Count);
        Assert.Equal(80, (int)json[0]["Port"]!);
        Assert.Equal("Tcp", (string)json[0]["Protocol"]);
        Assert.Equal("Open", (string)json[0]["State"]);
    }

    [Fact]
    public void Ports_PortScanDisabled_Empty()
    {
        var options = AllEnabled with { PortScanEnabled = false };

        var files = ExportAll((f, t) => ExportManager.Export(f, t, [CreateHostWithPorts()], options));

        Assert.Equal("", files.CsvValue("Ports"));
        Assert.Equal("", files.CsvValue("PortStatus"));
        Assert.Empty(files.XmlItems[0].Elements(nameof(PortInfo)));
        Assert.Empty((JArray)files.JsonValue("Ports"));
    }
}
