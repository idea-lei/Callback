namespace CallbackCore;

public readonly struct MethodCallback<TArg>
{
    public async Task InvokeAsync(TArg arg, CancellationToken ct = default)
    {
        switch (@delegate)
        {
            case Action<TArg> action:
                await Task.Run(() => action(arg), ct).WaitAsync(ct);
                break;
            case Func<TArg, Task> func:
                await Task.Run(() => func(arg), ct).WaitAsync(ct);
                break;
            case Func<TArg, CancellationToken, Task> cancelableFunc:
                await cancelableFunc(arg, ct);
                break;
        }
    }

    public void Invoke(TArg arg)
    {
        switch (@delegate)
        {
            case Action<TArg> action:
                action(arg);
                break;
            case Func<TArg, Task> func:
                Task.Run(() => func(arg)).GetAwaiter().GetResult();
                break;
            case Func<TArg, CancellationToken, Task> cancelableFunc:
                cancelableFunc(arg, CancellationToken.None).GetAwaiter().GetResult();
                break;
        }
    }

    private readonly Delegate? @delegate;

    public MethodCallback(Action<TArg> action) => @delegate = action;
    public MethodCallback(Func<TArg, Task> func) => @delegate = func;
    public MethodCallback(Func<TArg, CancellationToken, Task> func) => @delegate = func;

    public static implicit operator MethodCallback<TArg>(Action<TArg> action) => new(action);
    public static implicit operator MethodCallback<TArg>(Func<TArg, Task> func) => new(func);
    public static implicit operator MethodCallback<TArg>(Func<TArg, CancellationToken, Task> func) => new(func);
}
