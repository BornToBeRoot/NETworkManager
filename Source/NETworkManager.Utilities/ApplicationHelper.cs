using System;
using System.IO;
using System.Linq;

namespace NETworkManager.Utilities;

/// <summary>
/// Helper class to interact with the applications on the system.
/// </summary>
public static class ApplicationHelper
{
    /// <summary>
    /// Find an application in the system PATH.
    /// This is similar to the `where` command in Windows.
    /// </summary>
    /// <param name="fileName">The name of the application to find (like `notepad` or `notepad.exe`).</param>
    /// <returns>The full path to the application if found, otherwise `null`.</returns>
    public static string Find(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");

        if (path == null) 
            return null;
        
        var directories = path.Split(';');
        
        if (!fileName.EndsWith(".exe"))
            fileName += ".exe";

        return directories
            .Select(dir => Path.Combine(dir, fileName))
            .FirstOrDefault(File.Exists);
    }
}