using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace NETworkManager.Settings;

/// <summary>
///     Class contains local settings that are stored outside the main settings file.
///     These settings control where the main settings file is located.
/// </summary>
public class LocalSettingsInfo
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    /// <remarks>This event is typically used to notify subscribers that a property value has been updated. It
    /// is commonly implemented in classes that support data binding or need to signal changes to property
    /// values.</remarks>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Helper method to raise the <see cref="PropertyChanged" /> event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        SettingsChanged = true;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Variables

    [JsonIgnore] public bool SettingsChanged { get; set; }

    /// <summary>
    /// Private field for the <see cref="Settings_FolderLocation" /> property.
    /// </summary>
    private string _settings_FolderLocation;

    /// <summary>
    ///  Location of the folder where the local settings file is stored.
    ///  This can be changed by the user to move the settings file to a different location.
    /// </summary>
    public string Settings_FolderLocation
    {
        get => _settings_FolderLocation;
        set
        {
            if (_settings_FolderLocation == value)
                return;

            _settings_FolderLocation = value;
            OnPropertyChanged();
        }
    }
    #endregion
}
