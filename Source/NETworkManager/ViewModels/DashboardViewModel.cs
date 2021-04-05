using NETworkManager.Settings;
using System;
using System.ComponentModel;
using NETworkManager.Profiles;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Data;

namespace NETworkManager.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        #region  Variables 
        public ICollectionView Profiles { get; }
        private readonly DispatcherTimer _searchDispatcherTimer = new DispatcherTimer();

        private string _search;
        public string Search
        {
            get => _search;
            set
            {
                if (value == _search)
                    return;

                _search = value;

                StartDelayedSearch();

                OnPropertyChanged();
            }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                if (value == _isSearching)
                    return;

                _isSearching = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor, load settings

        public DashboardViewModel()
        {
            Profiles = new CollectionViewSource
            {
                Source = ProfileManager.Profiles
            }.View;
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            Profiles.Filter = o =>
            {
                if (!(o is ProfileInfo info))
                    return false;
                
                if (string.IsNullOrEmpty(Search))
                    return true;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                // Search by: Name, IPScanner_IPRange
                return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
            };

            _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
            _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

            LoadSettings();
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions

        #endregion

        #region Methods
        private void StartDelayedSearch()
        {
            if (!IsSearching)
            {
                IsSearching = true;

                _searchDispatcherTimer.Start();
            }
            else
            {
                _searchDispatcherTimer.Stop();
                _searchDispatcherTimer.Start();
            }
        }

        private void StopDelayedSearch()
        {
            _searchDispatcherTimer.Stop();

            RefreshProfiles();

            IsSearching = false;
        }

        public void OnViewVisible()
        {

        }

        public void OnViewHide()
        {

        }

        public void RefreshProfiles()
        {
            Profiles.Refresh();
        }
        #endregion

        #region Events
        private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
        {
            StopDelayedSearch();
        }
        #endregion
    }
}

