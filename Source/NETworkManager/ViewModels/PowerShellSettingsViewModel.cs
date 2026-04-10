using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.PowerShell;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class PowerShellSettingsViewModel : ViewModelBase
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
                SettingsManager.Current.PowerShell_ApplicationFilePath = value;

            IsConfigured = !string.IsNullOrEmpty(value);

            field = value;
            OnPropertyChanged();
        }
    }

    public string Command
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PowerShell_Command = value;

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
                SettingsManager.Current.PowerShell_AdditionalCommandLine = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public List<ExecutionPolicy> ExecutionPolicies
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ExecutionPolicy ExecutionPolicy
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PowerShell_ExecutionPolicy = value;

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

    #endregion

    #region Contructor, load settings

    public PowerShellSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ApplicationFilePath = SettingsManager.Current.PowerShell_ApplicationFilePath;
        IsConfigured = File.Exists(ApplicationFilePath);
        Command = SettingsManager.Current.PowerShell_Command;
        AdditionalCommandLine = SettingsManager.Current.PowerShell_AdditionalCommandLine;

        LoadExecutionPolicies();
    }

    private void LoadExecutionPolicies()
    {
        ExecutionPolicies = Enum.GetValues(typeof(ExecutionPolicy)).Cast<ExecutionPolicy>().ToList();
        ExecutionPolicy =
            ExecutionPolicies.FirstOrDefault(x => x == SettingsManager.Current.PowerShell_ExecutionPolicy);
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
            Process.Start(SettingsManager.Current.PowerShell_ApplicationFilePath);
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