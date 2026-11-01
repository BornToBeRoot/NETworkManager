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
        "WebCertificateIssuer,WebCertificateNotAfter,WebCertificateSelfSigned,Change,PreviousMACAddress,SNMPError";

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

    [Fact]
    public void Json_SameKeysAsCsv()
    {
        var host = (JObject)JArray.Parse(Export(ExportFileType.Json, CreateHost())).Single();

        Assert.Equal(CsvHeader.Split(','), host.Properties().Select(x => x.Name));
    }
}
