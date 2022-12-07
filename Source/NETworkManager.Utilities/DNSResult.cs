namespace NETworkManager.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DNSResult
    {
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        
        public DNSResult()
        {
            
        }

        public DNSResult(bool hasError, string errorMessage)
        {
            HasError = hasError;
            ErrorMessage = errorMessage;
        }
    }
}
