using Octokit;
using System;

namespace NETworkManager.Update
{
    /// <summary>
    /// Contains informations about a program update.
    /// </summary>
    public class UpdateAvailableArgs : EventArgs
    {
        /// <summary>
        /// Release of the program update.
        /// </summary>
        public Release Release { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAvailableArgs"/> class and passes the <see cref="Release"/> as paramter.
        /// </summary>
        /// <param name="release">Release of the program update.</param>
        public UpdateAvailableArgs(Release release)
        {
            Release = release;
        }
    }
}
