using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HR.Utilities
{
    /// <summary>
    /// A command that relays its functionality to specified delegates.
    /// Implements the <see cref="ICommand"/> interface to enable command binding in WPF MVVM patterns.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked. Cannot be null.</param>
        /// <param name="canExecute">A predicate to determine whether the command can execute. Optional.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="execute"/> is null.</exception>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. Can be null.</param>
        /// <returns>True if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. Can be null.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // Private field to hold event subscribers for CanExecuteChanged.
        private EventHandler _canExecuteChanged;
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// Subscribes also to CommandManager.RequerySuggested to automatically update command state.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                _canExecuteChanged += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                _canExecuteChanged -= value;
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to indicate that the return value of <see cref="CanExecute"/> may have changed.
        /// Call this method to force WPF to re-query the command state and update UI elements bound to this command.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
