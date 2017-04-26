using System;

namespace NETworkManager.Model.Common
{
    public class ProgressChangedArgs : EventArgs
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
