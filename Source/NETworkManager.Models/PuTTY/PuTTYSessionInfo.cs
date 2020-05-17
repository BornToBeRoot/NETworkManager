namespace NETworkManager.Models.PuTTY
{
    /// <summary>
    /// Stores informations about a PuTTY session.
    /// </summary>
    public class PuTTYSessionInfo
    {
        /// <summary>
        /// Full path to the PuTTY.exe on the filesystem.
        /// </summary>
        public string ApplicationFilePath { get; set; }

        /// <summary>
        /// Mode (SSH, Telnet, etc.), which is used to establish the connection.
        /// </summary>
        public ConnectionMode Mode { get; set; }

        /// <summary>
        /// Hostname or SerialLine. Depends on the <see cref="ConnectionMode"/>.
        /// </summary>
        public string HostOrSerialLine { get; set; }

        /// <summary>
        /// Port or Baud. Depends on the <see cref="ConnectionMode"/>.
        /// </summary>
        public int PortOrBaud { get; set; }

        /// <summary>
        /// Username for login.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Path to the private key.
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// PuTTY profile to use.
        /// </summary>
        public string Profile { get; set; }

        /// <summary>
        /// Enables session log.
        /// </summary>
        public bool EnableLog { get; set; }

        /// <summary>
        /// PuTTY log mode.
        /// </summary>
        public LogMode LogMode { get; set; }

        /// <summary>
        /// Path to the PuTTY log files like "C:\temp".
        /// </summary>
        public string LogPath { get; set; }

        /// <summary>
        /// Filename of the PuTTY log like "PuTTY.log".
        /// </summary>
        public string LogFileName { get; set; }

        /// <summary>
        /// Additional command line argument. Everything putty can handle.
        /// </summary>
        public string AdditionalCommandLine { get; set; }

        /// <summary>
        /// Create an instance of <see cref="PuTTYSessionInfo"/>.
        /// </summary>
        public PuTTYSessionInfo()
        {

        } 
    }
}
