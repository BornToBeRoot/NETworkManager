using System.Collections.ObjectModel;

namespace NETworkManager.Models.Export
{
    public class ExportInfo
    {
        public ApplicationViewManager.Name ApplicationName { get; set; }
        public string FilePath { get; set; }
        public string Data { get; set; }
    }
}
