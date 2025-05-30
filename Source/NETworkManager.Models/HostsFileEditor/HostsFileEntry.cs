namespace NETworkManager.Models.HostsFileEditor;

/// <summary>
/// Class that represents a single entry in the hosts file.
/// </summary>
public class HostsFileEntry
{
    /// <summary>
    /// Indicates whether the entry is enabled or not.
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// IP address of the host.
    /// </summary>
    public string IPAddress { get; init; }

    /// <summary>
    /// Host name(s) of the host. Multiple host names are separated by a
    /// space (equal to the hosts file format).
    /// </summary>
    public string Hostname { get; init; }

    /// <summary>
    /// Comment of the host.
    /// </summary>
    public string Comment { get; init; }
    
    /// <summary>
    /// Line of the entry in the hosts file.
    /// </summary>
    public string Line { get; init; }

    /// <summary>
    /// Creates a new instance of <see cref="HostsFileEntry" />.
    /// </summary>
    public HostsFileEntry()
    {

    }

    /// <summary>
    /// Creates a new instance of <see cref="HostsFileEntry" /> with parameters.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the entry is enabled or not.</param>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="hostname">Host name(s) of the host.</param>
    /// <param name="comment">Comment of the host.</param>
    public HostsFileEntry(bool isEnabled, string ipAddress, string hostname, string comment)
    {
        IsEnabled = isEnabled;
        IPAddress = ipAddress;
        Hostname = hostname;
        Comment = comment;
    }
    
    /// <summary>
    /// Creates a new instance of <see cref="HostsFileEntry" /> with parameters.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the entry is enabled or not.</param>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="hostname">Host name(s) of the host.</param>
    /// <param name="comment">Comment of the host.</param>
    /// <param name="line">Line of the entry in the hosts file.</param>
    public HostsFileEntry(bool isEnabled, string ipAddress, string hostname, string comment, string line) : this(isEnabled, ipAddress, hostname, comment)
    {
        Line = line;
    }
}
