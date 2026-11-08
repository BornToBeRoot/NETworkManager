using System;
using System.IO;
using NETworkManager.Models.Export;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;
using Newtonsoft.Json.Linq;
using Xunit;
using static NETworkManager.Tests.Export.ExportTestHelper;

namespace NETworkManager.Tests.Export;

/// <summary>
///     Format rules of the shared CSV / XML / JSON writer (tested with the Lookup exports).
/// </summary>
public class ExportManagerTests
{
    [Fact]
    public void PortLookup_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new PortLookupInfo(443, TransportProtocol.Tcp, "https", "http protocol over TLS/SSL")]));

        AssertConsistent(files, "Number,Protocol,Service,Description");
    }

    [Fact]
    public void OUI_Consistent()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [new OUIInfo("00:11:22", "Cisco Systems, Inc")]));

        AssertConsistent(files, "MACAddress,Vendor");
    }

    [Fact]
    public void Csv_TextWithCommaAndQuote_Quoted()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new PortLookupInfo(443, TransportProtocol.Tcp, "https", "Secure, \"TLS\"")]));

        Assert.Equal("443,Tcp,\"https\",\"Secure, \"\"TLS\"\"\"", files.CsvLines[1]);
        Assert.Equal("Secure, \"TLS\"", files.CsvValue("Description"));
    }

    [Fact]
    public void Csv_ValueWithCommaNotMarkedAsText_Quoted()
    {
        // The MAC address column is not quoted - a value with a comma must still not break the line
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [new OUIInfo("00,11", "Vendor")]));

        Assert.Equal("\"00,11\",\"Vendor\"", files.CsvLines[1]);
    }

    [Fact]
    public void Empty_IsEmpty()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [new OUIInfo("00:11:22", null)]));

        Assert.Equal("", files.CsvValue("Vendor"));
        Assert.Equal("", files.XmlValue("Vendor"));
        Assert.Equal(JTokenType.Null, files.JsonValue("Vendor").Type);
    }

    [Fact]
    public void Csv_EmptyValueVsEmptyText()
    {
        // Empty value: empty (also in a quoted column), empty text: "" - like null and "" in JSON
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new OUIInfo("00:11:22", null), new OUIInfo("00:11:23", "")]));

        Assert.Equal("00:11:22,", files.CsvLines[1]);
        Assert.Equal("00:11:23,\"\"", files.CsvLines[2]);
        Assert.Equal(JTokenType.Null, files.JsonItems[0]["Vendor"]!.Type);
        Assert.Equal("", (string)files.JsonItems[1]["Vendor"]);
    }

    [Fact]
    public void Json_NumberAsNumber_EnumAsText()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t,
            [new PortLookupInfo(53, TransportProtocol.Udp, "domain", "DNS")]));

        Assert.Equal(JTokenType.Integer, files.JsonValue("Number").Type);
        Assert.Equal("Udp", (string)files.JsonValue("Protocol"));
        Assert.Equal("Udp", files.XmlValue("Protocol"));
        Assert.Equal("Udp", files.CsvValue("Protocol"));
    }

    [Fact]
    public void Xml_Structure()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [new OUIInfo("00:11:22", "Vendor")]));

        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>", files.Xml);
        Assert.Contains("<Lookup>", files.Xml);
        Assert.Contains("<OUIInfos>", files.Xml);
        Assert.Equal("OUIInfo", files.XmlItems[0].Name.LocalName);
    }

    [Fact]
    public void NoItems_HeaderOnly()
    {
        var files = ExportAll((f, t) => ExportManager.Export(f, t, Array.Empty<OUIInfo>()));

        Assert.Single(files.CsvLines);
        Assert.Empty(files.XmlItems);
        Assert.Empty(files.JsonItems);
    }

    [Fact]
    public void Xml_InvalidCharactersRemoved()
    {
        // Control characters (e.g. in SNMP data) are not allowed in XML 1.0 - the export must not fail
        var files = ExportAll((f, t) => ExportManager.Export(f, t, [new OUIInfo("00:11:22", "Ven\0dor\u001B \U0001F600")]));

        Assert.Equal("Vendor \U0001F600", files.XmlValue("Vendor"));
        Assert.Equal("Ven\0dor\u001B \U0001F600", (string)files.JsonValue("Vendor"));
    }

    [Fact]
    public void Txt_NotSupported()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExportManager.Export(Path.Combine(Path.GetTempPath(), "export.txt"), ExportFileType.Txt,
                Array.Empty<OUIInfo>()));
    }
}
