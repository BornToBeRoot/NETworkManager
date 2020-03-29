namespace NETworkManager.Models.PuTTY
{
    /// <summary>
    /// Stores informations about a putty session.
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
        /// PuTTY profile to use.
        /// </summary>
        public string Profile { get; set; }

        /// <summary>
        /// Enables session log.
        /// </summary>
        public bool EnableSessionLog { get; set; }

        /// <summary>
        /// Path and filename of the session log file (e.g. "C:\..\PuTTY.log").
        /// </summary>
        public string SessionLogFullName { get; set; }

        /// <summary>
        /// Additional command line argument. Everything putty can handle.
        /// </summary>
        public string AdditionalCommandLine { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PuTTYSessionInfo"/> class.
        /// </summary>
        public PuTTYSessionInfo()
        {

        } 
    }
}
