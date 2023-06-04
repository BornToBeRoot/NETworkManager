using System;
using System.Threading.Tasks;
using log4net;
using Octokit;

namespace NETworkManager.Update;

/// <summary>
/// Updater to check if a new program version is available.
/// </summary>
public class Updater
{
    private static readonly ILog _log = LogManager.GetLogger(typeof(Updater));

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
    /// <param name="e">Passes <see cref="UpdateErrorArgs"/> to the event.</param>
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
    public void CheckOnGitHub(string userName, string projectName, Version currentVersion, bool includePreRelease)
    {
        Task.Run(() =>
        {
            try
            {
                _log.Info("Checking for new program version on GitHub...");

                var client = new GitHubClient(new ProductHeaderValue(userName + "_" + projectName));

                Release release = null;

                if (includePreRelease)
                    release = client.Repository.Release.GetAll(userName, projectName).Result[0];
                else
                    release = client.Repository.Release.GetLatest(userName, projectName).Result;

                // Compare versions (tag=2021.2.15.0, version=2021.2.15.0)
                if (new Version(release.TagName) > currentVersion)
                {
                    _log.Info($"New version \"{release.TagName}\" is available on GitHub!");
                    OnUpdateAvailable(new UpdateAvailableArgs(release));
                }
                else
                {
                    _log.Info("No new program version available on GitHub!");
                    OnNoUpdateAvailable();
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error while checking for new program version on GitHub!", ex);
                OnError();
            }
        });
    }
    #endregion
}
