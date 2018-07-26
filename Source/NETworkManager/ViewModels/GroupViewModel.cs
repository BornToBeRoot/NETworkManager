using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class GroupViewModel : ViewModelBase
    {
        private readonly bool _isLoading;

        public ICommand OKCommand { get; }

        public ICommand CancelCommand { get; }

        private string _oldGroup;
        public string OldGroup
        {
            get => _oldGroup;
            set
            {
                if (value == _oldGroup)
                    return;

                _oldGroup = value;
                OnPropertyChanged();
            }
        }

        private string _group;
        public string Group
        {
            get => _group;
            set
            {
                if (value == _group)
                    return;

                if (!_isLoading)
                    GroupHasChanged = !string.IsNullOrEmpty(value) && (value != OldGroup);

                _group = value;
                OnPropertyChanged();
            }
        }

        private bool _groupHasChanged;
        public bool GroupHasChanged
        {
            get => _groupHasChanged;
            set
            {
                if (value == _groupHasChanged)
                    return;

                _groupHasChanged = value;
                OnPropertyChanged();
            }
        }

        public GroupViewModel(Action<GroupViewModel> okCommand, Action<GroupViewModel> cancelHandler, string group)
        {
            _isLoading = true;

            OKCommand = new RelayCommand(p => okCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            OldGroup = group;
            Group = group;

            _isLoading = false;
        }
    }
}