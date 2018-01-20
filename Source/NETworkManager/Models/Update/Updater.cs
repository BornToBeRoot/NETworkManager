using System;
using System.Threading.Tasks;
using Octokit;
using NETworkManager.Models.Settings;

namespace NETworkManager.Models.Update
{
    public class Updater
    {
        #region Events
        public event EventHandler<UpdateAvailableArgs> UpdateAvailable;

        protected virtual void OnUpdateAvailable(UpdateAvailableArgs e)
        {
            UpdateAvailable?.Invoke(this, e);
        }

        public event EventHandler NoUpdateAvailable;

        protected virtual void OnNoUpdateAvailable()
        {
            NoUpdateAvailable?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        public void CheckViaGithub()
        {
            Task.Run(() =>
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("NETworkManager"));

                Task<Release> latestRelease = client.Repository.Release.GetLatest("BornToBeRoot", "NETworkManager");

                Version latestVersion = new Version(latestRelease.Result.TagName.TrimStart('v'));

                // Compare versions (tag=v1.4.2.0, version=1.4.2.0)
                if (latestVersion > AssemblyManager.Current.Version)
                    OnUpdateAvailable(new UpdateAvailableArgs(latestVersion));
                else
                    OnNoUpdateAvailable();
            });
        }
        #endregion
    }
}
