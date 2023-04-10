using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

/// <summary>
/// This class will provide methods to export text or objects to files like csv, xml, json or plain text.
/// </summary>
public static partial class ExportManager
{
    #region Variables
    /// <summary>
    /// Default declaration for XML documents.
    /// </summary>
    private static readonly XDeclaration DefaultXDeclaration = new XDeclaration("1.0", "utf-8", "yes");
    #endregion

    #region Methods

    #region Export
    /// <summary>
    /// Method to export objects from type <see cref="WiFiNetworkInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{WiFiNetworkInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<WiFiNetworkInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="IPScannerHostInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{HostInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<IPScannerHostInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="PortScannerPortInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{PortScannerPortInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<PortScannerPortInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="PingInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{PingInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<PingInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="TracerouteHopInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{TracerouteHopInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<TracerouteHopInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="DNSLookupRecordInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{DNSLookupRecordInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<DNSLookupRecordInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="SNMPReceivedInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{SNMPReceivedInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<SNMPReceivedInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="SNTPLookupResultInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{SNTPLookupResultInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<SNTPLookupResultInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="IPNetworkInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{IPNetworkInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<IPNetworkInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="OUIInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{OUIInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<OUIInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="PortLookupInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{PortLookupInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<PortLookupInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="ConnectionInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{ConnectionInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<ConnectionInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="ListenerInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{ListenerInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<ListenerInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export objects from type <see cref="ARPInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="ObservableCollection{ARPInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, ObservableCollection<ARPInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCSV(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXML(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJSON(collection, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Method to export text to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="content">Text to export.</param>
    public static void Export(string filePath, string content)
    {
        CreateTXT(content, filePath);
    }
    #endregion

    #region CreateCSV
    private static void CreateCSV(IEnumerable<WiFiNetworkInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(WiFiNetworkInfo.AvailableNetwork.Bssid)},{nameof(WiFiNetworkInfo.AvailableNetwork.Ssid)},{nameof(WiFiNetworkInfo.AvailableNetwork.ChannelCenterFrequencyInKilohertz)},{nameof(WiFiNetworkInfo.AvailableNetwork.SignalBars)},{nameof(WiFiNetworkInfo.AvailableNetwork.IsWiFiDirect)},{nameof(WiFiNetworkInfo.AvailableNetwork.NetworkRssiInDecibelMilliwatts)},{nameof(WiFiNetworkInfo.AvailableNetwork.PhyKind)},{nameof(WiFiNetworkInfo.AvailableNetwork.NetworkKind)},{nameof(WiFiNetworkInfo.AvailableNetwork.SecuritySettings.NetworkAuthenticationType)},{nameof(WiFiNetworkInfo.AvailableNetwork.SecuritySettings.NetworkEncryptionType)},{nameof(WiFiNetworkInfo.AvailableNetwork.BeaconInterval)}.{nameof(WiFiNetworkInfo.AvailableNetwork.Uptime)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.AvailableNetwork.Bssid},{info.AvailableNetwork.Ssid},{info.AvailableNetwork.ChannelCenterFrequencyInKilohertz},{info.AvailableNetwork.SignalBars},{info.AvailableNetwork.IsWiFiDirect},{info.AvailableNetwork.NetworkRssiInDecibelMilliwatts},{info.AvailableNetwork.PhyKind},{info.AvailableNetwork.NetworkKind},{info.AvailableNetwork.SecuritySettings.NetworkAuthenticationType},{info.AvailableNetwork.SecuritySettings.NetworkEncryptionType},{info.AvailableNetwork.BeaconInterval},{info.AvailableNetwork.Uptime}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<IPScannerHostInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Status,{nameof(PingInfo.IPAddress)},{nameof(IPScannerHostInfo.Hostname)},PortStatus,PingStatus,{nameof(IPScannerHostInfo.MACAddress)},{nameof(IPScannerHostInfo.Vendor)},{nameof(IPScannerHostInfo.Ports)},{nameof(PingInfo.Bytes)},{nameof(PingInfo.Time)},{nameof(PingInfo.TTL)}");
        
        foreach (var info in collection)
        {
            var stringBuilderPorts = new StringBuilder();

            foreach (var port in info.Ports)
                stringBuilderPorts.Append($"{port.Port}/{port.LookupInfo.Protocol}/{port.LookupInfo.Service}/{port.LookupInfo.Description}/{port.State};");

            stringBuilder.AppendLine($"{info.IsReachable},{info.PingInfo.IPAddress},{info.Hostname},{(info.IsAnyPortOpen ? PortState.Open : PortState.Closed)},{info.PingInfo.Status},{info.MACAddress},\"{info.Vendor}\",\"{stringBuilderPorts.ToString().TrimEnd(';')}\",{info.PingInfo.Bytes},{Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)},{info.PingInfo.TTL}");
        }

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<PortScannerPortInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(PortScannerPortInfo.IPAddress)},{nameof(PortScannerPortInfo.Hostname)},{nameof(PortScannerPortInfo.Port)},{nameof(PortLookupInfo.Protocol)},{nameof(PortLookupInfo.Service)},{nameof(PortLookupInfo.Description)},{nameof(PortScannerPortInfo.State)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.IPAddress},{info.Hostname},{info.Port},{info.LookupInfo.Protocol},{info.LookupInfo.Service},\"{info.LookupInfo.Description}\",{info.State}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<PingInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(PingInfo.Timestamp)},{nameof(PingInfo.IPAddress)},{nameof(PingInfo.Hostname)},{nameof(PingInfo.Bytes)},{nameof(PingInfo.Time)},{nameof(PingInfo.TTL)},{nameof(PingInfo.Status)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.Timestamp},{info.IPAddress},{info.Hostname},{info.Bytes},{Ping.TimeToString(info.Status, info.Time, true)},{info.TTL},{info.Status}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<TracerouteHopInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(TracerouteHopInfo.Hop)},{nameof(TracerouteHopInfo.Time1)},{nameof(TracerouteHopInfo.Time2)},{nameof(TracerouteHopInfo.Time3)},{nameof(TracerouteHopInfo.IPAddress)},{nameof(TracerouteHopInfo.Hostname)},{nameof(TracerouteHopInfo.Status1)},{nameof(TracerouteHopInfo.Status2)},{nameof(TracerouteHopInfo.Status3)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.Hop},{Ping.TimeToString(info.Status1, info.Time1, true)},{Ping.TimeToString(info.Status2, info.Time2, true)},{Ping.TimeToString(info.Status3, info.Time3, true)},{info.IPAddress},{info.Hostname},{info.Status1},{info.Status2},{info.Status3}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<DNSLookupRecordInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(DNSLookupRecordInfo.DomainName)},{nameof(DNSLookupRecordInfo.TTL)},{nameof(DNSLookupRecordInfo.Class)},{nameof(DNSLookupRecordInfo.Type)},{nameof(DNSLookupRecordInfo.Result)},{nameof(DNSLookupRecordInfo.Server)},{nameof(DNSLookupRecordInfo.IPEndPoint)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.DomainName},{info.TTL},{info.Class},{info.Type},{info.Result},{info.Server},{info.IPEndPoint}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<SNMPReceivedInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(SNMPReceivedInfo.OID)},{nameof(SNMPReceivedInfo.Data)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.OID},{info.Data}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<SNTPLookupResultInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(SNTPLookupResultInfo.Server)},{nameof(SNTPLookupResultInfo.IPEndPoint)},{nameof(SNTPLookupResultInfo.DateTime.NetworkTime)},{nameof(SNTPLookupResultInfo.DateTime.LocalStartTime)},{nameof(SNTPLookupResultInfo.DateTime.LocalEndTime)},{nameof(SNTPLookupResultInfo.DateTime.Offset)},{nameof(SNTPLookupResultInfo.DateTime.RoundTripDelay)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.Server},{info.IPEndPoint},{info.DateTime.NetworkTime.ToString("yyyy.MM.dd HH:mm:ss.fff")},{info.DateTime.LocalStartTime.ToString("yyyy.MM.dd HH:mm:ss.fff")},{info.DateTime.LocalEndTime.ToString("yyyy.MM.dd HH:mm:ss.fff")},{info.DateTime.Offset} s,{info.DateTime.RoundTripDelay} ms");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<IPNetworkInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(IPNetworkInfo.Network)},{nameof(IPNetworkInfo.Broadcast)},{nameof(IPNetworkInfo.Total)},{nameof(IPNetworkInfo.Netmask)},{nameof(IPNetworkInfo.Cidr)},{nameof(IPNetworkInfo.FirstUsable)},{nameof(IPNetworkInfo.LastUsable)},{nameof(IPNetworkInfo.Usable)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.Network},{info.Broadcast},{info.Total},{info.Netmask},{info.Cidr},{info.FirstUsable},{info.LastUsable},{info.Usable}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<OUIInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(OUIInfo.MACAddress)},{nameof(OUIInfo.Vendor)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.MACAddress},\"{info.Vendor}\"");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<PortLookupInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(PortLookupInfo.Number)},{nameof(PortLookupInfo.Protocol)},{nameof(PortLookupInfo.Service)},{nameof(PortLookupInfo.Description)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.Number},{info.Protocol},{info.Service},\"{info.Description}\"");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<ConnectionInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(ConnectionInfo.Protocol)},{nameof(ConnectionInfo.LocalIPAddress)},{nameof(ConnectionInfo.LocalPort)},{nameof(ConnectionInfo.RemoteIPAddress)},{nameof(ConnectionInfo.RemotePort)},{nameof(ConnectionInfo.TcpState)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.Protocol},{info.LocalIPAddress},{info.LocalPort},{info.RemoteIPAddress},{info.RemotePort},{info.TcpState}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<ListenerInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(ListenerInfo.Protocol)},{nameof(ListenerInfo.IPAddress)},{nameof(ListenerInfo.Port)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.Protocol},{info.IPAddress},{info.Port}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateCSV(IEnumerable<ARPInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(ARPInfo.IPAddress)},{nameof(ARPInfo.MACAddress)},{nameof(ARPInfo.IsMulticast)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.IPAddress},{info.MACAddress},{info.IsMulticast}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }
    #endregion

    #region CreateXML
    public static void CreateXML(IEnumerable<WiFiNetworkInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.IPScanner.ToString(),
                new XElement(nameof(IPScannerHostInfo) + "s",

                from info in collection
                select
                    new XElement(nameof(IPScannerHostInfo),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.Bssid), info.AvailableNetwork.Bssid),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.Ssid), info.AvailableNetwork.Ssid),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.ChannelCenterFrequencyInKilohertz), info.AvailableNetwork.ChannelCenterFrequencyInKilohertz),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.SignalBars), info.AvailableNetwork.SignalBars),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.IsWiFiDirect), info.AvailableNetwork.IsWiFiDirect),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.NetworkRssiInDecibelMilliwatts), info.AvailableNetwork.NetworkRssiInDecibelMilliwatts),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.PhyKind), info.AvailableNetwork.PhyKind),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.NetworkKind), info.AvailableNetwork.NetworkKind),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.SecuritySettings.NetworkAuthenticationType), info.AvailableNetwork.SecuritySettings.NetworkAuthenticationType),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.SecuritySettings.NetworkEncryptionType), info.AvailableNetwork.SecuritySettings.NetworkEncryptionType),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.BeaconInterval), info.AvailableNetwork.BeaconInterval),
                        new XElement(nameof(WiFiNetworkInfo.AvailableNetwork.Uptime), info.AvailableNetwork.Uptime)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<IPScannerHostInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.IPScanner.ToString(),
                new XElement(nameof(IPScannerHostInfo) + "s",

                from info in collection
                select
                    new XElement(nameof(IPScannerHostInfo),
                        new XElement("Status", info.IsReachable),
                        new XElement(nameof(PingInfo.IPAddress), info.PingInfo.IPAddress),
                        new XElement(nameof(IPScannerHostInfo.Hostname), info.Hostname),
                        new XElement("PortStatus", info.IsAnyPortOpen ? PortState.Open : PortState.Closed),
                        new XElement("PingStatus", info.PingInfo.Status),
                        new XElement(nameof(IPScannerHostInfo.MACAddress), info.MACAddress),
                        new XElement(nameof(IPScannerHostInfo.Vendor), info.Vendor),
                        from port in info.Ports
                        select new XElement(nameof(PortInfo),
                            new XElement(nameof(PortInfo.Port), port.Port),
                            new XElement(nameof(PortInfo.LookupInfo.Protocol), port.LookupInfo.Protocol),
                            new XElement(nameof(PortInfo.LookupInfo.Service), port.LookupInfo.Service),
                            new XElement(nameof(PortInfo.LookupInfo.Description), port.LookupInfo.Description),
                            new XElement(nameof(PortInfo.State), port.State)),
                        new XElement(nameof(PingInfo.Bytes), info.PingInfo.Bytes),
                        new XElement(nameof(PingInfo.Time), Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)),
                        new XElement(nameof(PingInfo.TTL), info.PingInfo.TTL)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<PortScannerPortInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.PortScanner.ToString(),
                new XElement(nameof(PortScannerPortInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(PortScannerPortInfo),
                            new XElement(nameof(PortScannerPortInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(PortScannerPortInfo.Hostname), info.Hostname),
                            new XElement(nameof(PortScannerPortInfo.Port), info.Port),
                            new XElement(nameof(PortLookupInfo.Protocol), info.LookupInfo.Protocol),
                            new XElement(nameof(PortLookupInfo.Service), info.LookupInfo.Service),
                            new XElement(nameof(PortLookupInfo.Description), info.LookupInfo.Description),
                            new XElement(nameof(PortScannerPortInfo.State), info.State)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<PingInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.PingMonitor.ToString(),
                new XElement(nameof(PingInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(PingInfo),
                            new XElement(nameof(PingInfo.Timestamp), info.Timestamp),
                            new XElement(nameof(PingInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(PingInfo.Hostname), info.Hostname),
                            new XElement(nameof(PingInfo.Bytes), info.Bytes),
                            new XElement(nameof(PingInfo.Time), Ping.TimeToString(info.Status, info.Time, true)),
                            new XElement(nameof(PingInfo.TTL), info.TTL),
                            new XElement(nameof(PingInfo.Status), info.Status)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<TracerouteHopInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.Traceroute.ToString(),
                new XElement(nameof(TracerouteHopInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(TracerouteHopInfo),
                            new XElement(nameof(TracerouteHopInfo.Hop), info.Hop),
                            new XElement(nameof(TracerouteHopInfo.Time1), Ping.TimeToString(info.Status1, info.Time1, true)),
                            new XElement(nameof(TracerouteHopInfo.Time2), Ping.TimeToString(info.Status2, info.Time2, true)),
                            new XElement(nameof(TracerouteHopInfo.Time3), Ping.TimeToString(info.Status3, info.Time3, true)),
                            new XElement(nameof(TracerouteHopInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(TracerouteHopInfo.Hostname), info.Hostname),
                            new XElement(nameof(TracerouteHopInfo.Status1), info.Status1),
                            new XElement(nameof(TracerouteHopInfo.Status2), info.Status2),
                            new XElement(nameof(TracerouteHopInfo.Status3), info.Status3)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<DNSLookupRecordInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.DNSLookup.ToString(),
                new XElement(nameof(DNSLookupRecordInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(DNSLookupRecordInfo),
                            new XElement(nameof(DNSLookupRecordInfo.DomainName), info.DomainName),
                            new XElement(nameof(DNSLookupRecordInfo.TTL), info.TTL),
                            new XElement(nameof(DNSLookupRecordInfo.Class), info.Class),
                            new XElement(nameof(DNSLookupRecordInfo.Type), info.Type),
                            new XElement(nameof(DNSLookupRecordInfo.Result), info.Result),
                            new XElement(nameof(DNSLookupRecordInfo.Server), info.Server),
                            new XElement(nameof(DNSLookupRecordInfo.IPEndPoint), info.IPEndPoint)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<SNMPReceivedInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.SNMP.ToString(),
                new XElement(nameof(SNMPReceivedInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(SNMPReceivedInfo),
                            new XElement(nameof(SNMPReceivedInfo.OID), info.OID),
                            new XElement(nameof(SNMPReceivedInfo.Data), info.Data)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<SNTPLookupResultInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.SNMP.ToString(),
                new XElement(nameof(SNTPLookupResultInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(SNTPLookupResultInfo),
                            new XElement(nameof(SNTPLookupResultInfo.Server), info.Server),
                            new XElement(nameof(SNTPLookupResultInfo.IPEndPoint), info.IPEndPoint),
                            new XElement(nameof(SNTPLookupResultInfo.DateTime.NetworkTime), info.DateTime.NetworkTime.ToString("yyyy.MM.dd HH:mm:ss.fff")),
                            new XElement(nameof(SNTPLookupResultInfo.DateTime.LocalStartTime), info.DateTime.LocalStartTime.ToString("yyyy.MM.dd HH:mm:ss.fff")),
                            new XElement(nameof(SNTPLookupResultInfo.DateTime.LocalEndTime), info.DateTime.LocalEndTime.ToString("yyyy.MM.dd HH:mm:ss.fff")),
                            new XElement(nameof(SNTPLookupResultInfo.DateTime.Offset), $"{info.DateTime.Offset} s"),
                            new XElement(nameof(SNTPLookupResultInfo.DateTime.RoundTripDelay), $"{info.DateTime.RoundTripDelay} ms")))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<IPNetworkInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.SNMP.ToString(),
                new XElement(nameof(IPNetworkInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(IPNetworkInfo),
                            new XElement(nameof(IPNetworkInfo.Network), info.Network),
                            new XElement(nameof(IPNetworkInfo.Broadcast), info.Broadcast),
                            new XElement(nameof(IPNetworkInfo.Total), info.Total),
                            new XElement(nameof(IPNetworkInfo.Netmask), info.Netmask),
                            new XElement(nameof(IPNetworkInfo.Cidr), info.Cidr),
                            new XElement(nameof(IPNetworkInfo.FirstUsable), info.FirstUsable),
                            new XElement(nameof(IPNetworkInfo.LastUsable), info.LastUsable),
                            new XElement(nameof(IPNetworkInfo.Usable), info.Usable)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<OUIInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.Lookup.ToString(),
                new XElement(nameof(OUIInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(OUIInfo),
                            new XElement(nameof(OUIInfo.MACAddress), info.MACAddress),
                            new XElement(nameof(OUIInfo.Vendor), info.Vendor)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<PortLookupInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.Lookup.ToString(),
                new XElement(nameof(PortLookupInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(PortLookupInfo),
                            new XElement(nameof(PortLookupInfo.Number), info.Number),
                            new XElement(nameof(PortLookupInfo.Protocol), info.Protocol),
                            new XElement(nameof(PortLookupInfo.Service), info.Service),
                            new XElement(nameof(PortLookupInfo.Description), info.Description)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<ConnectionInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.Connections.ToString(),
                new XElement(nameof(ConnectionInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(ConnectionInfo),
                            new XElement(nameof(ConnectionInfo.Protocol), info.Protocol),
                            new XElement(nameof(ConnectionInfo.LocalIPAddress), info.LocalIPAddress),
                            new XElement(nameof(ConnectionInfo.LocalPort), info.LocalPort),
                            new XElement(nameof(ConnectionInfo.RemoteIPAddress), info.RemoteIPAddress),
                            new XElement(nameof(ConnectionInfo.RemotePort), info.RemotePort),
                            new XElement(nameof(ConnectionInfo.TcpState), info.TcpState)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<ListenerInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.Listeners.ToString(),
                new XElement(nameof(ListenerInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(ListenerInfo),
                            new XElement(nameof(ListenerInfo.Protocol), info.Protocol),
                            new XElement(nameof(ListenerInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(ListenerInfo.Port), info.Port)))));

        document.Save(filePath);
    }

    public static void CreateXML(IEnumerable<ARPInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,

            new XElement(ApplicationName.ARPTable.ToString(),
                new XElement(nameof(ARPInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(ARPInfo),
                            new XElement(nameof(ARPInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(ARPInfo.MACAddress), info.MACAddress),
                            new XElement(nameof(ARPInfo.IsMulticast), info.IsMulticast)))));

        document.Save(filePath);
    }
    #endregion

    #region CreateJSON
    // This might be a horror to maintain, but i have no other idea...
    public static void CreateJSON(ObservableCollection<WiFiNetworkInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].AvailableNetwork.Bssid,
                collection[i].AvailableNetwork.Ssid,
                ChannelCenterFrequencyInKilohertz = collection[i].AvailableNetwork.Ssid.ToString(),
                SignalBars = collection[i].AvailableNetwork.SignalBars.ToString(),
                IsWiFiDirect = collection[i].AvailableNetwork.IsWiFiDirect.ToString(),
                NetworkRssiInDecibelMilliwatts = collection[i].AvailableNetwork.NetworkRssiInDecibelMilliwatts.ToString(),
                PhyKind = collection[i].AvailableNetwork.PhyKind.ToString(),
                NetworkKind = collection[i].AvailableNetwork.NetworkKind.ToString(),
                NetworkAuthenticationType = collection[i].AvailableNetwork.SecuritySettings.NetworkAuthenticationType.ToString(),
                NetworkEncryptionType = collection[i].AvailableNetwork.SecuritySettings.NetworkEncryptionType.ToString(),
                BeaconInterval = collection[i].AvailableNetwork.BeaconInterval.ToString(),
                Uptime = collection[i].AvailableNetwork.Uptime.ToString()
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<IPScannerHostInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            var jsonDataPorts = new object[collection[i].Ports.Count];

            for (var j = 0; j < collection[i].Ports.Count; j++)
            {
                jsonDataPorts[j] = new
                {
                    collection[i].Ports[j].Port,
                    Protocol = collection[i].Ports[j].LookupInfo.Protocol.ToString(),
                    collection[i].Ports[j].LookupInfo.Service,
                    collection[i].Ports[j].LookupInfo.Description,
                    State = collection[i].Ports[j].State.ToString()
                };
            }

            jsonData[i] = new
            {
                Status = collection[i].IsReachable.ToString(),
                IPAddress = collection[i].PingInfo.IPAddress.ToString(),
                collection[i].Hostname,
                PortStatus = collection[i].IsAnyPortOpen ? PortState.Open.ToString() : PortState.Closed.ToString(),
                PingStatus = collection[i].PingInfo.Status.ToString(),
                MACAddress = collection[i].MACAddress?.ToString(),
                collection[i].Vendor,
                Ports = jsonDataPorts,
                collection[i].PingInfo.Bytes,
                Time = Ping.TimeToString(collection[i].PingInfo.Status, collection[i].PingInfo.Time, true),
                collection[i].PingInfo.TTL
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<PortScannerPortInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                IPAddress = collection[i].IPAddress.ToString(),
                collection[i].Hostname,
                collection[i].Port,
                Protocol = collection[i].LookupInfo.Protocol.ToString(),
                collection[i].LookupInfo.Service,
                collection[i].LookupInfo.Description,
                Status = collection[i].State.ToString()
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<PingInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].Timestamp,
                IPAddress = collection[i].IPAddress.ToString(),
                collection[i].Hostname,
                collection[i].Bytes,
                Time = Ping.TimeToString(collection[i].Status, collection[i].Time, true),
                collection[i].TTL,
                Status = collection[i].Status.ToString()
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<TracerouteHopInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].Hop,
                Time1 = Ping.TimeToString(collection[i].Status1, collection[i].Time1, true),
                Time2 = Ping.TimeToString(collection[i].Status2, collection[i].Time2, true),
                Time3 = Ping.TimeToString(collection[i].Status3, collection[i].Time3, true),
                IPAddress = collection[i].IPAddress.ToString(),
                collection[i].Hostname,
                Status1 = collection[i].Status1.ToString(),
                Status2 = collection[i].Status2.ToString(),
                Status3 = collection[i].Status3.ToString()
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<DNSLookupRecordInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].DomainName,
                collection[i].TTL,
                collection[i].Class,
                collection[i].Type,
                collection[i].Result,
                collection[i].Server,
                collection[i].IPEndPoint
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<SNMPReceivedInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].OID,
                collection[i].Data
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<SNTPLookupResultInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].Server,
                collection[i].IPEndPoint,
                NetworkTime = collection[i].DateTime.NetworkTime.ToString("yyyy.MM.dd HH:mm:ss.fff"),
                LocalStartTime = collection[i].DateTime.LocalStartTime.ToString("yyyy.MM.dd HH:mm:ss.fff"),
                LocalEndTime = collection[i].DateTime.LocalEndTime.ToString("yyyy.MM.dd HH:mm:ss.fff"),
                Offset = $"{collection[i].DateTime.Offset} s",
                RoundTripDelay = $"{collection[i].DateTime.RoundTripDelay} ms",
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<IPNetworkInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                Network = collection[i].Network.ToString(),
                Broadcast = collection[i].Broadcast.ToString(),
                collection[i].Total,
                Netmask = collection[i].Netmask.ToString(),
                collection[i].Cidr,
                FirstUsable = collection[i].FirstUsable.ToString(),
                LastUsable = collection[i].LastUsable.ToString(),
                collection[i].Usable
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<OUIInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].MACAddress,
                collection[i].Vendor
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<PortLookupInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].Number,
                Protocol = collection[i].Protocol.ToString(),
                collection[i].Service,
                collection[i].Description
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<ConnectionInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                Protocol = collection[i].Protocol.ToString(),
                LocalIPAddress = collection[i].LocalIPAddress.ToString(),
                collection[i].LocalPort,
                RemoteIPAddress = collection[i].RemoteIPAddress.ToString(),
                collection[i].RemotePort,
                TcpState = collection[i].TcpState.ToString()
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<ListenerInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                Protocol = collection[i].Protocol.ToString(),
                IPAddress = collection[i].IPAddress.ToString(),
                collection[i].Port
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    public static void CreateJSON(ObservableCollection<ARPInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                IPAddress = collection[i].IPAddress.ToString(),
                MACAddress = collection[i].MACAddress.ToString(),
                collection[i].IsMulticast
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
    #endregion

    #region CreateTXT

    public static void CreateTXT(string content, string filePath)
    {
        System.IO.File.WriteAllText(filePath, content);
    }
    #endregion

    public static string GetFileExtensionAsString(ExportFileType fileExtension)
    {
        return fileExtension switch
        {
            ExportFileType.CSV => "CSV",
            ExportFileType.XML => "XML",
            ExportFileType.JSON => "JSON",
            ExportFileType.TXT => "TXT",
            _ => string.Empty,
        };
    }
    #endregion
}
