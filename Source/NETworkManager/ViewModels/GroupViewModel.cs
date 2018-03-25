using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class GroupViewModel : ViewModelBase
    {
        bool _isLoading = true;

        private readonly ICommand _okCommand;
        public ICommand OKCommand
        {
            get { return _okCommand; }
        }

        private readonly ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        private string _oldGroup;
        public string OldGroup
        {
            get { return _oldGroup; }
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
            get { return _group; }
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
            get { return _groupHasChanged; }
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
            _okCommand = new RelayCommand(p => okCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            OldGroup = group;
            Group = group;

            _isLoading = false;
        }
    }
}