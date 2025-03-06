namespace CallbackCore;

public readonly struct MethodCallback<TArg1, TArg2>
{
    public async Task InvokeAsync(TArg1 arg1, TArg2 arg2, CancellationToken ct = default)
    {
        switch (@delegate)
        {
            case Action<TArg1, TArg2> action:
                await Task.Run(() => action(arg1, arg2), ct).WaitAsync(ct);
                break;
            case Func<TArg1, TArg2, Task> func:
                await Task.Run(() => func(arg1, arg2), ct).WaitAsync(ct);
                break;
            case Func<TArg1, TArg2, CancellationToken, Task> cancelableFunc:
                await cancelableFunc(arg1, arg2, ct);
                break;
        }
    }

    public void Invoke(TArg1 arg1, TArg2 arg2, CancellationToken ct = default)
    {
        switch (@delegate)
        {
            case Action<TArg1, TArg2> action:
                action(arg1, arg2);
                break;
            case Func<TArg1, TArg2, Task> func:
                Task.Run(() => func(arg1, arg2), ct).GetAwaiter().GetResult();
                break;
            case Func<TArg1, TArg2, CancellationToken, Task> cancelableFunc:
                cancelableFunc(arg1, arg2, ct).GetAwaiter().GetResult();
                break;
        }
    }

    private readonly Delegate @delegate;

    public MethodCallback(Action<TArg1, TArg2> action) => @delegate = action;
    public MethodCallback(Func<TArg1, TArg2, Task> func) => @delegate = func;
    public MethodCallback(Func<TArg1, TArg2, CancellationToken, Task> func) => @delegate = func;

    public static implicit operator MethodCallback<TArg1, TArg2>(Action<TArg1, TArg2> action) => new(action);
    public static implicit operator MethodCallback<TArg1, TArg2>(Func<TArg1, TArg2, Task> func) => new(func);
    public static implicit operator MethodCallback<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> func) => new(func);
}