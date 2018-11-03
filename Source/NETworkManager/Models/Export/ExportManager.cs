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
        public static void Export(ExportInfo exportInfo)
        {
            System.IO.File.WriteAllText(exportInfo.FilePath, exportInfo.Data);
        }

        // IPScannerHostInfo
        public static string CreateData(ObservableCollection<IPScannerHostInfo> data, ExportFileType fileType)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{Strings.IPAddress},{Strings.Hostname},{Strings.MACAddress},{Strings.Vendor},{Strings.Bytes},{Strings.Time},{Strings.TTL},{Strings.Status}");

            foreach (var info in data)
            {
                stringBuilder.AppendLine($"{info.PingInfo.IPAddress},{info.Hostname},{info.MACAddress},{info.Vendor},{info.PingInfo.Bytes},{info.PingInfo.Time},{LocalizationManager.TranslateIPStatus(info.PingInfo.Status)}");
            }

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
