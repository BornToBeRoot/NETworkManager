using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class TigerVNCSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public string ApplicationFilePath
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.TigerVNC_ApplicationFilePath = value;

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

    public int Port
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.TigerVNC_Port = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Contructor, load settings

    public TigerVNCSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ApplicationFilePath = SettingsManager.Current.TigerVNC_ApplicationFilePath;
        IsConfigured = File.Exists(ApplicationFilePath);
        Port = SettingsManager.Current.TigerVNC_Port;
    }

    #endregion

    #region ICommands & Actions

    public ICommand BrowseFileCommand => new RelayCommand(_ => BrowseFileAction());

    private void BrowseFileAction()
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

    #endregion

    #region Methods

    private async Task Configure()
    {
        try
        {
            Process.Start(SettingsManager.Current.TigerVNC_ApplicationFilePath);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(System.Windows.Application.Current.MainWindow, Strings.Error, ex.Message, ChildWindowIcon.Error);
        }
    }

    public void SetFilePathFromDragDrop(string filePath)
    {
        ApplicationFilePath = filePath;

        OnPropertyChanged(nameof(ApplicationFilePath));
    }

    #endregion
}