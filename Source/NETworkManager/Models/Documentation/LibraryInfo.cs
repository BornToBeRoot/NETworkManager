namespace NETworkManager.Models.Documentation
{
    public class LibraryInfo : BaseInfo
    {
        public string License { get; set; }
        public string LicenseUrl { get; set; }

        public LibraryInfo(string name, string websiteUrl, string description, string license, string licenseUrl) : base(name, websiteUrl, description)
        {
            License = license;
            LicenseUrl = licenseUrl;
        }
    }
}
