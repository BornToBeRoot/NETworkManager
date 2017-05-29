namespace NETworkManager.Models.Network
{
    public class IPScannerProgressChangedArgs : System.EventArgs
    {
        public int Value { get; set; }

        public IPScannerProgressChangedArgs()
        {

        }

        public IPScannerProgressChangedArgs(int value)
        {            
            Value = value;
        }       
    }
}
