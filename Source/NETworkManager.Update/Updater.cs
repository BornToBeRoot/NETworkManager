using System;
using System.Threading.Tasks;
using Octokit;

namespace NETworkManager.Update
{
    /// <summary>
    /// Updater to check if a new program version is available.
    /// </summary>
    public class Updater
    {
        #region Events
        /// <summary>
        /// Is triggered when update check is complete and an update is available.
        /// </summary>
        public event EventHandler<UpdateAvailableArgs> UpdateAvailable;

        /// <summary>
        /// Triggers the <see cref="UpdateAvailable"/> event.
        /// </summary>
        /// <param name="e">Passes <see cref="UpdateAvailableArgs"/> to the event.</param>
        protected virtual void OnUpdateAvailable(UpdateAvailableArgs e)
        {
            UpdateAvailable?.Invoke(this, e);
        }

        /// <summary>
        /// Is triggered when update check is complete and no update is available.
        /// </summary>
        public event EventHandler NoUpdateAvailable;

        /// <summary>
        /// Triggers the <see cref="NoUpdateAvailable"/> event.
        /// </summary>
        protected virtual void OnNoUpdateAvailable()
        {
            NoUpdateAvailable?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Is triggered when an error occurs during the update check.
        /// </summary>
        public event EventHandler Error;

        /// <summary>
        /// Triggers the <see cref="Error"/> event.
        /// </summary>
        protected virtual void OnError()
        {
            Error?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks on GitHub whether a new version of the program is available
        /// </summary>
        /// <param name="userName">GitHub username like "BornToBeRoot".</param>
        /// <param name="projectName">GitHub repository like "NETworkManager".</param>
        /// <param name="currentVersion">Version like 1.2.0.0.</param>
        public void CheckOnGitHub(string userName, string projectName, Version currentVersion)
        {
            Task.Run(() =>
            {
                try
                {
                    var client = new GitHubClient(new ProductHeaderValue(userName + "_" + projectName));

                    var latestVersion = new Version(client.Repository.Release.GetLatest(userName, projectName).Result.TagName);

                    // Compare versions (tag=2019.12.0, version=2019.12.0)
                    if (latestVersion > currentVersion)
                        OnUpdateAvailable(new UpdateAvailableArgs(latestVersion));
                    else
                        OnNoUpdateAvailable();
                }
                catch
                {
                    OnError();
                }
            });
        }
        #endregion
    }
}
