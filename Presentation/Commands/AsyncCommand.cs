using System.Windows.Input;

namespace Biblia.Presentation.Commands;

public sealed class AsyncCommand(Func<Task> execute,Func<bool>? canExecute=null):ICommand
{
    private bool _running;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter)=>!_running&&(canExecute?.Invoke()??true);
    public async void Execute(object? parameter)
    {
        if(!CanExecute(parameter))return;
        _running=true;CanExecuteChanged?.Invoke(this,EventArgs.Empty);
        try{await execute();}
        finally{_running=false;CanExecuteChanged?.Invoke(this,EventArgs.Empty);}
    }
    public void NotifyCanExecuteChanged()=>CanExecuteChanged?.Invoke(this,EventArgs.Empty);
}

public sealed class AsyncCommand<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null) : ICommand
{
    private bool _running;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !_running && parameter is T value && (canExecute?.Invoke(value) ?? true);
    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter) || parameter is not T value) return;
        _running = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try { await execute(value); }
        finally { _running = false; CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
    }
    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
