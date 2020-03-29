using System;
using System.IO;


namespace NETworkManager.Utilities
{
    /// <summary>
    /// Contains methods to create a directory with sub directories.
    /// </summary>
    public static class DirectoryCreator
    {
        /// <summary>
        /// Create a directory with subdirectories and resolve environment variables.
        /// </summary>
        /// <param name="path">Path like "%AppDataLocal%\Folder1".</param>
        public static void CreateWithEnvironmentVariables(string path)
        {
            Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(path));
        }
    }
}
