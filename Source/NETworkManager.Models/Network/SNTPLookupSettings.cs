namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class contains the settings for the SNTP lookup.
    /// </summary>
    public class SNTPLookupSettings
    {
        /// <summary>
        /// Timeout in milliseconds after which the request is aborted.
        /// </summary>
        public int Timeout { get; set; }
    }
}
