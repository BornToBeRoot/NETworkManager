using System.Collections.Generic;

namespace NETworkManager.Profiles;

/// <summary>
///     A source-agnostic candidate that can be turned into a <see cref="ProfileInfo"/> by an import dialog.
/// </summary>
public sealed class ProfileImportCandidate(
    string name,
    string host,
    string description,
    ProfileImportSource importSource,
    string importSourceId = null)
{
    /// <summary>Profile name suggestion (e.g. sAMAccountName without trailing '$').</summary>
    public string Name { get; } = name ?? string.Empty;

    /// <summary>Host or DNS name used as the profile host. May be empty.</summary>
    public string Host { get; } = host ?? string.Empty;

    /// <summary>Optional free-form description.</summary>
    public string Description { get; } = description;

    /// <summary>Import method. Used together with <see cref="ImportSourceId"/> for duplicate detection on re-import.</summary>
    public ProfileImportSource ImportSource { get; } = importSource;

    /// <summary>Stable ID within the import source (e.g. AD objectGUID). Empty/null if not applicable.</summary>
    public string ImportSourceId { get; } = importSourceId;
}