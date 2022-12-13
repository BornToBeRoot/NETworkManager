namespace NETworkManager.Documentation
{
    /// <summary>
    /// Class to hold all informations about a library.
    /// </summary>
    public class LibraryInfo : BaseInfo
    {
        /// <summary>
        /// License which is used by the library.
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// Url of the license which is used by the library.
        /// </summary>
        public string LicenseUrl { get; set; }

        /// <summary>
        /// Create an instance of <see cref="LibraryInfo"/> with parameters.
        /// </summary>
        /// <param name="name">Name of the library.</param>
        /// <param name="websiteUrl">Url of the library.</param>
        /// <param name="description">Description of the library.</param>
        /// <param name="license">License which is used by the library.</param>
        /// <param name="licenseUrl">Url of the license which is used by the library.</param>
        public LibraryInfo(string name, string websiteUrl, string description, string license, string licenseUrl) : base(name, websiteUrl, description)
        {
            License = license;
            LicenseUrl = licenseUrl;
        }
    }
}
