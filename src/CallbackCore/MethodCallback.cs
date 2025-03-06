namespace CallbackCore;

public readonly struct MethodCallback
{
    public async Task InvokeAsync(CancellationToken ct = default)
    {
        switch (@delegate)
        {
            case Action action:
                await Task.Run(action, ct).WaitAsync(ct);
                break;
            case Func<Task> func:
                await Task.Run(func, ct).WaitAsync(ct);
                break;
            case Func<CancellationToken, Task> cancelableFunc:
                await cancelableFunc(ct);
                break;
        }
    }

    public void Invoke(CancellationToken ct = default) // Can not cancel sync action, suggestions are welcome
    {
        switch (@delegate)
        {
            case Action action:
                action();
                break;
            case Func<Task> func:
                Task.Run(func, ct).GetAwaiter().GetResult();
                break;
            case Func<CancellationToken, Task> cancelableFunc:
                cancelableFunc(ct).GetAwaiter().GetResult();
                break;
        }
    }

    private readonly Delegate @delegate;

    public MethodCallback(Action action) => @delegate = action;
    public MethodCallback(Func<Task> func) => @delegate = func;
    public MethodCallback(Func<CancellationToken, Task> func) => @delegate = func;

    public static implicit operator MethodCallback(Action action) => new(action);
    public static implicit operator MethodCallback(Func<Task> func) => new(func);
    public static implicit operator MethodCallback(Func<CancellationToken, Task> func) => new(func);
}
