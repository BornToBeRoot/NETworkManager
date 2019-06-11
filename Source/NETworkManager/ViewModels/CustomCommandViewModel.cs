using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class CustomCommandViewModel : ViewModelBase
    {
        private readonly bool _isLoading;

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _name = value;

                if (!_isLoading)
                    CheckInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath == value)
                    return;

                _filePath = value;

                if (!_isLoading)
                    CheckInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _arguments;
        public string Arguments
        {
            get => _arguments;
            set
            {
                if (_arguments == value)
                    return;

                _arguments = value;

                if (!_isLoading)
                    CheckInfoChanged();

                OnPropertyChanged();
            }
        }

        private readonly CustomCommandInfo _info;


        private bool _infoChanged;
        public bool InfoChanged
        {
            get => _infoChanged;
            set
            {
                if (value == _infoChanged)
                    return;

                _infoChanged = value;
                OnPropertyChanged();
            }
        }

        private bool _isEdited;
        public bool IsEdited
        {
            get => _isEdited;
            set
            {
                if (value == _isEdited)
                    return;

                _isEdited = value;
                OnPropertyChanged();
            }
        }

        public CustomCommandViewModel(Action<CustomCommandViewModel> saveCommand, Action<CustomCommandViewModel> cancelHandler, bool isEdited = false, CustomCommandInfo info = null)
        {
            _isLoading = true;

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            _isEdited = isEdited;

            _info = info ?? new CustomCommandInfo();

            Name = _info.Name;
            FilePath = _info.FilePath;
            Arguments = _info.Arguments;

            _isLoading = false;
        }

        public void CheckInfoChanged() => InfoChanged = _info.Name != null || _info.FilePath != FilePath || _info.Arguments != Arguments;
    }
}