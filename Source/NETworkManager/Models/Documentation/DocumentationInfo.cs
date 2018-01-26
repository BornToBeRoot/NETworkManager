using NETworkManager.Models.Settings;
using System.Collections.Generic;

namespace NETworkManager.Models.Documentation
{
    public class DocumentationInfo
    {
        public int Identifier { get; set; }
        public string Path { get; set; }
        public List<LocalizationInfo> Localizations { get; set; }

        public DocumentationInfo()
        {

        }

        public DocumentationInfo(int identifier, string path, List<LocalizationInfo> localizations)
        {
            Identifier = identifier;
            Path = path;
            Localizations = localizations;
        }
    }
}
