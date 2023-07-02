namespace NETworkManager.Models.Network
{
   
    public class IPDNSApiResult
    {
        public IPDNSApiInfo Info { get; set; }

        /// <summary>
        /// Indicates if the IP information retrieval has failed.
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Error message if the IP information retrieval has failed.
        /// </summary>
        public string ErrorMessage { get; set; }

 
        public IPDNSApiResult(IPDNSApiInfo info)
        {
            Info = info;         
        }

        public IPDNSApiResult(bool hasError, string errorMessage)
        {
            HasError = hasError;
            ErrorMessage = errorMessage;
        }
    }
}
