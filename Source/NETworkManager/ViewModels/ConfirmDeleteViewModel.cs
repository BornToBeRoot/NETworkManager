using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class ConfirmDeleteViewModel : ViewModelBase
    {
        public ICommand DeleteCommand { get; }

        public ICommand CancelCommand { get; }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                if (value == _message)
                    return;

                _message = value;
                OnPropertyChanged();
            }
        }
        
        public ConfirmDeleteViewModel(Action<ConfirmDeleteViewModel> deleteCommand, Action<ConfirmDeleteViewModel> cancelHandler, string message)
        {
            DeleteCommand = new RelayCommand(p => deleteCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            Message = message;
        }
    }
}
