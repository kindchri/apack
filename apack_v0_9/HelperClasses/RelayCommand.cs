using System;
using System.Windows.Input;
using System.Diagnostics;

namespace apack_v0_9.HelperClasses
{
    public class RelayCommand : ICommand
    {
        #region Members

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        private event EventHandler CanExecuteChangedInternal;
        #endregion
        #region Constructors
        public RelayCommand(Action<object> execute) : this(execute, null){}

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException(nameof(canExecute));
            }

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion

        #region ICommand Members
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this.CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this.CanExecuteChangedInternal -= value;
            }
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return this._canExecute != null && this._canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this._execute(parameter);
        }

        public void OnCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChangedInternal;
            handler?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
