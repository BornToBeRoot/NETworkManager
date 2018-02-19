using NETworkManager.Models.Settings;
using System.Collections.Generic;

namespace NETworkManager.Models.Documentation
{
    public static class LibraryManager
    {
        public static List<LibraryInfo> List
        {
            get
            {
                return new List<LibraryInfo>
                {
                    new LibraryInfo("MahApps.Metro", "https://github.com/mahapps/mahapps.metro", "A toolkit for creating Metro / Modern UI styled WPF apps.", "MIT License", "https://github.com/MahApps/MahApps.Metro/blob/master/LICENSE")
                };
            }
        }
    }
}
