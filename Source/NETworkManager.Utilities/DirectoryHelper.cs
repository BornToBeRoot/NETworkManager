using System;
using System.IO;
using System.Security;
using log4net;

namespace NETworkManager.Utilities;

/// <summary>
///     Contains methods interact with directories.
/// </summary>
public static class DirectoryHelper
{
    /// <summary>
    ///     Logger for logging.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(DirectoryHelper));

    /// <summary>
    ///     Create a directory with subdirectories and resolve environment variables.
    /// </summary>
    /// <param name="path">Path like "%AppDataLocal%\Folder1".</param>
    public static void CreateWithEnvironmentVariables(string path)
    {
        Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(path));
    }

    /// <summary>
    ///     Validates a folder path for correctness and accessibility.
    ///     Expands environment variables, checks that the path is absolute,
    ///     validates characters, and ensures it does not point to a file.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <param name="pathSource">Description of the path source for logging (e.g., "Policy-provided", "Custom").</param>
    /// <param name="propertyName">Name of the property being validated for logging (e.g., "Settings_FolderLocation").</param>
    /// <param name="fallbackMessage">Message describing what happens on validation failure (e.g., "next priority", "default location").</param>
    /// <returns>The validated full path if valid; otherwise, null.</returns>
    public static string ValidateFolderPath(string path, string pathSource, string propertyName, string fallbackMessage)
    {
        // Expand environment variables first (e.g. %userprofile%\settings -> C:\Users\...\settings)
        path = Environment.ExpandEnvironmentVariables(path);

        // Validate that the path is rooted (absolute)
        if (!Path.IsPathRooted(path))
        {
            Log.Error($"{pathSource} {propertyName} is not an absolute path: {path}. Falling back to {fallbackMessage}.");
            return null;
        }

        // Validate that the path doesn't contain invalid characters
        try
        {
            var fullPath = Path.GetFullPath(path);

            // Check if the path is a directory (not a file)
            if (File.Exists(fullPath))
            {
                Log.Error($"{pathSource} {propertyName} is a file, not a directory: {path}. Falling back to {fallbackMessage}.");
                return null;
            }

            return Path.TrimEndingDirectorySeparator(fullPath);
        }
        catch (ArgumentException ex)
        {
            Log.Error($"{pathSource} {propertyName} contains invalid characters: {path}. Falling back to {fallbackMessage}.", ex);
            return null;
        }
        catch (NotSupportedException ex)
        {
            Log.Error($"{pathSource} {propertyName} format is not supported: {path}. Falling back to {fallbackMessage}.", ex);
            return null;
        }
        catch (SecurityException ex)
        {
            Log.Error($"Insufficient permissions to access {pathSource} {propertyName}: {path}. Falling back to {fallbackMessage}.", ex);
            return null;
        }
        catch (PathTooLongException ex)
        {
            Log.Error($"{pathSource} {propertyName} path is too long: {path}. Falling back to {fallbackMessage}.", ex);
            return null;
        }
        catch (IOException ex)
        {
            Log.Error($"{pathSource} {propertyName} caused an I/O error: {path}. Falling back to {fallbackMessage}.", ex);
            return null;
        }
    }
}