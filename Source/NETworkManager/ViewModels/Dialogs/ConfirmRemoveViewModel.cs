using NETworkManager.Utils;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Dialogs
{
    public class ConfirmRemoveViewModel : ViewModelBase
    {
        private readonly ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get { return _deleteCommand; }
        }

        private readonly ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                if (value == _message)
                    return;

                _message = value;
                OnPropertyChanged();
            }
        }
        
        public ConfirmRemoveViewModel(Action<ConfirmRemoveViewModel> deleteCommand, Action<ConfirmRemoveViewModel> cancelHandler, string message)
        {
            _deleteCommand = new RelayCommand(p => deleteCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
            Message = message;
        }
    }
}
