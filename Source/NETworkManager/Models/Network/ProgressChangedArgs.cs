namespace NETworkManager.Models.Network
{
    public class ProgressChangedArgs : System.EventArgs
    {
        public int Value { get; set; }

        public ProgressChangedArgs()
        {

        }

        public ProgressChangedArgs(int value)
        {            
            Value = value;
        }       
    }
}
