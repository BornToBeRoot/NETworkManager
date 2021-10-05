using NETworkManager.Profiles;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class GroupViewModel : ViewModelBase
    {
        private readonly bool _isLoading = true;

        public bool IsProfileFileEncrypted => ProfileManager.LoadedProfileFile.IsEncrypted;

        public ICollectionView GroupViews { get; }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        private GroupInfo _group;
        public GroupInfo Group
        {
            get => _group;
            set
            {
                if (value == _group)
                    return;

                _group = value;
                OnPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (value == _name)
                    return;

                // Check name for duplicate...

                _name = value;
                OnPropertyChanged();
            }
        }

        private List<string> _groups { get; }
        
        public GroupViewModel(Action<GroupViewModel> saveCommand, Action<GroupViewModel> cancelHandler, GroupInfo group, List<string> groups)
        {
            // Load the view
            GroupViews = new CollectionViewSource { Source = GroupViewManager.List }.View;
            GroupViews.SortDescriptions.Add(new SortDescription(nameof(GroupViewInfo.Name), ListSortDirection.Ascending));

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            Group = group;
            _groups = groups;

            Name = Group.Name;

            _isLoading = false;
        }
    }
}