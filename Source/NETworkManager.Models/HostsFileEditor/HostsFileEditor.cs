using log4net;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    /// Path to the hosts folder.
    /// </summary>
    private static string HostsFolderPath => Path.Combine(Environment.SystemDirectory, "drivers", "etc");

    /// <summary>
    /// Path to the hosts file.
    /// </summary>
    private static string HostsFilePath => Path.Combine(HostsFolderPath, "hosts");

    /// <summary>
    /// Identifier for the hosts file backup.
    /// </summary>
    private static string HostsFileBackupIdentifier => "hosts_backup_NETworkManager";

    /// <summary>
    /// Number of backups to keep.
    /// </summary>
    private static int HostsFileBackupsToKeep => 5;

    /// <summary>
    /// Last time a backup was created.
    /// </summary>
    private static DateTime _lastBackupTime = DateTime.MinValue;

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
            HostsFileWatcher.Path = Path.GetDirectoryName(HostsFilePath) ??
                                    throw new InvalidOperationException("Hosts file path is invalid.");
            HostsFileWatcher.Filter = Path.GetFileName(HostsFilePath) ??
                                      throw new InvalidOperationException("Hosts file name is invalid.");
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

    /// <summary>
    /// Gets the entries from the hosts file asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing all entries from the hosts file.</returns>
    public static Task<IEnumerable<HostsFileEntry>> GetHostsFileEntriesAsync()
    {
        return Task.Run(GetHostsFileEntries);
    }

    /// <summary>
    /// Gets the entries from the hosts file.
    /// </summary>
    /// <returns>All entries from the hosts file.</returns>
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
                    Comment = result.Groups[4].Value.TrimStart('#', ' '),
                    Line = line
                };

                // Skip example entries
                if (!entry.IsEnabled)
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

    public static Task<bool> EnableEntryAsync(HostsFileEntry entry)
    {
        return Task.Run(() => EnableEntry(entry));
    }

    private static bool EnableEntry(HostsFileEntry entry)
    {
        // Create a backup of the hosts file before making changes
        if (CreateBackup() == false)
        {
            Log.Error("EnableEntry - Failed to create backup before enabling entry.");
            return false;
        }

        // Replace the entry in the hosts file
        var hostsFileLines = File.ReadAllLines(HostsFilePath).ToList();

        for (var i = 0; i < hostsFileLines.Count; i++)
        {
            if (hostsFileLines[i] == entry.Line)
                hostsFileLines[i] = entry.Line.TrimStart('#', ' ');
        }

        try
        {
            Log.Debug($"EnableEntry - Writing changes to hosts file: {HostsFilePath}");
            File.WriteAllLines(HostsFilePath, hostsFileLines);
        }
        catch (Exception ex)
        {
            Log.Error($"EnableEntry - Failed to write changes to hosts file: {HostsFilePath}", ex);

            return false;
        }

        return true;
    }

    public static Task<bool> DisableEntryAsync(HostsFileEntry entry)
    {
        return Task.Run(() => DisableEntry(entry));
    }

    private static bool DisableEntry(HostsFileEntry entry)
    {
        // Create a backup of the hosts file before making changes
        if (CreateBackup() == false)
        {
            Log.Error("DisableEntry - Failed to create backup before disabling entry.");
            return false;
        }

        // Replace the entry in the hosts file
        var hostsFileLines = File.ReadAllLines(HostsFilePath).ToList();

        for (var i = 0; i < hostsFileLines.Count; i++)
        {
            if (hostsFileLines[i] == entry.Line)
                hostsFileLines[i] = "# " + entry.Line;
        }

        try
        {
            Log.Debug($"DisableEntry - Writing changes to hosts file: {HostsFilePath}");
            File.WriteAllLines(HostsFilePath, hostsFileLines);
        }
        catch (Exception ex)
        {
            Log.Error($"DisableEntry - Failed to write changes to hosts file: {HostsFilePath}", ex);

            return false;
        }

        return true;
    }

    /// <summary>
    /// Create a daily backup of the hosts file (before making a change).
    /// </summary>
    private static bool CreateBackup()
    {
        Log.Debug($"CreateBackup - Creating backup of hosts file: {HostsFilePath}");

        var dateTimeNow = DateTime.Now;

        // Check if a daily backup has already been created today (in the current running instance)
        if (_lastBackupTime.Date == dateTimeNow.Date)
        {
            Log.Debug("CreateBackup - Daily backup already created today. Skipping...");
            return true;
        }

        // Get existing backup files
        var backupFiles = Directory.GetFiles(HostsFolderPath, $"{HostsFileBackupIdentifier}_*")
            .OrderByDescending(f => f).ToList();

        Log.Debug($"CreateBackup - Found {backupFiles.Count} backup files in {HostsFolderPath}");

        // Cleanup old backups if they exceed the limit
        if (backupFiles.Count > HostsFileBackupsToKeep)
        {
            for (var i = HostsFileBackupsToKeep; i < backupFiles.Count; i++)
            {
                try
                {
                    Log.Debug($"CreateBackup - Deleting old backup file: {backupFiles[i]}");
                    File.Delete(backupFiles[i]);
                }
                catch (Exception ex)
                {
                    Log.Error($"CreateBackup - Failed to delete old backup file: {backupFiles[i]}", ex);
                }
            }
        }

        // Check if a daily backup already exists on disk (from previous instances)
        var dailyBackupFound = backupFiles.Count > 0 &&
                               backupFiles[0].Contains($"{HostsFileBackupIdentifier}_{dateTimeNow:yyyyMMdd}");

        if (dailyBackupFound)
        {
            Log.Debug("CreateBackup - Daily backup already exists on disk. Skipping...");

            _lastBackupTime = dateTimeNow;

            return true;
        }

        // Create a new backup file with the current date
        try
        {
            Log.Debug($"CreateBackup - Creating new backup file: {HostsFileBackupIdentifier}_{dateTimeNow:yyyyMMdd}");
            File.Copy(HostsFilePath,
                Path.Combine(HostsFolderPath, $"{HostsFileBackupIdentifier}_{dateTimeNow:yyyyMMdd}"));

            _lastBackupTime = dateTimeNow;
        }
        catch (Exception ex)
        {
            Log.Error($"CreateBackup - Failed to create backup file: {HostsFilePath}", ex);
            return false;
        }

        return true;
    }

    #endregion
}