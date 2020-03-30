using NETworkManager.Localization;
using System.Collections.Generic;

namespace NETworkManager.Documentation
{
    public class DocumentationInfo
    {
        public DocumentationIdentifier Identifier { get; set; }
        public string Path { get; set; }

        public DocumentationInfo(DocumentationIdentifier identifier, string path)
        {
            Identifier = identifier;
            Path = path;
        }
    }
}
