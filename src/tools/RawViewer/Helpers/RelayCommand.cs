using System.Diagnostics;
using System.Windows.Input;
using System;

namespace RawViewer.Helpers;

public class RelayCommand : ICommand
{
    #region Fields

    readonly Action<object> execute;
    readonly Predicate<object> canExecute;

    #endregion

    #region Constructors

    public RelayCommand(Action<object> execute)
        : this(execute, null)
    {
    }

    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
        this.execute = execute ?? throw new ArgumentNullException("execute");
        this.canExecute = canExecute;
    }

    #endregion

    #region ICommand Members

    [DebuggerStepThrough]
    public bool CanExecute(object parameter)
    {
        return canExecute == null ? true : canExecute(parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object parameter)
    {
        execute(parameter);
    }

    #endregion
}

