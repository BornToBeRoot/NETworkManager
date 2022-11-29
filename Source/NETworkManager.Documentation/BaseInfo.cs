namespace NETworkManager.Documentation
{
    /// <summary>
    /// Base class to hold informations about a library, service or resource.
    /// </summary>
    public abstract class BaseInfo
    {
        /// <summary>
        /// Name of the library, service or resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Url of the library, service or resource.
        /// </summary>
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Description of the library, service or resource.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Create an instance of <see cref="BaseInfo"/> with parameters.
        /// </summary>
        /// <param name="name">Name of the library, service or resource.</param>
        /// <param name="websiteUrl">Url of the library, service or resource.</param>
        /// <param name="description">Description of the library, service or resource.</param>
        protected BaseInfo(string name, string websiteUrl, string description)
        {
            Name = name;
            WebsiteUrl = websiteUrl;
            Description = description;
        }
    }
}
