using System;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using Xunit;
using static NETworkManager.Tests.Export.ExportTestHelper;

namespace NETworkManager.Tests.Export;

public class ExportManagerMDNSServiceInfoTests
{
    [Fact]
    public void ExportsPrinterDetailsWithTypedPortAndMultilineTXT()
    {
        var service = new MDNSServiceInfo("Office Printer._ipp._tcp.local.", "Office, Printer", "_ipp._tcp.local.",
            "Ethernet", "printer.local.", 631, 0, 0, "192.168.1.20; fe80::20%7",
            "ty=Office \"Printer\"" + Environment.NewLine + "rp=ipp/print", 120);
        var files = ExportAll((path, type) => ExportManager.Export(path, type, new[] { service }));
        var json = Assert.Single(files.JsonItems);
        Assert.Equal(631, (int)json["Port"]);
        Assert.Equal(service.TXTRecords, (string)json["TXTRecords"]);
        Assert.Equal(service.Name, Assert.Single(files.XmlItems).Element("Name")?.Value);
        Assert.Contains("\"Office, Printer\"", files.Csv);
        Assert.Contains("\"\"Printer\"\"", files.Csv);
    }

    [Fact]
    public void UnresolvedServiceHasNullPortInExport()
    {
        var service = new MDNSServiceInfo("Printer._ipp._tcp.local.", "Printer", "_ipp._tcp.local.",
            "Ethernet", "", null, null, null, "", "", 120);
        var files = ExportAll((path, type) => ExportManager.Export(path, type, new[] { service }));
        Assert.Equal(Newtonsoft.Json.Linq.JTokenType.Null, Assert.Single(files.JsonItems)["Port"]?.Type);
    }

    [Fact]
    public void ExportsDNSRecordsWithTypedTTLAndSingleLineValue()
    {
        var record = new MDNSRecordInfo("id", "Printer._ipp._tcp.local.", "SRV", "0 0 631 printer.local.", "Ethernet", true, 119);
        var files = ExportAll((path, type) => ExportManager.Export(path, type, new[] { record }));
        var json = Assert.Single(files.JsonItems);
        Assert.Equal(119, (int)json["TTL"]);
        Assert.True((bool)json["CacheFlush"]);
        Assert.Equal(record.Value, (string)json["Value"]);
        Assert.Equal(record.Value, Assert.Single(files.XmlItems).Element("Value")?.Value);
        Assert.Equal(7, files.CsvHeader.Count);
    }
}
