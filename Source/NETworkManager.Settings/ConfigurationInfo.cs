using NETworkManager.Utilities;
using System;

namespace NETworkManager.Settings;

public class ConfigurationInfo : PropertyChangedBase
{
    public bool IsAdmin { get; set; }
    public string ExecutionPath { get; set; }
    public string ApplicationFullName { get; set; }
    public string ApplicationName { get; set; }
    public bool IsPortable { get; set; }
    public Version OSVersion { get; set; }

    // Everything below will be set dynamically in the application
    public bool ShowSettingsResetNoteOnStartup { get; set; }        
    public bool DisableSaveSettings { get; set; }
    public bool Restart { get; set; }

    private bool _isDialogOpen;
    public bool IsDialogOpen
    {
        get => _isDialogOpen;
        set
        {
            if (value == _isDialogOpen)
                return;

            _isDialogOpen = value;
            OnPropertyChanged();
        }
    }

    public ConfigurationInfo()
    {

    }
}
