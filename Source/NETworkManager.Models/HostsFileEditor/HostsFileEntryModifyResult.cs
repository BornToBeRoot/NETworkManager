namespace NETworkManager.Models.HostsFileEditor
{
    /// <summary>
    ///     Represents the result of an attempt to modify a hosts file entry.
    /// </summary>
    public enum HostsFileEntryModifyResult
    {
        /// <summary>
        ///     The entry was modified successfully and the hosts file was updated.
        /// </summary>
        Success,

        /// <summary>
        ///     The entry was not found in the hosts file.
        /// </summary>
        NotFound,

        /// <summary>
        ///     An error occurred while reading the hosts file.
        /// </summary>
        ReadError,

        /// <summary>
        ///     An error occurred while writing to the hosts file.
        /// </summary>
        WriteError,

        /// <summary>
        ///     An error occurred while backing up the hosts file.
        /// </summary>
        BackupError,
    }
}
