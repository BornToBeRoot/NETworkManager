using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using Newtonsoft.Json.Linq;
using Xunit;

namespace NETworkManager.Tests.Network;

public class ExportManagerIPScannerTests
{
    /// <summary>
    ///     Columns of the CSV export - scripts read them by position: released columns must not move, new ones are
    ///     appended at the end.
    /// </summary>
    private const string CsvHeader =
        "IsReachable,IPAddress,Hostname,PingStatus,Timestamp,Time,TTL,Bytes,PortStatus,Ports,NetBIOSIsReachable," +
        "NetBIOSIPAddress,NetBIOSComputerName,NetBIOSUserName,NetBIOSGroupName,NetBIOSMACAddress,NetBIOSVendor," +
        "MACAddress,Vendor,DNSHostname,DetectedBy,OSGuess,Role,MDNSHostname,WebTitle,WebURL,SNMPName,SNMPDescription," +
        "SNMPLocation,WebStatusCode,WebServer,WebPoweredBy,WebRealm,WebRedirectURL,WebCertificateSubject," +
        "WebCertificateIssuer,WebCertificateNotAfter,WebCertificateSelfSigned,Change,PreviousMACAddress,SNMPError," +
        "WindowsOS,WindowsEdition,WindowsVersion,WindowsRole,WindowsComputerName,WindowsDomain,WindowsTimeOffset," +
        "WindowsUsers,WindowsDescription,WindowsError,SMBDialect,SMBSigningRequired,SMBServerGuid";

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
        Assert.Equal("-185.500", (string)host["WindowsTimeOffset"]);
        Assert.Equal(@"CORP\alice; CORP\bob", (string)host["WindowsUsers"]);
        Assert.Equal(string.Empty, (string)host["WindowsError"]);
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
        // The description contains a comma (quoted)
        var row = Export(ExportFileType.Csv, CreateWindowsHost()).Split(Environment.NewLine)[1];

        Assert.Contains("\"Reception, 1st floor\"", row);
    }

    [Fact]
    public void Json_SameKeysAsCsv()
    {
        var host = (JObject)JArray.Parse(Export(ExportFileType.Json, CreateHost())).Single();

        Assert.Equal(CsvHeader.Split(','), host.Properties().Select(x => x.Name));
    }
}
