namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Event arguments when a discovery protocol package is returned.
    /// </summary>
    public class DiscoveryProtocolPackageArgs : System.EventArgs
    {
        /// <summary>
        /// Contains the <see cref="DiscoveryProtocolPackageInfo"/> which is returned.
        /// </summary>
        public DiscoveryProtocolPackageInfo PackageInfo { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryProtocolPackageArgs"/> class.
        /// </summary>
        public DiscoveryProtocolPackageArgs()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryProtocolWarningArgs"/> class with a discovery protocol package (<paramref name="packageInfo"/>).
        /// </summary>
        /// <param name="packageInfo">Discovery protocol package information</param>
        public DiscoveryProtocolPackageArgs(DiscoveryProtocolPackageInfo packageInfo)
        {
            PackageInfo = packageInfo;
        }
    }
}
