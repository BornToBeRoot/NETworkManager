using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the bit calculator view.
/// </summary>
public class BitCalculatorViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(BitCalculatorViewModel));

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// Backing field for <see cref="Input"/>.
    /// </summary>
    private string _input;

    /// <summary>
    /// Gets or sets the input value.
    /// </summary>
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

    /// <summary>
    /// Gets the collection view for the input history.
    /// </summary>
    public ICollectionView InputHistoryView { get; }

    /// <summary>
    /// Backing field for <see cref="Units"/>.
    /// </summary>
    private readonly List<BitCaluclatorUnit> _units = new();

    /// <summary>
    /// Gets the list of available units.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="Unit"/>.
    /// </summary>
    private BitCaluclatorUnit _unit;

    /// <summary>
    /// Gets or sets the selected unit.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="IsRunning"/>.
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// Gets or sets a value indicating whether the calculation is running.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="IsResultVisible"/>.
    /// </summary>
    private bool _isResultVisible;

    /// <summary>
    /// Gets or sets a value indicating whether the result is visible.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="Result"/>.
    /// </summary>
    private BitCaluclatorInfo _result = new();

    /// <summary>
    /// Gets the calculation result.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="BitCalculatorViewModel"/> class.
    /// </summary>
    public BitCalculatorViewModel()
    {
        _isLoading = true;

        InputHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.BitCalculator_InputHistory);

        Units = Enum.GetValues(typeof(BitCaluclatorUnit)).Cast<BitCaluclatorUnit>().ToList();
        Unit = Units.First(x => x == SettingsManager.Current.BitCalculator_Unit);

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to calculate the result.
    /// </summary>
    public ICommand CalculateCommand => new RelayCommand(_ => CalculateAction(), Calculate_CanExecute);

    /// <summary>
    /// Checks if the calculate command can be executed.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> if the command can be executed; otherwise, <c>false</c>.</returns>
    private bool Calculate_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>
    /// Action to calculate the result.
    /// </summary>
    private void CalculateAction()
    {
        Calculate();
    }

    /// <summary>
    /// Gets the command to export the result.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    /// <summary>
    /// Action to export the result.
    /// </summary>
    private Task ExportAction()
    {
        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    [Result]);
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.BitCalculator_ExportFileType = instance.FileType;
            SettingsManager.Current.BitCalculator_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], false, SettingsManager.Current.BitCalculator_ExportFileType,
            SettingsManager.Current.BitCalculator_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Calculates the result based on the input.
    /// </summary>
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

    /// <summary>
    /// Adds the input to the history.
    /// </summary>
    /// <param name="input">The input string.</param>
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

    /// <summary>
    /// Called when the view becomes visible.
    /// </summary>
    public void OnViewVisible()
    {
    }

    /// <summary>
    /// Called when the view is hidden.
    /// </summary>
    public void OnViewHide()
    {
    }

    #endregion
}