using System;
using System.Windows.Input;

// Source: http://www.codeproject.com/Tips/813345/Basic-MVVM-and-ICommand-Usage-Example

namespace NETworkManager.ViewModels
{
    public class RelayCommand : ICommand
    {
        #region Fields

        /// <summary>
        /// Encapsulated the execute action
        /// </summary>
        private Action<object> execute;

        /// <summary>
        /// Encapsulated the representation for the validation of the execute method
        /// </summary>
        private Predicate<object> canExecute;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the RelayCommand class
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute) : this(execute, DefaultCanExecute)
        {

        }

        /// <summary>
        /// Initializes a new instance of the RelayCommand class
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        /// <summary>
        /// An event to raise when the CanExecute value is changed
        /// </summary>
        /// <remarks>
        /// Any subscription to this event will automatically subscribe to both 
        /// the local OnCanExecuteChanged method AND
        /// the CommandManager RequerySuggested event
        /// </remarks>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }

        /// <summary>
        /// An event to allow the CanExecuteChanged event to be raised manually
        /// </summary>
        private event EventHandler CanExecuteChangedInternal;

        /// <summary>
        /// Defines if command can be executed
        /// </summary>
        /// <param name="parameter">the parameter that represents the validation method</param>
        /// <returns>true if the command can be executed</returns>
        public bool CanExecute(object parameter)
        {
            return canExecute != null && canExecute(parameter);
        }

        /// <summary>
        /// Execute the encapsulated command
        /// </summary>
        /// <param name="parameter">the parameter that represents the execution method</param>
        public void Execute(object parameter)
        {
            execute(parameter);
        }

        #endregion // ICommand Members

        /// <summary>
        /// Raises the can execute changed.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChangedInternal;
            if (handler != null)
            {
                //DispatcherHelper.BeginInvokeOnUIThread(() => handler.Invoke(this, EventArgs.Empty));
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        public void Destroy()
        {
            canExecute = _ => false;
            execute = _ => { return; };
        }

        /// <summary>
        /// Defines if command can be executed (default behaviour)
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>Always true</returns>
        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }
    }
}
