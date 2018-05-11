namespace NETworkManager.Models.Documentation
{
    public class ResourceInfo
    {
        public string Resource { get; set; }
        public string ResourceUrl { get; set; }
        public string Description { get; set; }
        
        public ResourceInfo()
        {

        }

        public ResourceInfo(string resource, string resourceUrl, string description)
        {
            Resource = resource;
            ResourceUrl = resourceUrl;
            Description = description;
        }
    }
}
