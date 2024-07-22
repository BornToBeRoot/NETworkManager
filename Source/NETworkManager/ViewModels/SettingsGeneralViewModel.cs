using NETworkManager.Models;
using NETworkManager.Settings;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace NETworkManager.ViewModels;

public class SettingsGeneralViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public ICollectionView Applications { get; private set; }

    private ApplicationInfo _applicationSelectedItem;

    public ApplicationInfo ApplicationSelectedItem
    {
        get => _applicationSelectedItem;
        set
        {
            if (Equals(value, _applicationSelectedItem))
                return;

            _applicationSelectedItem = value;
            OnPropertyChanged();
        }
    }

    private int _backgroundJobInterval;

    public int BackgroundJobInterval
    {
        get => _backgroundJobInterval;
        set
        {
            if (value == _backgroundJobInterval)
                return;

            if (!_isLoading)
                SettingsManager.Current.General_BackgroundJobInterval = value;

            _backgroundJobInterval = value;
            OnPropertyChanged();
        }
    }

    private int _threadPoolAdditionalMinThreads;

    public int ThreadPoolAdditionalMinThreads
    {
        get => _threadPoolAdditionalMinThreads;
        set
        {
            if (value == _threadPoolAdditionalMinThreads)
                return;

            if (!_isLoading)
                SettingsManager.Current.General_ThreadPoolAdditionalMinThreads = value;

            _threadPoolAdditionalMinThreads = value;
            OnPropertyChanged();
        }
    }

    private int _historyListEntries;

    public int HistoryListEntries
    {
        get => _historyListEntries;
        set
        {
            if (value == _historyListEntries)
                return;

            if (!_isLoading)
                SettingsManager.Current.General_HistoryListEntries = value;

            _historyListEntries = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, LoadSettings

    public SettingsGeneralViewModel()
    {
        _isLoading = true;

        LoadSettings();

        SettingsManager.Current.General_ApplicationList.CollectionChanged += General_ApplicationList_CollectionChanged;

        _isLoading = false;
    }

    private void General_ApplicationList_CollectionChanged(object sender,
        NotifyCollectionChangedEventArgs e)
    {
        Applications.Refresh();
    }

    private void LoadSettings()
    {
        Applications = new CollectionViewSource
        {
            Source = SettingsManager.Current.General_ApplicationList
        }.View;

        BackgroundJobInterval = SettingsManager.Current.General_BackgroundJobInterval;
        ThreadPoolAdditionalMinThreads = SettingsManager.Current.General_ThreadPoolAdditionalMinThreads;
        HistoryListEntries = SettingsManager.Current.General_HistoryListEntries;
    }

    #endregion
}