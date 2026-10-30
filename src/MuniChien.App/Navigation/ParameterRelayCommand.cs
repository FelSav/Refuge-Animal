using System.Windows.Input;

namespace MuniChien.App.Navigation;

public sealed class ParameterRelayCommand<T> : ICommand where T : class
{
    private readonly Action<T> _execute;
    public ParameterRelayCommand(Action<T> execute) => _execute = execute;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => parameter is T;
    public void Execute(object? parameter)
    {
        if (parameter is T value) _execute(value);
    }
}
