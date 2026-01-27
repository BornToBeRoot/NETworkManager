using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    ///     List of groups containing profiles.
    /// </summary>
    public List<GroupInfoSerializable> Groups { get; set; } = [];
}
