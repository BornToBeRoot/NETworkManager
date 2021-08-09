namespace NETworkManager.Models.Network
{
    public class DNSServerDoHInfo
    {
        
        public string URL { get; set; }


        public DNSServerDoHInfo()
        {
           
        }

        public DNSServerDoHInfo(string url)
        {
            URL = url;
        }
    }
}
