using System.Diagnostics;

namespace NETworkManager.Utilities;

/// <summary>
/// Provides helper methods for starting external processes and opening URLs.
/// </summary>
public static class ExternalProcessStarter
{
    /// <summary>
    ///     Open a url with the default browser.
    /// </summary>
    /// <param name="url">Url like: https://github.com/BornToBeRoot</param>
    public static void OpenUrl(string url)
    {
        // Escape the $ in the command prompt
        url = url.Replace("&", "^&");

        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
    }

    /// <summary>
    /// Starts an external process by filename.
    /// </summary>
    /// <param name="filename">The name or path of the executable to run (e.g. "WF.msc").</param>
    /// <param name="asAdmin">If <c>true</c>, the process is started with elevated privileges via UAC.</param>
    public static void RunProcess(string filename, bool asAdmin = false)
    {
        RunProcess(filename, null, asAdmin);
    }

    /// <summary>
    /// Starts an external process by filename with optional arguments.
    /// </summary>
    /// <param name="filename">The name or path of the executable to run.</param>
    /// <param name="arguments">Optional command-line arguments to pass to the process.</param>
    /// <param name="asAdmin">If <c>true</c>, the process is started with elevated privileges via UAC.</param>
    public static void RunProcess(string filename, string arguments, bool asAdmin = false)
    {
        ProcessStartInfo info = new()
        {
            FileName = filename,
            UseShellExecute = true
        };

        if (!string.IsNullOrEmpty(arguments))
            info.Arguments = arguments;

        if (asAdmin)
            info.Verb = "runas";

        Process.Start(info);
    }
}