namespace NETworkManager.Models.Documentation
{
    public class LibraryInfo
    {
        public string Library { get; set; }
        public string LibraryUrl { get; set; }
        public string Description { get; set; }
        public string License { get; set; }
        public string LicenseUrl { get; set; }

        public LibraryInfo()
        {

        }

        public LibraryInfo(string library, string libraryUrl, string description, string license, string licenseUrl)
        {
            Library = library;
            LibraryUrl = libraryUrl;
            Description = description;
            License = license;
            LicenseUrl = licenseUrl;
        }
    }
}
