namespace NETworkManager.Models.Documentation
{
    public abstract class BaseInfo
    {
        public string Name { get; set; }
        public string WebsiteUrl { get; set; }
        public string Description { get; set; }

        protected BaseInfo(string name, string websiteUrl, string description)
        {
            Name = name;
            WebsiteUrl = websiteUrl;
            Description = description;
        }
    }
}
