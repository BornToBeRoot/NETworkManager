using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Settings;

namespace NETworkManager.ViewModels
{
    public class PortProfilesViewModel : ViewModelBase
    {
        private readonly bool _isLoading;

        public ICommand OKCommand { get; }

        public ICommand CancelCommand { get; }


        private string _search;
        public string Search
        {
            get => _search;
            set
            {
                if (value == _search)
                    return;

                _search = value;

                PortProfiles.Refresh();

                OnPropertyChanged();
            }
        }

        public ICollectionView PortProfiles { get; }

        private PortProfileInfo _selectedPortProfile = new PortProfileInfo();
        public PortProfileInfo SelectedPortProfile
        {
            get => _selectedPortProfile;
            set
            {
                if (value == _selectedPortProfile)
                    return;

                _selectedPortProfile = value;
                OnPropertyChanged();
            }
        }

        public PortProfilesViewModel(Action<PortProfilesViewModel> okCommand, Action<PortProfilesViewModel> cancelHandler)
        {
            _isLoading = true;

            OKCommand = new RelayCommand(p => okCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            PortProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortProfiles);
            PortProfiles.SortDescriptions.Add(new SortDescription(nameof(PortProfileInfo.Name), ListSortDirection.Ascending));
            PortProfiles.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                if (!(o is PortProfileInfo info))
                    return false;

                var search = Search.Trim();

                // Search: Name, Ports
                return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.Ports.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
            };

            _isLoading = false;
        }
    }
}