using NETworkManager.Profiles;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class GroupViewModel : ViewModelBase
    {
        private readonly bool _isLoading = true;

        public bool IsProfileFileEncrypted => ProfileManager.LoadedProfileFile.IsEncrypted;

        public ICommand OKCommand { get; }

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
        
        public GroupViewModel(Action<GroupViewModel> okCommand, Action<GroupViewModel> cancelHandler, GroupInfo group, List<string> groups)
        {
            OKCommand = new RelayCommand(p => okCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            Group = group;
            _groups = groups;

            Name = Group.Name;

            _isLoading = false;
        }
    }
}