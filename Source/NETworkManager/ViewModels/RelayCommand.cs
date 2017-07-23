using System;
using System.Diagnostics;
using System.Windows.Input;

// Source: https://msdn.microsoft.com/en-us/magazine/dd419663.aspx?f=255&MSPPError=-2147217396#id0090030

namespace NETworkManager.ViewModels
{
    public class RelayCommand : ICommand
    {
        #region Fields 
        readonly Action<object> _execute;

        readonly Predicate<object> _canExecute;
        #endregion

        #region Constructors 
        public RelayCommand(Action<object> execute) : this(execute, null)
        {

        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute"); _canExecute = canExecute;
        }
        #endregion

        #region ICommand Members 
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion
    }
}
