using System.Net;

namespace NETworkManager.Models.Network
{
    public class SNTPLookupResultArgs : System.EventArgs
    {        
        public string Test { get; set; }        

        public SNTPLookupResultArgs()
        {

        }

        public SNTPLookupResultArgs(string test)
        {
            Test = test;
        }
    }
}
