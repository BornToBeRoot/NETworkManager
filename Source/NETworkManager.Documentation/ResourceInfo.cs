namespace NETworkManager.Documentation
{
    /// <summary>
    /// Class to hold all informations about a resource.
    /// </summary>
    public class ResourceInfo : BaseInfo
    {
        /// <summary>
        /// Create an instance of <see cref="ResourceInfo"/> with parameters.
        /// </summary>
        /// <param name="name">Name of the resource.</param>
        /// <param name="websiteUrl">Url of the resource.</param>
        /// <param name="description">Description of the resource.</param>
        public ResourceInfo(string name, string websiteUrl, string description) : base(name, websiteUrl, description)
        {
            
        }
    }
}
