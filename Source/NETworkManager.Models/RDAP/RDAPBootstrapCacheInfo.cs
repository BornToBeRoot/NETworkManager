using System;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Status of a cached IANA bootstrap file.
/// </summary>
public class RDAPBootstrapCacheInfo
{
    /// <summary>
    ///     Type of the bootstrap file.
    /// </summary>
    public RDAPBootstrapFileType Type { get; set; }

    /// <summary>
    ///     File name of the bootstrap file (e.g. dns.json).
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    ///     Publication date from the file itself. This can be years old while the data is still current.
    /// </summary>
    public string Publication { get; set; }

    /// <summary>
    ///     Time when the file content was last downloaded (HTTP 200).
    /// </summary>
    public DateTime? Downloaded { get; set; }

    /// <summary>
    ///     Time when the server was last asked for a newer version (HTTP 200 or 304).
    /// </summary>
    public DateTime? LastChecked { get; set; }

    /// <summary>
    ///     Size of the cached file in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    ///     ETag of the cached file, used for conditional requests.
    /// </summary>
    public string ETag { get; set; }

    /// <summary>
    ///     Last-Modified header of the cached file, used for conditional requests.
    /// </summary>
    public DateTimeOffset? LastModified { get; set; }

    /// <summary>
    ///     Time until the cached file is considered fresh (from Cache-Control max-age or Expires).
    /// </summary>
    public DateTime? Expires { get; set; }
}
