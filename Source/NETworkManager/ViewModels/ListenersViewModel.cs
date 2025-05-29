﻿using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

public class ListenersViewModel : ViewModelBase
{
    #region Contructor, load settings

    public ListenersViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        // Result view + search
        ResultsView = CollectionViewSource.GetDefaultView(Results);

        ((ListCollectionView)ResultsView).CustomSort = Comparer<ListenerInfo>.Create((x, y) =>
            IPAddressHelper.CompareIPAddresses(x.IPAddress, y.IPAddress));

        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not ListenerInfo info)
                return false;

            // Search by IP Address, Port and Protocol
            return info.IPAddress.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Port.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Protocol.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Get listeners
        Refresh(true).ConfigureAwait(false);

        // Auto refresh
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetDefaults);
        SelectedAutoRefreshTime = AutoRefreshTimes.SourceCollection.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x =>
            x.Value == SettingsManager.Current.Listeners_AutoRefreshTime.Value &&
            x.TimeUnit == SettingsManager.Current.Listeners_AutoRefreshTime.TimeUnit);
        AutoRefreshEnabled = SettingsManager.Current.Listeners_AutoRefreshEnabled;

        _isLoading = false;
    }

    #endregion

    #region Events

    private async void AutoRefreshTimer_Tick(object sender, EventArgs e)
    {
        // Stop timer...
        _autoRefreshTimer.Stop();

        // Refresh
        await Refresh();

        // Restart timer...
        _autoRefreshTimer.Start();
    }

    #endregion

    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(ListenersViewModel));

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly bool _isLoading;
    private readonly DispatcherTimer _autoRefreshTimer = new();

    private string _search;

    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;

            ResultsView.Refresh();

            OnPropertyChanged();
        }
    }

    private ObservableCollection<ListenerInfo> _results = new();

    public ObservableCollection<ListenerInfo> Results
    {
        get => _results;
        set
        {
            if (value == _results)
                return;

            _results = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView ResultsView { get; }

    private ListenerInfo _selectedResult;

    public ListenerInfo SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (value == _selectedResult)
                return;

            _selectedResult = value;
            OnPropertyChanged();
        }
    }

    private IList _selectedResults = new ArrayList();

    public IList SelectedResults
    {
        get => _selectedResults;
        set
        {
            if (Equals(value, _selectedResults))
                return;

            _selectedResults = value;
            OnPropertyChanged();
        }
    }

    private bool _autoRefreshEnabled;

    public bool AutoRefreshEnabled
    {
        get => _autoRefreshEnabled;
        set
        {
            if (value == _autoRefreshEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.Listeners_AutoRefreshEnabled = value;

            _autoRefreshEnabled = value;

            // Start timer to refresh automatically
            if (value)
            {
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(SelectedAutoRefreshTime);
                _autoRefreshTimer.Start();
            }
            else
            {
                _autoRefreshTimer.Stop();
            }

            OnPropertyChanged();
        }
    }

    public ICollectionView AutoRefreshTimes { get; }

    private AutoRefreshTimeInfo _selectedAutoRefreshTime;

    public AutoRefreshTimeInfo SelectedAutoRefreshTime
    {
        get => _selectedAutoRefreshTime;
        set
        {
            if (value == _selectedAutoRefreshTime)
                return;

            if (!_isLoading)
                SettingsManager.Current.Listeners_AutoRefreshTime = value;

            _selectedAutoRefreshTime = value;

            if (AutoRefreshEnabled)
            {
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(value);
                _autoRefreshTimer.Start();
            }

            OnPropertyChanged();
        }
    }

    private bool _isRefreshing;

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (value == _isRefreshing)
                return;

            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    private bool _isStatusMessageDisplayed;

    public bool IsStatusMessageDisplayed
    {
        get => _isStatusMessageDisplayed;
        set
        {
            if (value == _isStatusMessageDisplayed)
                return;

            _isStatusMessageDisplayed = value;
            OnPropertyChanged();
        }
    }

    private string _statusMessage;

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (value == _statusMessage)
                return;

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands & Actions

    public ICommand RefreshCommand => new RelayCommand(_ => RefreshAction().ConfigureAwait(false), Refresh_CanExecute);

    private bool Refresh_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen &&
               !IsRefreshing &&
               !AutoRefreshEnabled;
    }

    private async Task RefreshAction()
    {
        IsStatusMessageDisplayed = false;

        await Refresh();
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
                    instance.ExportAll
                        ? Results
                        : new ObservableCollection<ListenerInfo>(SelectedResults.Cast<ListenerInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.Listeners_ExportFileType = instance.FileType;
            SettingsManager.Current.Listeners_ExportFilePath = instance.FilePath;
        }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new[]
        {
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        }, true, SettingsManager.Current.Listeners_ExportFileType, SettingsManager.Current.Listeners_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    #endregion

    #region Methods

    private async Task Refresh(bool init = false)
    {
        IsRefreshing = true;

        StatusMessage = Strings.RefreshingDots;
        IsStatusMessageDisplayed = true;

        if (init == false)
            await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        Results.Clear();

        (await Listener.GetAllActiveListenersAsync()).ForEach(Results.Add);

        StatusMessage = string.Format(Strings.ReloadedAtX, DateTime.Now.ToShortTimeString());
        IsStatusMessageDisplayed = true;

        IsRefreshing = false;
    }

    public void OnViewVisible()
    {
        // Restart timer...
        if (AutoRefreshEnabled)
            _autoRefreshTimer.Start();
    }

    public void OnViewHide()
    {
        // Temporarily stop timer...
        if (AutoRefreshEnabled)
            _autoRefreshTimer.Stop();
    }

    #endregion
}