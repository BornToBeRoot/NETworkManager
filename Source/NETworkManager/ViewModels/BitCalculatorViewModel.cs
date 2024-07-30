using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class BitCalculatorViewModel : ViewModelBase
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private static readonly ILog Log = LogManager.GetLogger(typeof(BitCalculatorViewModel));

    private readonly bool _isLoading;

    private string _input;

    public string Input
    {
        get => _input;
        set
        {
            if (value == _input)
                return;

            _input = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView InputHistoryView { get; }

    private readonly List<BitCaluclatorUnit> _units = new();

    public List<BitCaluclatorUnit> Units
    {
        get => _units;
        private init
        {
            if (value == _units)
                return;

            _units = value;
            OnPropertyChanged();
        }
    }

    private BitCaluclatorUnit _unit;

    public BitCaluclatorUnit Unit
    {
        get => _unit;
        set
        {
            if (value == _unit)
                return;

            if (!_isLoading)
                SettingsManager.Current.BitCalculator_Unit = value;

            _unit = value;
            OnPropertyChanged();
        }
    }

    private bool _isRunning;

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value == _isRunning)
                return;

            _isRunning = value;
            OnPropertyChanged();
        }
    }

    private bool _isResultVisible;

    public bool IsResultVisible
    {
        get => _isResultVisible;
        set
        {
            if (value == _isResultVisible)
                return;


            _isResultVisible = value;
            OnPropertyChanged();
        }
    }

    private BitCaluclatorInfo _result = new();

    public BitCaluclatorInfo Result
    {
        get => _result;
        private set
        {
            if (value == _result)
                return;

            _result = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public BitCalculatorViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;
        _dialogCoordinator = instance;

        InputHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.BitCalculator_InputHistory);

        Units = Enum.GetValues(typeof(BitCaluclatorUnit)).Cast<BitCaluclatorUnit>().ToList();
        Unit = Units.First(x => x == SettingsManager.Current.BitCalculator_Unit);

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
    }

    #endregion

    #region ICommands & Actions

    public ICommand CalculateCommand => new RelayCommand(_ => CalculateAction(), Calculate_CanExecute);

    private bool Calculate_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow)
                   .IsAnyDialogOpen;
    }

    private void CalculateAction()
    {
        Calculate();
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    private async Task ExportAction()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.Export
        };

        var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType,
                        [Result]);
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                        Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.BitCalculator_ExportFileType = instance.FileType;
                SettingsManager.Current.BitCalculator_ExportFilePath = instance.FilePath;
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], false, SettingsManager.Current.BitCalculator_ExportFileType,
            SettingsManager.Current.BitCalculator_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    #endregion

    #region Methods

    private async void Calculate()
    {
        IsResultVisible = false;
        IsRunning = true;

        if (double.TryParse(Input.Replace('.', ','), out var input))
            Result = await BitCaluclator.CalculateAsync(input, Unit, SettingsManager.Current.BitCalculator_Notation);
        else
            Log.Error($"Could not parse input \"{Input}\" into double!");

        IsResultVisible = true;

        AddInputToHistory(Input);

        IsRunning = false;
    }

    private void AddInputToHistory(string input)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.BitCalculator_InputHistory.ToList(), input,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.BitCalculator_InputHistory.Clear();
        OnPropertyChanged(nameof(Input)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.BitCalculator_InputHistory.Add(x));
    }

    public void OnViewVisible()
    {
    }

    public void OnViewHide()
    {
    }

    #endregion
}