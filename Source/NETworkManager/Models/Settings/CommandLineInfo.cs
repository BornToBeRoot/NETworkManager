namespace NETworkManager.Models.Settings
{
    public class CommandLineInfo
    {
        public bool Help { get; set; }
        public bool Autostart { get; set; }
        public bool ResetSettings { get; set; }
        public int RestartPid { get; set; } = -1;

        public Models.Application.Name Application { get; set; } = Models.Application.Name.None;
        public string WrongParameter { get; set; }

        public CommandLineInfo()
        {

        }
    }
}
