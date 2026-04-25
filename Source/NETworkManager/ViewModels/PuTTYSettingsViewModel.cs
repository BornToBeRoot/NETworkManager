using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.PuTTY;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using PuTTY = NETworkManager.Settings.Application.PuTTY;

namespace NETworkManager.ViewModels;

public class PuTTYSettingsViewModel : ViewModelBase
{
    #region Variables
    public bool IsPortable => ConfigurationManager.Current.IsPortable;

    public string PortableLogPath => PuTTY.PortableLogPath;

    private readonly bool _isLoading;

    public string ApplicationFilePath
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_ApplicationFilePath = value;

            IsConfigured = !string.IsNullOrEmpty(value);

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsConfigured
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseSSH
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
                SettingsManager.Current.PuTTY_DefaultConnectionMode = ConnectionMode.SSH;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseTelnet
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
                SettingsManager.Current.PuTTY_DefaultConnectionMode = ConnectionMode.Telnet;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseSerial
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
                SettingsManager.Current.PuTTY_DefaultConnectionMode = ConnectionMode.Serial;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseRlogin
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
                SettingsManager.Current.PuTTY_DefaultConnectionMode = ConnectionMode.Rlogin;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseRAW
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
                SettingsManager.Current.PuTTY_DefaultConnectionMode = ConnectionMode.RAW;

            field = value;
            OnPropertyChanged();
        }
    }

    public string Username
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_Username = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PrivateKeyFile
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_PrivateKeyFile = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public string Profile
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_Profile = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool EnableLog
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_EnableSessionLog = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<LogMode> LogModes => Enum.GetValues(typeof(LogMode)).Cast<LogMode>();

    public LogMode LogMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_LogMode = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public string LogPath
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_LogPath = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public string LogFileName
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_LogFileName = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public string AdditionalCommandLine
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_AdditionalCommandLine = value;

            field = value;
            OnPropertyChanged();
        }
    }


    public string SerialLine
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_SerialLine = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public int SSHPort
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_SSHPort = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public int TelnetPort
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_TelnetPort = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public int BaudRate
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_BaudRate = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public int RloginPort
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PuTTY_RloginPort = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Contructor, load settings

    public PuTTYSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ApplicationFilePath = SettingsManager.Current.PuTTY_ApplicationFilePath;

        switch (SettingsManager.Current.PuTTY_DefaultConnectionMode)
        {
            case ConnectionMode.SSH:
                UseSSH = true;
                break;
            case ConnectionMode.Telnet:
                UseTelnet = true;
                break;
            case ConnectionMode.Serial:
                UseSerial = true;
                break;
            case ConnectionMode.Rlogin:
                UseRlogin = true;
                break;
            case ConnectionMode.RAW:
                UseRAW = true;
                break;
        }

        IsConfigured = File.Exists(ApplicationFilePath);
        Username = SettingsManager.Current.PuTTY_Username;
        PrivateKeyFile = SettingsManager.Current.PuTTY_PrivateKeyFile;
        Profile = SettingsManager.Current.PuTTY_Profile;
        EnableLog = SettingsManager.Current.PuTTY_EnableSessionLog;
        LogMode = LogModes.FirstOrDefault(x => x == SettingsManager.Current.PuTTY_LogMode);
        LogPath = SettingsManager.Current.PuTTY_LogPath;
        LogFileName = SettingsManager.Current.PuTTY_LogFileName;
        AdditionalCommandLine = SettingsManager.Current.PuTTY_AdditionalCommandLine;
        SerialLine = SettingsManager.Current.PuTTY_SerialLine;
        SSHPort = SettingsManager.Current.PuTTY_SSHPort;
        TelnetPort = SettingsManager.Current.PuTTY_TelnetPort;
        BaudRate = SettingsManager.Current.PuTTY_BaudRate;
        RloginPort = SettingsManager.Current.PuTTY_RloginPort;
    }

    #endregion

    #region ICommands & Actions

    public ICommand ApplicationBrowseFileCommand => new RelayCommand(_ => ApplicationBrowseFileAction());

    private void ApplicationBrowseFileAction()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = GlobalStaticConfiguration.ApplicationFileExtensionFilter
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
            ApplicationFilePath = openFileDialog.FileName;
    }

    public ICommand ConfigureCommand => new RelayCommand(_ => ConfigureAction());

    private void ConfigureAction()
    {
        Configure().ConfigureAwait(false);
    }

    public ICommand PrivateKeyFileBrowseFileCommand => new RelayCommand(_ => PrivateKeyFileBrowseFileAction());

    private void PrivateKeyFileBrowseFileAction()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = GlobalStaticConfiguration.PuTTYPrivateKeyFileExtensionFilter
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
            PrivateKeyFile = openFileDialog.FileName;
    }

    public ICommand LogPathBrowseFolderCommand => new RelayCommand(_ => LogPathBrowseFolderAction());

    private void LogPathBrowseFolderAction()
    {
        var openFolderDialog = new FolderBrowserDialog
        {
            ShowNewFolderButton = true
        };

        if (openFolderDialog.ShowDialog() == DialogResult.OK)
            LogPath = openFolderDialog.SelectedPath;
    }

    #endregion

    #region Methods

    private async Task Configure()
    {
        try
        {
            Process.Start(SettingsManager.Current.PuTTY_ApplicationFilePath);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(System.Windows.Application.Current.MainWindow, Strings.Error, ex.Message, ChildWindowIcon.Error);
        }
    }

    /// <summary>
    ///     Method to set the <see cref="ApplicationFilePath" /> from drag and drop.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    public void SetApplicationFilePathFromDragDrop(string filePath)
    {
        ApplicationFilePath = filePath;

        OnPropertyChanged(nameof(ApplicationFilePath));
    }

    /// <summary>
    ///     Method to set the <see cref="PrivateKeyFile" /> drag drop.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    public void SetPrivateKeyFilePathFromDragDrop(string filePath)
    {
        PrivateKeyFile = filePath;

        OnPropertyChanged(nameof(PrivateKeyFile));
    }

    /// <summary>
    ///     Method to set the <see cref="LogPath" /> from drag and drop.
    /// </summary>
    /// <param name="folderPath">Path to the folder.</param>
    public void SetLogPathFolderPathFromDragDrop(string folderPath)
    {
        LogPath = folderPath;

        OnPropertyChanged(nameof(LogPath));
    }

    #endregion
}