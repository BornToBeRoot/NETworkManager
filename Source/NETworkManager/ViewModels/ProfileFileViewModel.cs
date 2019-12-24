using NETworkManager.Models.Profile;
using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class ProfileFileViewModel : ViewModelBase
    {
        private readonly bool _isLoading;

        public ICommand AcceptCommand { get; }

        public ICommand CancelCommand { get; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (value == _name)
                    return;

                _name = value;
                OnPropertyChanged();
            }
        }
            
        private bool _isEdit;
        public bool IsEdit
        {
            get => _isEdit;
            set
            {
                if (value == _isEdit)
                    return;

                _isEdit = value;
                OnPropertyChanged();
            }
        }

        public ProfileFileViewModel(Action<ProfileFileViewModel> addCommand, Action<ProfileFileViewModel> cancelHandler, ProfileFileInfo info = null)
        {
            _isLoading = true;

            AcceptCommand = new RelayCommand(p => addCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            if (info != null)
            {
                Name = info.Name;
                
                IsEdit = true;
            }

            _isLoading = false;
        }
    }
}