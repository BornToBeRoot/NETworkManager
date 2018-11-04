using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export
{
    public static class ExportManager
    {
        #region Variables
        private static readonly XDeclaration DefaultXDeclaration = new XDeclaration("1.0", "utf-8", "yes");

        #endregion

        #region Methods
        public static void Export(string filePath, ExportFileType fileType, ObservableCollection<HostInfo> collection)
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

        public static void Export(string filePath, ExportFileType fileType, ObservableCollection<PortInfo> collection)
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

        private static void CreateCSV(IEnumerable<HostInfo> collection, string filePath)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(PingInfo.IPAddress)},{nameof(HostInfo.Hostname)},{nameof(HostInfo.MACAddress)},{nameof(HostInfo.Vendor)},{nameof(PingInfo.Bytes)},{nameof(PingInfo.Time)},{nameof(PingInfo.TTL)},{nameof(PingInfo.Status)}");

            foreach (var info in collection)
                stringBuilder.AppendLine($"{info.PingInfo.IPAddress},{info.Hostname},{info.MACAddress},{info.Vendor},{info.PingInfo.Bytes},{Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)},{info.PingInfo.TTL},{info.PingInfo.Status}");

            System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
        }

        private static void CreateCSV(IEnumerable<PortInfo> collection, string filePath)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(PortInfo.IPAddress)},{nameof(PortInfo.Hostname)},{nameof(PortInfo.Port)},{nameof(PortLookupInfo.Protocol)},{nameof(PortLookupInfo.Service)},{nameof(PortLookupInfo.Description)},{nameof(PortInfo.Status)}");

            foreach (var info in collection)
                stringBuilder.AppendLine($"{info.IPAddress},{info.Hostname},{info.Port},{info.LookupInfo.Protocol},{info.LookupInfo.Service},{info.LookupInfo.Description},{info.Status}");

            System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
        }

        public static void CreateXML(IEnumerable<HostInfo> collection, string filePath)
        {
            var document = new XDocument(DefaultXDeclaration,

                new XElement(ApplicationViewManager.Name.IPScanner.ToString(),
                    new XElement(nameof(HostInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(HostInfo),
                            new XElement(nameof(PingInfo.IPAddress), info.PingInfo.IPAddress),
                            new XElement(nameof(HostInfo.Hostname), info.Hostname),
                            new XElement(nameof(HostInfo.MACAddress), info.MACAddress),
                            new XElement(nameof(HostInfo.Vendor), info.Vendor),
                            new XElement(nameof(PingInfo.Bytes), info.PingInfo.Bytes),
                            new XElement(nameof(PingInfo.Time), Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)),
                            new XElement(nameof(PingInfo.TTL), info.PingInfo.TTL),
                            new XElement(nameof(PingInfo.Status), info.PingInfo.Status)))));

            document.Save(filePath);
        }

        public static void CreateXML(IEnumerable<PortInfo> collection, string filePath)
        {
            var document = new XDocument(DefaultXDeclaration,

                new XElement(ApplicationViewManager.Name.PortScanner.ToString(),
                    new XElement(nameof(PortInfo) + "s",

                        from info in collection
                        select
                            new XElement(nameof(PortInfo),
                                new XElement(nameof(info.IPAddress), info.IPAddress),
                                new XElement(nameof(PortInfo.Hostname), info.Hostname),
                                new XElement(nameof(PortInfo.Port), info.Port),
                                new XElement(nameof(PortLookupInfo.Protocol), info.LookupInfo.Protocol),
                                new XElement(nameof(PortLookupInfo.Service), info.LookupInfo.Service),
                                new XElement(nameof(PortLookupInfo.Description), info.LookupInfo.Description),
                                new XElement(nameof(PortInfo.Status), info.Status)))));

            document.Save(filePath);
        }

        public static void CreateJSON(IEnumerable<HostInfo> collection, string filePath)
        {
            System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(collection,Formatting.Indented));
        }

        public static void CreateJSON(IEnumerable<PortInfo> collection, string filePath)
        {
            System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(collection, Formatting.Indented));
        }

        public static string GetFileExtensionAsString(ExportFileType fileExtension)
        {
            switch (fileExtension)
            {
                case ExportFileType.CSV:
                    return "CSV";
                case ExportFileType.XML:
                    return "XML";
                case ExportFileType.JSON:
                    return "JSON";
                default:
                    return string.Empty;
            }
        }
        #endregion

        public enum ExportFileType
        {
            CSV,
            XML,
            JSON
        }
    }
}
