using System;

namespace NETworkManager.Utilities
{
    public class CustomCommandInfo : ICloneable
    {
        public Guid ID { get; set; }
            
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Arguments { get; set; }

        public CustomCommandInfo()
        {
            ID = Guid.NewGuid();
        }

        public CustomCommandInfo(Guid id,string name, string filePath)
        {
            ID = id;
            Name = name;
            FilePath = filePath;
        }

        public CustomCommandInfo(Guid id, string name, string filePath, string arguments) : this(id, name, filePath)
        {
            Arguments = arguments;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }             
    }
}