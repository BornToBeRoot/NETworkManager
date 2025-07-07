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

    /// <summary>
    /// Enable a hosts file entry asynchronously.
    /// </summary>
    /// <param name="entry">Entry to enable.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was enabled successfully, otherwise an error result.</returns>
    public static Task<HostsFileEntryModifyResult> EnableEntryAsync(HostsFileEntry entry)
    {
        return Task.Run(() => EnableEntry(entry));
    }

    /// <summary>
    /// Enable a hosts file entry.
    /// </summary>
    /// <param name="entry">Entry to enable.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was enabled successfully, otherwise an error result.</returns>
    private static HostsFileEntryModifyResult EnableEntry(HostsFileEntry entry)
    {
        // Create a backup of the hosts file before making changes
        if (CreateBackup() == false)
        {
            Log.Error("EnableEntry - Failed to create backup before enabling entry.");
            return HostsFileEntryModifyResult.BackupError;
        }

        // Enable the entry in the hosts file
        List<string> hostsFileLines;

        try
        {
            hostsFileLines = [.. File.ReadAllLines(HostsFilePath)];
        }
        catch (Exception ex)
        {
            Log.Error($"EnableEntry - Failed to read hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.ReadError;
        }

        bool entryFound = false;

        for (var i = 0; i < hostsFileLines.Count; i++)
        {
            if (hostsFileLines[i] == entry.Line)
            {
                entryFound = true;

                hostsFileLines.RemoveAt(i);
                hostsFileLines.Insert(i, CreateEntryLine(new HostsFileEntry
                {
                    IsEnabled = true,
                    IPAddress = entry.IPAddress,
                    Hostname = entry.Hostname,
                    Comment = entry.Comment,
                    Line = entry.Line
                }));

                break;
            }
        }

        if (!entryFound)
        {
            Log.Warn($"EnableEntry - Entry not found in hosts file: {entry.Line}");

            return HostsFileEntryModifyResult.NotFound;
        }

        try
        {
            Log.Debug($"EnableEntry - Writing changes to hosts file: {HostsFilePath}");
            File.WriteAllLines(HostsFilePath, hostsFileLines);
        }
        catch (Exception ex)
        {
            Log.Error($"EnableEntry - Failed to write changes to hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.WriteError;
        }

        return HostsFileEntryModifyResult.Success;
    }

    /// <summary>
    /// Disable a hosts file entry asynchronously.
    /// </summary>
    /// <param name="entry">Entry to disable.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was enabled successfully, otherwise an error result.</returns>
    public static Task<HostsFileEntryModifyResult> DisableEntryAsync(HostsFileEntry entry)
    {
        return Task.Run(() => DisableEntry(entry));
    }

    /// <summary>
    /// Disable a hosts file entry.
    /// </summary>
    /// <param name="entry">Entry to disable.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was enabled successfully, otherwise an error result.</returns>
    private static HostsFileEntryModifyResult DisableEntry(HostsFileEntry entry)
    {
        // Create a backup of the hosts file before making changes
        if (CreateBackup() == false)
        {
            Log.Error("DisableEntry - Failed to create backup before disabling entry.");
            return HostsFileEntryModifyResult.BackupError;
        }

        // Disable the entry in the hosts file
        List<string> hostsFileLines;

        try
        {
            hostsFileLines = [.. File.ReadAllLines(HostsFilePath)];
        }
        catch (Exception ex)
        {
            Log.Error($"DisableEntry - Failed to read hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.ReadError;
        }

        bool entryFound = false;

        for (var i = 0; i < hostsFileLines.Count; i++)
        {
            if (hostsFileLines[i] == entry.Line)
            {
                entryFound = true;

                hostsFileLines.RemoveAt(i);
                hostsFileLines.Insert(i, CreateEntryLine(new HostsFileEntry
                {
                    IsEnabled = false,
                    IPAddress = entry.IPAddress,
                    Hostname = entry.Hostname,
                    Comment = entry.Comment,
                    Line = entry.Line
                }));

                break;
            }
        }

        if (!entryFound)
        {
            Log.Warn($"DisableEntry - Entry not found in hosts file: {entry.Line}");
            return HostsFileEntryModifyResult.NotFound;
        }

        try
        {
            Log.Debug($"DisableEntry - Writing changes to hosts file: {HostsFilePath}");
            File.WriteAllLines(HostsFilePath, hostsFileLines);
        }
        catch (Exception ex)
        {
            Log.Error($"DisableEntry - Failed to write changes to hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.WriteError;
        }

        return HostsFileEntryModifyResult.Success;
    }

    /// <summary>
    /// Add a hosts file entry asynchronously.
    /// </summary>
    /// <param name="entry">Entry to add.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was added successfully, otherwise an error result.</returns>"/>
    public static Task<HostsFileEntryModifyResult> AddEntryAsync(HostsFileEntry entry)
    {
        return Task.Run(() => AddEntry(entry));
    }

    /// <summary>
    /// Add a hosts file entry.
    /// </summary>
    /// <param name="entry">Entry to add.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was added successfully, otherwise an error result.</returns>"/>
    private static HostsFileEntryModifyResult AddEntry(HostsFileEntry entry)
    {
        // Create a backup of the hosts file before making changes
        if (CreateBackup() == false)
        {
            Log.Error("AddEntry - Failed to create backup before adding entry.");
            return HostsFileEntryModifyResult.BackupError;
        }

        // Add the entry to the hosts file       
        List<string> hostsFileLines;

        try
        {
            hostsFileLines = [.. File.ReadAllLines(HostsFilePath)];
        }
        catch (Exception ex)
        {
            Log.Error($"AddEntry - Failed to read hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.ReadError;
        }

        hostsFileLines.Add(CreateEntryLine(entry));

        try
        {
            Log.Debug($"AddEntry - Writing changes to hosts file: {HostsFilePath}");
            File.WriteAllLines(HostsFilePath, hostsFileLines);
        }
        catch (Exception ex)
        {
            Log.Error($"AddEntry - Failed to write changes to hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.WriteError;
        }

        return HostsFileEntryModifyResult.Success;
    }

    /// <summary>
    /// Edit a hosts file entry asynchronously.
    /// </summary>
    /// <param name="entry">Entry to edit.</param>
    /// <param name="newEntry">New entry to replace the old one.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was edited successfully, otherwise an error result.</returns>"/>
    public static Task<HostsFileEntryModifyResult> EditEntryAsync(HostsFileEntry entry, HostsFileEntry newEntry)
    {
        return Task.Run(() => EditEntry(entry, newEntry));
    }

    /// <summary>
    /// Edit a hosts file entry.
    /// </summary>
    /// <param name="entry">Entry to edit.</param>
    /// <param name="newEntry">New entry to replace the old one.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was edited successfully, otherwise an error result.</returns>"/>
    private static HostsFileEntryModifyResult EditEntry(HostsFileEntry entry, HostsFileEntry newEntry)
    {
        // Create a backup of the hosts file before making changes
        if (CreateBackup() == false)
        {
            Log.Error("EditEntry - Failed to create backup before editing entry.");
            return HostsFileEntryModifyResult.BackupError;
        }

        // Replace the entry from the hosts file
        List<string> hostsFileLines;

        try
        {
            hostsFileLines = [.. File.ReadAllLines(HostsFilePath)];
        }
        catch (Exception ex)
        {
            Log.Error($"EditEntry - Failed to read hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.ReadError;
        }

        bool entryFound = false;

        for (var i = 0; i < hostsFileLines.Count; i++)
        {
            if (hostsFileLines[i] == entry.Line)
            {
                entryFound = true;

                hostsFileLines.RemoveAt(i);
                hostsFileLines.Insert(i, CreateEntryLine(newEntry));

                break;
            }
        }

        if (!entryFound)
        {
            Log.Warn($"EditEntry - Entry not found in hosts file: {entry.Line}");
            return HostsFileEntryModifyResult.NotFound;
        }

        try
        {
            Log.Debug($"EditEntry - Writing changes to hosts file: {HostsFilePath}");
            File.WriteAllLines(HostsFilePath, hostsFileLines);
        }
        catch (Exception ex)
        {
            Log.Error($"EditEntry - Failed to write changes to hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.WriteError;
        }

        return HostsFileEntryModifyResult.Success;
    }

    /// <summary>
    /// Delete a hosts file entry asynchronously.
    /// </summary>
    /// <param name="entry">Entry to delete.</param>"/>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was enabled successfully, otherwise an error result.</returns>s
    public static Task<HostsFileEntryModifyResult> DeleteEntryAsync(HostsFileEntry entry)
    {
        return Task.Run(() => DeleteEntry(entry));
    }

    /// <summary>
    /// Delete a hosts file entry.
    /// </summary>
    /// <param name="entry">Entry to delete.</param>"/>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was enabled successfully, otherwise an error result.</returns>
    private static HostsFileEntryModifyResult DeleteEntry(HostsFileEntry entry)
    {
        // Create a backup of the hosts file before making changes
        if (CreateBackup() == false)
        {
            Log.Error("DeleteEntry - Failed to create backup before deleting entry.");
            return HostsFileEntryModifyResult.BackupError;
        }

        // Remove the entry from the hosts file
        List<string> hostsFileLines;

        try
        {
            hostsFileLines = [.. File.ReadAllLines(HostsFilePath)];
        }
        catch (Exception ex)
        {
            Log.Error($"DeleteEntry - Failed to read hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.ReadError;
        }

        bool entryFound = false;

        for (var i = 0; i < hostsFileLines.Count; i++)
        {
            if (hostsFileLines[i] == entry.Line)
            {
                entryFound = true;

                hostsFileLines.RemoveAt(i);

                break;
            }
        }

        if (!entryFound)
        {
            Log.Warn($"DeleteEntry - Entry not found in hosts file: {entry.Line}");
            return HostsFileEntryModifyResult.NotFound;
        }

        try
        {
            Log.Debug($"DeleteEntry - Writing changes to hosts file: {HostsFilePath}");
            File.WriteAllLines(HostsFilePath, hostsFileLines);
        }
        catch (Exception ex)
        {
            Log.Error($"DeleteEntry - Failed to write changes to hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.WriteError;
        }

        return HostsFileEntryModifyResult.Success;
    }

    /// <summary>
    /// Create a line for the hosts file entry.
    /// </summary>
    /// <param name="entry">Entry to create the line for.</param>
    /// <returns>Line for the hosts file entry.</returns>
    private static string CreateEntryLine(HostsFileEntry entry)
    {
        var line = entry.IsEnabled ? "" : "# ";

        line += $"{entry.IPAddress}    {entry.Hostname}";

        if (!string.IsNullOrWhiteSpace(entry.Comment))
        {
            line += $"        # {entry.Comment}";
        }

        return line.Trim();
    }

    /// <summary>
    /// Create a "daily" backup of the hosts file (before making a change).
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