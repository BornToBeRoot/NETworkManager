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
    public static string HostsFilePath => Path.Combine(HostsFolderPath, "hosts");

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
                Log.Debug($"GetHostsFileEntries - Line matched: {line}");

                var entry = new HostsFileEntry
                (
                    !result.Groups[1].Value.Equals("#"),
                    result.Groups[2].Value,
                    result.Groups[3].Value.Replace(@"\s", "").Trim(),
                    result.Groups[4].Value.TrimStart('#', ' '),
                    line
                );

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
                Log.Debug($"GetHostsFileEntries - Line not matched: {line}");
            }
        }

        return entries;
    }

    /// <summary>
    /// Enable multiple hosts file entries asynchronously.
    /// </summary>
    /// <param name="entries">Entries to enable.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if entries were enabled successfully, otherwise an error result.</returns>
    public static Task<HostsFileEntryModifyResult> EnableEntriesAsync(IEnumerable<HostsFileEntry> entries)
    {
        return Task.Run(() => ModifyEntries(entries, e => new HostsFileEntry(true, e.IPAddress, e.Hostname, e.Comment)));
    }

    /// <summary>
    /// Enable a hosts file entry asynchronously.
    /// </summary>
    /// <param name="entry">Entry to enable.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was enabled successfully, otherwise an error result.</returns>
    public static Task<HostsFileEntryModifyResult> EnableEntryAsync(HostsFileEntry entry)
    {
        return EnableEntriesAsync([entry]);
    }

    /// <summary>
    /// Disable multiple hosts file entries asynchronously.
    /// </summary>
    /// <param name="entries">Entries to disable.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if entries were disabled successfully, otherwise an error result.</returns>
    public static Task<HostsFileEntryModifyResult> DisableEntriesAsync(IEnumerable<HostsFileEntry> entries)
    {
        return Task.Run(() => ModifyEntries(entries, e => new HostsFileEntry(false, e.IPAddress, e.Hostname, e.Comment)));
    }

    /// <summary>
    /// Disable a hosts file entry asynchronously.
    /// </summary>
    /// <param name="entry">Entry to disable.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was disabled successfully, otherwise an error result.</returns>
    public static Task<HostsFileEntryModifyResult> DisableEntryAsync(HostsFileEntry entry)
    {
        return DisableEntriesAsync([entry]);
    }

    /// <summary>
    /// Edit the comment for multiple hosts file entries asynchronously.
    /// </summary>
    /// <param name="entries">Entries to update.</param>
    /// <param name="comment">The new comment to apply.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if entries were edited successfully, otherwise an error result.</returns>
    public static Task<HostsFileEntryModifyResult> EditCommentAsync(IEnumerable<HostsFileEntry> entries, string comment)
    {
        return Task.Run(() => ModifyEntries(entries, e => new HostsFileEntry(e.IsEnabled, e.IPAddress, e.Hostname, comment)));
    }

    /// <summary>
    /// Modifies multiple hosts file entries using a transform function.
    /// </summary>
    private static HostsFileEntryModifyResult ModifyEntries(IEnumerable<HostsFileEntry> entries, Func<HostsFileEntry, HostsFileEntry> transform)
    {
        var entriesList = entries?.ToList() ?? [];
        if (entriesList.Count == 0)
            return HostsFileEntryModifyResult.Success;

        // Create a backup of the hosts file before making changes
        if (CreateBackup() == false)
        {
            Log.Error("ModifyEntries - Failed to create backup before modifying entries.");
            return HostsFileEntryModifyResult.BackupError;
        }

        List<string> hostsFileLines;

        try
        {
            hostsFileLines = [.. File.ReadAllLines(HostsFilePath)];
        }
        catch (Exception ex)
        {
            Log.Error($"ModifyEntries - Failed to read hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.ReadError;
        }

        var anyModified = false;
        var pending = entriesList.ToList();

        for (var i = 0; i < hostsFileLines.Count; i++)
        {
            var matchIndex = pending.FindIndex(e => e.Line == hostsFileLines[i]);
            if (matchIndex >= 0)
            {
                var matched = pending[matchIndex];
                pending.RemoveAt(matchIndex);

                var newEntry = transform(matched);
                Log.Debug($"ModifyEntries - Replacing entry: {hostsFileLines[i]} with {newEntry.Line}");
                hostsFileLines[i] = newEntry.Line;
                anyModified = true;
            }
        }

        if (!anyModified && entriesList.Count > 0)
        {
            Log.Warn("ModifyEntries - None of the specified entries were found in hosts file.");
            return HostsFileEntryModifyResult.NotFound;
        }

        try
        {
            Log.Debug($"ModifyEntries - Writing changes to hosts file: {HostsFilePath}");
            File.WriteAllLines(HostsFilePath, hostsFileLines);
        }
        catch (Exception ex)
        {
            Log.Error($"ModifyEntries - Failed to write changes to hosts file: {HostsFilePath}", ex);
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

        Log.Debug($"AddEntry - Adding entry: {entry.Line}");
        hostsFileLines.Add(entry.Line);

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

        var entryFound = false;

        for (var i = 0; i < hostsFileLines.Count; i++)
        {
            if (hostsFileLines[i] != entry.Line)
                continue;

            entryFound = true;

            Log.Debug($"EditEntry - Found entry: {hostsFileLines[i]}");
            hostsFileLines.RemoveAt(i);

            Log.Debug($"EditEntry - Editing entry: {newEntry.Line}");
            hostsFileLines.Insert(i, newEntry.Line);

            break;
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
    /// Delete multiple hosts file entries asynchronously.
    /// </summary>
    /// <param name="entries">Entries to delete.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if entries were deleted successfully, otherwise an error result.</returns>
    public static Task<HostsFileEntryModifyResult> DeleteEntriesAsync(IEnumerable<HostsFileEntry> entries)
    {
        return Task.Run(() => DeleteEntries(entries));
    }

    /// <summary>
    /// Delete a hosts file entry asynchronously.
    /// </summary>
    /// <param name="entry">Entry to delete.</param>
    /// <returns><see cref="HostsFileEntryModifyResult.Success"/> if the entry was deleted successfully, otherwise an error result.</returns>
    public static Task<HostsFileEntryModifyResult> DeleteEntryAsync(HostsFileEntry entry)
    {
        return DeleteEntriesAsync([entry]);
    }

    /// <summary>
    /// Delete multiple hosts file entries.
    /// </summary>
    private static HostsFileEntryModifyResult DeleteEntries(IEnumerable<HostsFileEntry> entries)
    {
        var entriesList = entries?.ToList() ?? [];
        if (entriesList.Count == 0)
            return HostsFileEntryModifyResult.Success;

        // Create a backup of the hosts file before making changes
        if (CreateBackup() == false)
        {
            Log.Error("DeleteEntries - Failed to create backup before deleting entries.");
            return HostsFileEntryModifyResult.BackupError;
        }

        // Remove the entries from the hosts file
        List<string> hostsFileLines;

        try
        {
            hostsFileLines = [.. File.ReadAllLines(HostsFilePath)];
        }
        catch (Exception ex)
        {
            Log.Error($"DeleteEntries - Failed to read hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.ReadError;
        }

        var anyDeleted = false;
        var pending = entriesList.ToList();

        for (var i = hostsFileLines.Count - 1; i >= 0; i--)
        {
            var matchIndex = pending.FindIndex(e => e.Line == hostsFileLines[i]);
            if (matchIndex >= 0)
            {
                pending.RemoveAt(matchIndex);
                Log.Debug($"DeleteEntries - Found entry: {hostsFileLines[i]}");
                hostsFileLines.RemoveAt(i);
                anyDeleted = true;
            }
        }

        if (!anyDeleted && entriesList.Count > 0)
        {
            Log.Warn("DeleteEntries - None of the specified entries were found in hosts file.");
            return HostsFileEntryModifyResult.NotFound;
        }

        try
        {
            Log.Debug($"DeleteEntries - Writing changes to hosts file: {HostsFilePath}");
            File.WriteAllLines(HostsFilePath, hostsFileLines);
        }
        catch (Exception ex)
        {
            Log.Error($"DeleteEntries - Failed to write changes to hosts file: {HostsFilePath}", ex);
            return HostsFileEntryModifyResult.WriteError;
        }

        return HostsFileEntryModifyResult.Success;
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