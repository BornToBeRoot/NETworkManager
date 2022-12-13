namespace NETworkManager.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class DNSResultString : DNSResult
    {
        public string Value { get; set; }

        public DNSResultString(bool hasError, string errorMessage) : base(hasError, errorMessage)
        {

        }

        public DNSResultString(string value)
        {
            Value = value;
        }
    }
}
