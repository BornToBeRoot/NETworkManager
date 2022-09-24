namespace NETworkManager.Models.AWS
{
    public class AWSSessionManagerSessionInfo
    {
        public string ApplicationFilePath { get; set; }
        public string InstanceID { get; set; }
        public string Profile { get; set; }
        public string Region { get; set; }

        public AWSSessionManagerSessionInfo()
        {

        }
    }
}
