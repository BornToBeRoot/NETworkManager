namespace NETworkManager.Utilities
{
    public class CustomCommandInfo
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Arguments { get; set; }

        public CustomCommandInfo()
        {

        }

        public CustomCommandInfo(string name, string filePath)
        {
            Name = name;
            FilePath = filePath;
        }

        public CustomCommandInfo(string name, string filePath, string arguments) : this(name, filePath)
        {
            Arguments = arguments;
        }
    }
}