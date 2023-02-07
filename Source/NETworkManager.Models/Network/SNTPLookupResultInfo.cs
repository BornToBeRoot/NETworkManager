namespace NETworkManager.Models.Network
{
    public class SNTPLookupResultInfo
    {

        public string Test { get; set; }        

        public SNTPLookupResultInfo()
        {

        }

        public SNTPLookupResultInfo(string test)
        {
            Test = test;
        }

        public static SNTPLookupResultInfo Parse(SNTPLookupResultArgs e)
        {
            return new SNTPLookupResultInfo(e.Test);
        }
    }
}