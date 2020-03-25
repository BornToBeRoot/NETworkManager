using System;

namespace NETworkManager.Update
{
    /// <summary>
    /// Contains informations about a program update.
    /// </summary>
    public class UpdateAvailableArgs : EventArgs
    {
        /// <summary>
        /// Version of the program update.
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAvailableArgs"/> class and passes the <see cref="Version"/> as paramter.
        /// </summary>
        /// <param name="version">Version of the program update.</param>
        public UpdateAvailableArgs(Version version)
        {
            Version = version;
        }
    }
}
