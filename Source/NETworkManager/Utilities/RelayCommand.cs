using System;
using System.Diagnostics;
using System.Windows.Input;

// Source: https://msdn.microsoft.com/en-us/magazine/dd419663.aspx?f=255&MSPPError=-2147217396#id0090030

namespace NETworkManager.Utilities
{
    public class RelayCommand : ICommand
    {
        #region Fields 

        private readonly Action<object> _execute;

        private readonly Predicate<object> _canExecute;
        #endregion

        #region Constructors 
        public RelayCommand(Action<object> execute) : this(execute, null)
        {

        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute)); _canExecute = canExecute;
        }
        #endregion

        #region ICommand Members 
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion
    }
}
