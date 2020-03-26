using NETworkManager.Models.Application;

namespace NETworkManager.Models.Settings
{
    public class CommandLineInfo
    {
        public bool Help { get; set; }
        public bool Autostart { get; set; }
        public bool ResetSettings { get; set; }
        public int RestartPid { get; set; } = -1;

        public Name Application { get; set; } = Name.None;
        public string WrongParameter { get; set; }

        public CommandLineInfo()
        {

        }
    }
}
