using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using log4net;
using NETworkManager.Localization;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public sealed class ImportProfilesResultViewModel : ViewModelBase
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ImportProfilesResultViewModel));

    private readonly Action<int, int, int> _completedCallback;
    private readonly DispatcherTimer _searchDispatcherTimer = new();

    public ImportProfilesResultViewModel(IReadOnlyList<ProfileImportCandidate> candidates, ProfileImportSource importSource, string targetGroup, Action backCallback, Action cancelCallback, Action<int, int, int> completedCallback)
    {
        _completedCallback = completedCallback;

        Candidates = new ObservableCollection<ImportCandidateItem>(candidates.Select(c => new ImportCandidateItem(c)));

        foreach (var item in Candidates)
            item.PropertyChanged += OnCandidateChanged;

        if (importSource != ProfileImportSource.None)
        {
            var importedIds = new HashSet<string>(
                ProfileManager.LoadedProfileFileData.Groups
                    .SelectMany(g => g.Profiles)
                    .Where(p => p.ImportSource == importSource && !string.IsNullOrEmpty(p.ImportSourceId))
                    .Select(p => p.ImportSourceId),
                StringComparer.OrdinalIgnoreCase);

            foreach (var item in Candidates)
                item.AlreadyExists = !string.IsNullOrEmpty(item.ImportSourceId) && importedIds.Contains(item.ImportSourceId);
        }

        GroupNames = new ObservableCollection<string>(ProfileManager.GetGroupNames());
        GroupName = !string.IsNullOrWhiteSpace(targetGroup) ? targetGroup : GroupNames.FirstOrDefault() ?? string.Empty;

        CandidatesView = CollectionViewSource.GetDefaultView(Candidates);
        CandidatesView.Filter = FilterCandidate;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        ApplicationOptions = BuildApplicationOptions();
        foreach (var tool in ApplicationOptions)
            tool.PropertyChanged += OnApplicationOptionChanged;

        HasAnyCandidateSelected = Candidates.Any(c => c.IsSelected);
        HasAnyEnabledApplication = ApplicationOptions.Any(t => t.IsEnabled);

        SelectAllCommand = new RelayCommand(_ => SetAllSelections(true));
        DeselectAllCommand = new RelayCommand(_ => SetAllSelections(false));
        BackCommand = new RelayCommand(_ => backCallback());
        CancelCommand = new RelayCommand(_ => cancelCallback());
        ImportCommand = new RelayCommand(_ => ImportAction());
    }

    public string Search
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            IsSearching = true;
            _searchDispatcherTimer.Start();

            OnPropertyChanged();
        }
    } = string.Empty;

    public bool IsSearching
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<ImportCandidateItem> Candidates { get; }

    public ICollectionView CandidatesView { get; }

    public bool HasAnyCandidateSelected
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ImportApplicationToggleItem> ApplicationOptions { get; }

    public bool HasAnyEnabledApplication
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> GroupNames { get; }

    public string GroupName
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

    public bool SkipDuplicates
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsStatusMessageDisplayed
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ICommand SelectAllCommand { get; }

    public ICommand DeselectAllCommand { get; }

    public ICommand BackCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand ImportCommand { get; }

    private static ObservableCollection<ImportApplicationToggleItem> BuildApplicationOptions()
    {
        return
        [
            new ImportApplicationToggleItem(ProfileName.IPScanner),
            new ImportApplicationToggleItem(ProfileName.PortScanner),
            new ImportApplicationToggleItem(ProfileName.PingMonitor, true),
            new ImportApplicationToggleItem(ProfileName.Traceroute),
            new ImportApplicationToggleItem(ProfileName.DNSLookup),
            new ImportApplicationToggleItem(ProfileName.RemoteDesktop, true),
            new ImportApplicationToggleItem(ProfileName.PowerShell, true),
            new ImportApplicationToggleItem(ProfileName.PuTTY),
            new ImportApplicationToggleItem(ProfileName.TigerVNC),
            new ImportApplicationToggleItem(ProfileName.SNMP),
            new ImportApplicationToggleItem(ProfileName.Whois),
            new ImportApplicationToggleItem(ProfileName.IPGeolocation)
        ];
    }

    private static void ApplyToolDefaults(ProfileInfo profile, ProfileName tool)
    {
        switch (tool)
        {
            case ProfileName.IPScanner:
                profile.IPScanner_Enabled = true;
                profile.IPScanner_HostOrIPRange = profile.Host;
                break;
            case ProfileName.PortScanner:
                profile.PortScanner_Enabled = true;
                profile.PortScanner_Host = profile.Host;
                break;
            case ProfileName.PingMonitor:
                profile.PingMonitor_Enabled = true;
                profile.PingMonitor_Host = profile.Host;
                break;
            case ProfileName.Traceroute:
                profile.Traceroute_Enabled = true;
                profile.Traceroute_Host = profile.Host;
                break;
            case ProfileName.DNSLookup:
                profile.DNSLookup_Enabled = true;
                profile.DNSLookup_Host = profile.Host;
                break;
            case ProfileName.RemoteDesktop:
                profile.RemoteDesktop_Enabled = true;
                profile.RemoteDesktop_Host = profile.Host;
                break;
            case ProfileName.PowerShell:
                profile.PowerShell_Enabled = true;
                profile.PowerShell_Host = profile.Host;
                break;
            case ProfileName.PuTTY:
                profile.PuTTY_Enabled = true;
                profile.PuTTY_HostOrSerialLine = profile.Host;
                break;
            case ProfileName.TigerVNC:
                profile.TigerVNC_Enabled = true;
                profile.TigerVNC_Host = profile.Host;
                break;
            case ProfileName.SNMP:
                profile.SNMP_Enabled = true;
                profile.SNMP_Host = profile.Host;
                break;
            case ProfileName.Whois:
                profile.Whois_Enabled = true;
                profile.Whois_Domain = profile.Host;
                break;
            case ProfileName.IPGeolocation:
                profile.IPGeolocation_Enabled = true;
                profile.IPGeolocation_Host = profile.Host;
                break;
        }
    }

    private bool FilterCandidate(object obj)
    {
        if (obj is not ImportCandidateItem item)
            return false;

        if (string.IsNullOrEmpty(Search))
            return true;

        return item.Name?.Contains(Search, StringComparison.OrdinalIgnoreCase) == true ||
               item.Host?.Contains(Search, StringComparison.OrdinalIgnoreCase) == true;
    }

    private void SetAllSelections(bool isSelected)
    {
        foreach (var item in Candidates)
            item.IsSelected = isSelected;
    }

    private void OnCandidateChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImportCandidateItem.IsSelected))
            HasAnyCandidateSelected = Candidates.Any(c => c.IsSelected);
    }

    private void OnApplicationOptionChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImportApplicationToggleItem.IsEnabled))
            HasAnyEnabledApplication = ApplicationOptions.Any(t => t.IsEnabled);
    }

    private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
    {
        _searchDispatcherTimer.Stop();
        CandidatesView.Refresh();
        IsSearching = false;
    }

    private void ImportAction()
    {
        IsStatusMessageDisplayed = false;

        try
        {
            var targetGroup = GroupName.Trim();
            var enabledTools = ApplicationOptions.Where(t => t.IsEnabled).ToList();

            var imported = 0;
            var skippedDuplicates = 0;
            var skippedNoHost = 0;
            var profilesToAdd = new List<ProfileInfo>();

            foreach (var item in Candidates.Where(c => c.IsSelected))
            {
                if (!item.CanImport)
                {
                    skippedNoHost++;
                    continue;
                }

                if (SkipDuplicates && item.AlreadyExists)
                {
                    skippedDuplicates++;
                    continue;
                }

                var profile = new ProfileInfo
                {
                    Name = item.Name,
                    Host = item.Host.Trim(),
                    Description = item.Description,
                    Group = targetGroup,
                    TagsCollection = [],
                    ImportSource = item.ImportSource,
                    ImportSourceId = item.ImportSourceId
                };

                foreach (var tool in enabledTools)
                    ApplyToolDefaults(profile, tool.ProfileName);

                profilesToAdd.Add(profile);
                imported++;
            }

            if (profilesToAdd.Count > 0)
                ProfileManager.AddProfiles(profilesToAdd);

            _completedCallback(imported, skippedDuplicates, skippedNoHost);
        }
        catch (Exception exception)
        {
            Log.Error("Profile import failed.", exception);
            StatusMessage = exception.Message;
            IsStatusMessageDisplayed = true;
        }
    }
}

public sealed class ImportCandidateItem : ViewModelBase
{
    public ImportCandidateItem(ProfileImportCandidate source)
    {
        Name = source.Name;
        Host = source.Host;
        Description = source.Description;
        ImportSource = source.ImportSource;
        ImportSourceId = source.ImportSourceId;
        CanImport = !string.IsNullOrWhiteSpace(source.Host);
        IsSelected = CanImport;
    }

    public string Name { get; }

    public string Host { get; }

    public string Description { get; }

    public ProfileImportSource ImportSource { get; }

    public string ImportSourceId { get; }

    public bool CanImport { get; }

    public bool IsSelected
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

    public bool AlreadyExists { get; set; }
}

public sealed class ImportApplicationToggleItem : ViewModelBase
{
    public ImportApplicationToggleItem(ProfileName profileName, bool isEnabled = false)
    {
        ProfileName = profileName;
        DisplayName = ResourceTranslator.Translate(ResourceIdentifier.ApplicationName, profileName);
        IsEnabled = isEnabled;
    }

    public ProfileName ProfileName { get; }

    public string DisplayName { get; }

    public bool IsEnabled
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
}