using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.Settings
{
    public class PuTTYSessionInfo
    {
        public string Name { get; set; }
        public ConnectionMode ConnectionMode { get; set; }
        public string Host { get; set; }
        public string SerialLine { get; set; }
        public int Port { get; set; }
        public int Baud { get; set; }
        public string Username { get; set; }
        public string Profile { get; set; }
        public string AdditionalCommandLine { get; set; }
        public string Group { get; set; }
        public string Tags { get; set; }

        public PuTTYSessionInfo()
        {

        }

        public PuTTYSessionInfo(string name, ConnectionMode connectionMode, string hostOrSerialLine, int portOrBaud, string username, string profile, string additionalCommandLine, string group, string tags)
        {
            Name = name;
            ConnectionMode = connectionMode;

            if(connectionMode==ConnectionMode.Serial)
            {
                SerialLine = hostOrSerialLine;
                Baud = portOrBaud;
            }
            else
            {
                Host = hostOrSerialLine;
                Port = portOrBaud;
            }
            
            Username = username;
            Profile = profile;
            AdditionalCommandLine = additionalCommandLine;
            Group = group;
            Tags = tags;
        }
    }
}
