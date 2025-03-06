namespace Callback;

public class EventCallback<TArg>
{
    public async Task InvokeAsync(TArg arg, CancellationToken ct = default)
    {
        Task[] tasks;
        lock (_lock)
        {
            var actionTasks = _actions.Select(a => Task.Run(() => a(arg), ct));
            var funcTasks = _funcs.Select(f => f(arg));
            var cancelableFuncTasks = _cancelableFuncs.Select(f => f(arg, ct));

            tasks = actionTasks
                .Concat(funcTasks)
                .Concat(cancelableFuncTasks)
                .ToArray();
        }

        await Task.WhenAll(tasks).WaitAsync(ct);
    }

    public void Invoke(TArg arg, CancellationToken ct = default)
        => InvokeAsync(arg, ct).ConfigureAwait(false).GetAwaiter().GetResult();

    #region Constructors & Fields & Properties
    private readonly List<Action<TArg>> _actions = new();
    private readonly List<Func<TArg, Task>> _funcs = new();
    private readonly List<Func<TArg, CancellationToken, Task>> _cancelableFuncs = new();

#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    public EventCallback(Action<TArg> action) => _actions.Add(action);
    public EventCallback(Func<TArg, Task> func) => _funcs.Add(func);
    public EventCallback(Func<TArg, CancellationToken, Task> func) => _cancelableFuncs.Add(func);
    #endregion

    #region Operators
    public static EventCallback<TArg> operator +(EventCallback<TArg>? left, Action<TArg> right)
    {
        if (left == null)
            return new EventCallback<TArg>(right);

        lock (left._lock) 
            left._actions.Add(right);

        return left;
    }
    public static EventCallback<TArg> operator +(EventCallback<TArg>? left, Func<TArg, Task> right)
    {
        if (left == null)
            return new EventCallback<TArg>(right);

        lock (left._lock)
            left._funcs.Add(right);

        return left;
    }
    public static EventCallback<TArg> operator +(EventCallback<TArg>? left, Func<TArg, CancellationToken, Task> right)
    {
        if (left == null)
            return new EventCallback<TArg>(right);

        lock (left._lock)
            left._cancelableFuncs.Add(right);

        return left;
    }
    public static EventCallback<TArg> operator -(EventCallback<TArg> left, Action<TArg> right)
    {
        lock (left._lock)
            left._actions.Remove(right);

        return left;
    }
    public static EventCallback<TArg> operator -(EventCallback<TArg> left, Func<TArg, Task> right)
    {
        lock (left._lock)
            left._funcs.Remove(right);

        return left;
    }
    public static EventCallback<TArg> operator -(EventCallback<TArg> left, Func<TArg, CancellationToken, Task> right)
    {
        lock (left._lock)
            left._cancelableFuncs.Remove(right);

        return left;
    }
    #endregion

    #region Implicit Conversions
    public static implicit operator EventCallback<TArg>(Action<TArg> action) => new(action);
    public static implicit operator EventCallback<TArg>(Func<TArg, Task> func) => new(func);
    public static implicit operator EventCallback<TArg>(Func<TArg, CancellationToken, Task> func) => new(func);
    #endregion
}
