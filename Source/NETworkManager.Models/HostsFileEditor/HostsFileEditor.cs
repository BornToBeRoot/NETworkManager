using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using NETworkManager.Utilities;

namespace NETworkManager.Models.HostsFileEditor;

public static class HostsFileEditor
{
#region Events
    public static event EventHandler HostsFileChanged;
    
    private static void OnHostsFileChanged()
    {
        Log.Debug("OnHostsFileChanged - Hosts file changed.");
        HostsFileChanged?.Invoke(null, EventArgs.Empty);
    }
    #endregion
    
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(HostsFileEditor));

    private static readonly FileSystemWatcher HostsFileWatcher;
    
    /// <summary>
    /// Path to the hosts file.
    /// </summary>
    private static string HostsFilePath => Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");

    /// <summary>
    /// Example values in the hosts file that should be ignored.
    /// </summary>
    private static readonly HashSet<(string IPAddress, string Hostname)> ExampleValuesToIgnore =
    [
        ("102.54.94.97", "rhino.acme.com"),
        ("38.25.63.10", "x.acme.com")
    ];
    
    /// <summary>
    /// Regex to match a hosts file entry with optional comments, supporting IPv4, IPv6, and hostnames
    /// </summary>
    private static readonly Regex HostsFileEntryRegex = new(RegexHelper.HostsEntryRegex);

    #endregion
    
    #region Constructor

    static HostsFileEditor()
    {
        // Create a file system watcher to monitor changes to the hosts file
        try
        {
            Log.Debug("HostsFileEditor - Creating file system watcher for hosts file...");
            
            // Create the file system watcher
            HostsFileWatcher = new FileSystemWatcher();
            HostsFileWatcher.Path = Path.GetDirectoryName(HostsFilePath) ?? throw new InvalidOperationException("Hosts file path is invalid.");
            HostsFileWatcher.Filter = Path.GetFileName(HostsFilePath) ?? throw new InvalidOperationException("Hosts file name is invalid.");
            HostsFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            
            // Maybe fired twice. This is a known bug/feature.
            // See: https://stackoverflow.com/questions/1764809/filesystemwatcher-changed-event-is-raised-twice
            HostsFileWatcher.Changed += (_, _) => OnHostsFileChanged();
            
            // Enable the file system watcher
            HostsFileWatcher.EnableRaisingEvents = true;
            
            Log.Debug("HostsFileEditor - File system watcher for hosts file created.");
        }
        catch (Exception ex)
        {
            Log.Error("Failed to create file system watcher for hosts file.", ex);
        }
    }
    #endregion

    #region Methods
    public static Task<IEnumerable<HostsFileEntry>> GetHostsFileEntriesAsync()
    {
        return Task.Run(GetHostsFileEntries);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<HostsFileEntry> GetHostsFileEntries()
    {
        var hostsFileLines = File.ReadAllLines(HostsFilePath);

        // Parse the hosts file content
        var entries = new List<HostsFileEntry>();

        foreach (var line in hostsFileLines)
        {
            var result = HostsFileEntryRegex.Match(line.Trim());

            if (result.Success)
            {
                Log.Debug("GetHostsFileEntries - Line matched: " + line);
                
                var entry = new HostsFileEntry
                {
                    IsEnabled = !result.Groups[1].Value.Equals("#"),
                    IPAddress = result.Groups[2].Value,
                    Hostname = result.Groups[3].Value.Replace(@"\s", "").Trim(),
                    Comment = result.Groups[4].Value.TrimStart('#',' '),
                    Line = line
                };
                
                // Skip example entries
                if(!entry.IsEnabled)
                {
                    if (ExampleValuesToIgnore.Contains((entry.IPAddress, entry.Hostname)))
                    {
                        Log.Debug("GetHostsFileEntries - Matched example entry. Skipping...");
                        continue;
                    }
                }
                
                entries.Add(entry);
            }
            else
            {
                Log.Debug("GetHostsFileEntries - Line not matched: " + line);
            }
        }

        return entries;
    }
    #endregion
}