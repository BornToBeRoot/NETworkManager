using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using NETworkManager.Utilities;

namespace NETworkManager.Models.HostsFileEditor;

public class HostsFileEditor
{
#region Events
    public event EventHandler HostsFileChanged;
    
    private void OnHostsFileChanged()
    {
        Log.Debug("OnHostsFileChanged - Hosts file changed.");
        HostsFileChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion
    
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(HostsFileEditor));
    
    /// <summary>
    /// Path to the hosts file.
    /// </summary>
    private static string HostsFilePath => Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");

    /// <summary>
    /// Regex to match a hosts file entry with optional comments, supporting IPv4, IPv6, and hostnames
    /// </summary>
    private readonly Regex _hostsFileEntryRegex = new Regex(RegexHelper.HostsEntryRegex);

    #endregion
    
    #region Constructor
    public HostsFileEditor()
    {
        // Create a file system watcher to monitor changes to the hosts file
        try
        {
            Log.Debug("HostsFileEditor - Creating file system watcher for hosts file...");
            
            FileSystemWatcher watcher = new();
            watcher.Path = Path.GetDirectoryName(HostsFilePath) ?? throw new InvalidOperationException("Hosts file path is invalid.");
            watcher.Filter = Path.GetFileName(HostsFilePath) ?? throw new InvalidOperationException("Hosts file name is invalid.");
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            
            // Maybe fired twice. This is a known bug/feature.
            // See: https://stackoverflow.com/questions/1764809/filesystemwatcher-changed-event-is-raised-twice
            watcher.Changed += (_, _) => OnHostsFileChanged();
            
            watcher.EnableRaisingEvents = true;
            
            Log.Debug("HostsFileEditor - File system watcher for hosts file created.");
        }
        catch (Exception ex)
        {
            Log.Error("Failed to create file system watcher for hosts file.", ex);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<HostsFileEntry> GetHostsFileEntries()
    {
        var hostsFileLines = File.ReadAllLines(HostsFilePath);

        // Parse the hosts file content
        var entries = new List<HostsFileEntry>();

        foreach (var line in hostsFileLines)
        {
            var result = _hostsFileEntryRegex.Match(line.Trim());

            if (result.Success)
            {
                Log.Debug("GetHostsFileEntries - Line matched: " + line);
                
                var entry = new HostsFileEntry
                {
                    IsEnabled = !result.Groups[1].Value.Equals("#"),
                    IpAddress = result.Groups[2].Value,
                    HostName = result.Groups[3].Value.Replace(@"\s", "").Split([' ']),
                    Comment = result.Groups[4].Value,
                    Line = line
                };
                
                // Skip example entries
                if(!entry.IsEnabled && entry.IpAddress is "102.54.94.97" or "38.25.63.10" && entry.HostName[0] is "rhino.acme.com" or "x.acme.com")
                {
                    Log.Debug("GetHostsFileEntries - Matched example entry. Skipping...");
                    continue;
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