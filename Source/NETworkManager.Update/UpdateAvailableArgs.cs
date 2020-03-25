using System;

namespace NETworkManager.Update
{
    /// <summary>
    /// Event arguments when an update is available.
    /// </summary>
    public class UpdateAvailableArgs : EventArgs
    {
        /// <summary>
        /// Version of the newest release.
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAvailableArgs"/> class
        /// </summary>
        /// <param name="version"><see cref="Version"/> of the latest release.</param>
        public UpdateAvailableArgs(Version version)
        {
            Version = version;
        }
    }
}
