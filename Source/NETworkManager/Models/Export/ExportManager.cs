using System;
using System.Collections.ObjectModel;
using System.Text;
using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using NETworkManager.Resources.Localization;

namespace NETworkManager.Models.Export
{
    public static class ExportManager
    {
        #region Methods
        public static void Export(string filePath, ExportFileType fileType, ObservableCollection<IPScannerHostInfo> collection)
        {
            var text = string.Empty;
            switch (fileType)
            {
                case ExportFileType.CSV:
                   text = CreateCSVData(collection);

                    break;
                case ExportFileType.XML:


                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }

            System.IO.File.WriteAllText(filePath, text);
        }

        // IPScannerHostInfo
        public static string CreateCSVData(ObservableCollection<IPScannerHostInfo> collection)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{Strings.IPAddress},{Strings.Hostname},{Strings.MACAddress},{Strings.Vendor},{Strings.Bytes},{Strings.Time},{Strings.TTL},{Strings.Status}");

            foreach (var info in collection)
                stringBuilder.AppendLine($"{info.PingInfo.IPAddress},{info.Hostname},{info.MACAddress},{info.Vendor},{info.PingInfo.Bytes},{info.PingInfo.Time},{info.PingInfo.TTL},{LocalizationManager.TranslateIPStatus(info.PingInfo.Status)}");

            return stringBuilder.ToString();
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
