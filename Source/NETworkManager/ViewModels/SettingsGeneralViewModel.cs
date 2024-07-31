using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Models;
using NETworkManager.Settings;
using NETworkManager.Utilities;

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

        SettingsManager.Current.General_ApplicationList.CollectionChanged += (_, _) => Applications.Refresh();

        _isLoading = false;
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

    #region ICommand & Actions

    public ICommand SetDefaultApplicationCommand => new RelayCommand(_ => SetDefaultApplicationAction());

    private void SetDefaultApplicationAction()
    {
        if (ApplicationSelectedItem == null)
            return;

        SetDefaultApplication(ApplicationSelectedItem.Name);
    }

    public ICommand ShowApplicationCommand => new RelayCommand(_ => ShowApplicationAction());

    private void ShowApplicationAction()
    {
        if (ApplicationSelectedItem == null)
            return;

        ChangeApplicationVisibility(ApplicationSelectedItem.Name, true);
    }

    public ICommand HideApplicationCommand => new RelayCommand(_ => HideApplicationAction());

    private void HideApplicationAction()
    {
        if (ApplicationSelectedItem == null)
            return;

        ChangeApplicationVisibility(ApplicationSelectedItem.Name, false);
    }

    public ICommand RestoreApplicationsDefaultsCommand => new RelayCommand(_ => RestoreApplicationsDefaultsAction());

    private void RestoreApplicationsDefaultsAction()
    {
        // Sort
        var defaultList = ApplicationManager.GetDefaultList().ToList();

        var indexMap = SettingsManager.Current.General_ApplicationList
            .Select((item, index) => new { Item = item, Index = defaultList.IndexOf(item) })
            .OrderBy(x => x.Index)
            .ToList();

        for (var i = 0; i < indexMap.Count; i++)
        {
            var currentIndex = SettingsManager.Current.General_ApplicationList.IndexOf(indexMap[i].Item);

            if (currentIndex != i)
                SettingsManager.Current.General_ApplicationList.Move(currentIndex, i);
        }

        // Visible        
        var hiddenApplications = SettingsManager.Current.General_ApplicationList.Where(x => x.IsVisible == false);

        foreach (var hiddenApplication in hiddenApplications.ToList())
            ChangeApplicationVisibility(hiddenApplication.Name, true);

        // Default
        SetDefaultApplication(ApplicationName.Dashboard);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Set the default application that will be shown when the application starts.
    /// </summary>
    /// <param name="applicationName">Name of the application.</param>
    private void SetDefaultApplication(ApplicationName applicationName)
    {
        foreach (var application in SettingsManager.Current.General_ApplicationList)
            application.IsDefault = application.Name == applicationName;

        SettingsManager.Current.SettingsChanged = true;
    }

    /// <summary>
    ///     Change the visibility of an application.
    /// </summary>
    /// <param name="applicationName">Name of the application.</param>
    /// <param name="isVisible">If set to <c>true</c> the application will be visible.</param>
    private void ChangeApplicationVisibility(ApplicationName applicationName, bool isVisible)
    {
        var application =
            SettingsManager.Current.General_ApplicationList.FirstOrDefault(x => x.Name == applicationName);

        var index = SettingsManager.Current.General_ApplicationList.IndexOf(application);

        application.IsVisible = isVisible;

        SettingsManager.Current.General_ApplicationList.RemoveAt(index);

        SettingsManager.Current.General_ApplicationList.Insert(index, application);
    }

    #endregion
}