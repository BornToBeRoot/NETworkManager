using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export
{
    public static class ExportManager
    {
        #region Methods
        public static void Export(string filePath, ExportFileType fileType, ObservableCollection<IPScannerHostInfo> collection)
        {
            switch (fileType)
            {
                case ExportFileType.CSV:
                    CreateCSV(collection, filePath);

                    break;
                case ExportFileType.XML:
                    CreateXMLData(collection, filePath);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }
        }

        // IPScannerHostInfo
        private static void CreateCSV(ObservableCollection<IPScannerHostInfo> collection, string filePath)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(PingInfo.IPAddress)},{nameof(IPScannerHostInfo.Hostname)},{nameof(IPScannerHostInfo.MACAddress)},{nameof(IPScannerHostInfo.Vendor)},{nameof(PingInfo.Bytes)},{nameof(PingInfo.Time)},{nameof(PingInfo.TTL)},{nameof(PingInfo.Status)}");

            foreach (var info in collection)
                stringBuilder.AppendLine($"{info.PingInfo.IPAddress},{info.Hostname},{info.MACAddress},{info.Vendor},{info.PingInfo.Bytes},{Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)},{info.PingInfo.TTL},{info.PingInfo.Status}");

            System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
        }

        public static void CreateXMLData(ObservableCollection<IPScannerHostInfo> collection, string filePath)
        {
            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),

                new XElement(ApplicationViewManager.Name.IPScanner.ToString(),
                    new XElement(nameof(IPScannerHostInfo) + "s",

                    from info in collection
                    select
                        new XElement(nameof(IPScannerHostInfo), new XAttribute(nameof(info.PingInfo.IPAddress), info.PingInfo.IPAddress),
                            new XElement(nameof(IPScannerHostInfo.Hostname), info.Hostname),
                            new XElement(nameof(IPScannerHostInfo.MACAddress), info.MACAddress),
                            new XElement(nameof(IPScannerHostInfo.Vendor), info.Vendor),
                            new XElement(nameof(PingInfo.Bytes), info.PingInfo.Bytes),
                            new XElement(nameof(PingInfo.Time), Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)),
                            new XElement(nameof(PingInfo.TTL), info.PingInfo.TTL),
                            new XElement(nameof(PingInfo.Status), info.PingInfo.Status)))));

            document.Save(filePath);
        }

        public static string CreateXAttributeString(string s)
        {
            return s.Replace(" ", "").Replace("-", "");
        }

        public static string GetFileExtensionAsString(ExportFileType fileExtension)
        {
            switch (fileExtension)
            {
                case ExportFileType.CSV:
                    return "CSV";
                case ExportFileType.XML:
                    return "XML";
                default:
                    return string.Empty;
            }
        }
        #endregion

        public enum ExportFileType
        {
            CSV,
            XML
        }
    }
}
