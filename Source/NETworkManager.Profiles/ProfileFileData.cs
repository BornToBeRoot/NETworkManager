using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace NETworkManager.Profiles;

/// <summary>
/// Represents the data structure for a profile file, including versioning, backup information, and groups of profiles.
/// </summary>
/// <remarks>This class supports property change notification through the <see cref="INotifyPropertyChanged"/>
/// interface, allowing consumers to track changes to its properties. The <see cref="ProfilesChanged"/> property
/// indicates whether the data has been modified since it was last saved, but is not persisted when serializing the
/// object. Use this class to manage and persist user profile data, including handling schema migrations via the <see
/// cref="Version"/> property.</remarks>
public class ProfileFileData : INotifyPropertyChanged
{
    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///     Helper method to raise the <see cref="PropertyChanged" /> event and mark data as changed.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        ProfilesChanged = true;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Indicates if the profile file data has been modified and needs to be saved.
    ///     This is not serialized to the file.
    /// </summary>
    [JsonIgnore]
    public bool ProfilesChanged { get; set; }

    private int _version = 1;

    /// <summary>
    ///     Schema version for handling future migrations.
    /// </summary>
    public int Version
    {
        get => _version;
        set
        {
            if (value == _version)
                return;

            _version = value;
            OnPropertyChanged();
        }
    }

    private DateTime? _lastBackup;

    /// <summary>
    ///     Date of the last backup (used for daily backup tracking).
    /// </summary>
    public DateTime? LastBackup
    {
        get => _lastBackup;
        set
        {
            if (value == _lastBackup)
                return;

            _lastBackup = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Working copy of groups containing profiles (runtime objects with SecureString passwords).
    ///     This is the single source of truth for profile data in memory.
    ///     Not serialized directly - use <see cref="GroupsSerializable"/> for JSON serialization.
    /// </summary>
    [JsonIgnore]
    public List<GroupInfo> Groups { get; set; } = [];

    /// <summary>
    ///     Serializable representation of groups for JSON persistence.
    ///     Gets: Converts working Groups to serializable format.
    ///     Sets: Converts deserialized data back to working Groups.
    /// </summary>
    [JsonPropertyName("Groups")]
    public List<GroupInfoSerializable> GroupsSerializable
    {
        get => [.. Groups.Select(g => new GroupInfoSerializable(g)
        {
            Profiles = [.. g.Profiles.Where(p => !p.IsDynamic).Select(p => new ProfileInfoSerializable(p)
            {
                RemoteDesktop_Password = p.RemoteDesktop_Password != null
                    ? Utilities.SecureStringHelper.ConvertToString(p.RemoteDesktop_Password)
                    : string.Empty,
                RemoteDesktop_GatewayServerPassword = p.RemoteDesktop_GatewayServerPassword != null
                    ? Utilities.SecureStringHelper.ConvertToString(p.RemoteDesktop_GatewayServerPassword)
                    : string.Empty,
                SNMP_Community = p.SNMP_Community != null
                    ? Utilities.SecureStringHelper.ConvertToString(p.SNMP_Community)
                    : string.Empty,
                SNMP_Auth = p.SNMP_Auth != null
                    ? Utilities.SecureStringHelper.ConvertToString(p.SNMP_Auth)
                    : string.Empty,
                SNMP_Priv = p.SNMP_Priv != null
                    ? Utilities.SecureStringHelper.ConvertToString(p.SNMP_Priv)
                    : string.Empty
            })],
            RemoteDesktop_Password = g.RemoteDesktop_Password != null
                ? Utilities.SecureStringHelper.ConvertToString(g.RemoteDesktop_Password)
                : string.Empty,
            RemoteDesktop_GatewayServerPassword = g.RemoteDesktop_GatewayServerPassword != null
                ? Utilities.SecureStringHelper.ConvertToString(g.RemoteDesktop_GatewayServerPassword)
                : string.Empty,
            SNMP_Community = g.SNMP_Community != null
                ? Utilities.SecureStringHelper.ConvertToString(g.SNMP_Community)
                : string.Empty,
            SNMP_Auth = g.SNMP_Auth != null
                ? Utilities.SecureStringHelper.ConvertToString(g.SNMP_Auth)
                : string.Empty,
            SNMP_Priv = g.SNMP_Priv != null
                ? Utilities.SecureStringHelper.ConvertToString(g.SNMP_Priv)
                : string.Empty
        }).Where(g => !g.IsDynamic)];

        set
        {
            Groups = value?.Select(gs => new GroupInfo(gs)
            {
                Profiles = [.. (gs.Profiles ?? []).Select(ps => new ProfileInfo(ps)
                {
                    TagsCollection = (ps.TagsCollection == null || ps.TagsCollection.Count == 0) && !string.IsNullOrEmpty(ps.Tags)
                        ? new Controls.ObservableSetCollection<string>(ps.Tags.Split([';'], StringSplitOptions.RemoveEmptyEntries))
                        : ps.TagsCollection,
                    RemoteDesktop_Password = !string.IsNullOrEmpty(ps.RemoteDesktop_Password)
                        ? Utilities.SecureStringHelper.ConvertToSecureString(ps.RemoteDesktop_Password)
                        : null,
                    RemoteDesktop_GatewayServerPassword = !string.IsNullOrEmpty(ps.RemoteDesktop_GatewayServerPassword)
                        ? Utilities.SecureStringHelper.ConvertToSecureString(ps.RemoteDesktop_GatewayServerPassword)
                        : null,
                    SNMP_Community = !string.IsNullOrEmpty(ps.SNMP_Community)
                        ? Utilities.SecureStringHelper.ConvertToSecureString(ps.SNMP_Community)
                        : null,
                    SNMP_Auth = !string.IsNullOrEmpty(ps.SNMP_Auth)
                        ? Utilities.SecureStringHelper.ConvertToSecureString(ps.SNMP_Auth)
                        : null,
                    SNMP_Priv = !string.IsNullOrEmpty(ps.SNMP_Priv)
                        ? Utilities.SecureStringHelper.ConvertToSecureString(ps.SNMP_Priv)
                        : null
                })],
                RemoteDesktop_Password = !string.IsNullOrEmpty(gs.RemoteDesktop_Password)
                    ? Utilities.SecureStringHelper.ConvertToSecureString(gs.RemoteDesktop_Password)
                    : null,
                RemoteDesktop_GatewayServerPassword = !string.IsNullOrEmpty(gs.RemoteDesktop_GatewayServerPassword)
                    ? Utilities.SecureStringHelper.ConvertToSecureString(gs.RemoteDesktop_GatewayServerPassword)
                    : null,
                SNMP_Community = !string.IsNullOrEmpty(gs.SNMP_Community)
                    ? Utilities.SecureStringHelper.ConvertToSecureString(gs.SNMP_Community)
                    : null,
                SNMP_Auth = !string.IsNullOrEmpty(gs.SNMP_Auth)
                    ? Utilities.SecureStringHelper.ConvertToSecureString(gs.SNMP_Auth)
                    : null,
                SNMP_Priv = !string.IsNullOrEmpty(gs.SNMP_Priv)
                    ? Utilities.SecureStringHelper.ConvertToSecureString(gs.SNMP_Priv)
                    : null
            }).ToList() ?? [];
        }
    }
}
