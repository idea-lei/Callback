namespace CallbackCore;

public class Callback<TArg1, TArg2>
{
    public async Task InvokeAsync(TArg1 arg1, TArg2 arg2, CancellationToken ct = default)
    {
        Task[] tasks;
        lock (_lock)
        {
            var actionTasks = _actions.Select(a => Task.Run(() => a(arg1, arg2), ct));
            var funcTasks = _funcs.Select(f => f(arg1, arg2));
            var cancelableFuncTasks = _cancelableFuncs.Select(f => f(arg1, arg2, ct));

            tasks = actionTasks
                .Concat(funcTasks)
                .Concat(cancelableFuncTasks)
                .ToArray();
        }

        await Task.WhenAll(tasks).WaitAsync(ct);
    }

    public void Invoke(TArg1 arg1, TArg2 arg2, CancellationToken ct = default)
        => InvokeAsync(arg1, arg2, ct).ConfigureAwait(false).GetAwaiter().GetResult();

    #region Constructors & Fields & Properties
    private readonly List<Action<TArg1, TArg2>> _actions = [];
    private readonly List<Func<TArg1, TArg2, Task>> _funcs = [];
    private readonly List<Func<TArg1, TArg2, CancellationToken, Task>> _cancelableFuncs = [];

#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    public Callback() { }
    public Callback(Action<TArg1, TArg2> action) => _actions.Add(action);
    public Callback(Func<TArg1, TArg2, Task> func) => _funcs.Add(func);
    public Callback(Func<TArg1, TArg2, CancellationToken, Task> func) => _cancelableFuncs.Add(func);
    #endregion

    #region Operators
    public static Callback<TArg1, TArg2> operator +(Callback<TArg1, TArg2>? left, Action<TArg1, TArg2> right)
    {
        if (left == null)
            return new Callback<TArg1, TArg2>(right);

        lock (left._lock)
            left._actions.Add(right);

        return left;
    }

    public static Callback<TArg1, TArg2> operator +(Callback<TArg1, TArg2>? left, Func<TArg1, TArg2, Task> right)
    {
        if (left == null)
            return new Callback<TArg1, TArg2>(right);

        lock (left._lock)
            left._funcs.Add(right);

        return left;
    }

    public static Callback<TArg1, TArg2> operator +(Callback<TArg1, TArg2>? left, Func<TArg1, TArg2, CancellationToken, Task> right)
    {
        if (left == null)
            return new Callback<TArg1, TArg2>(right);

        lock (left._lock)
            left._cancelableFuncs.Add(right);

        return left;
    }

    public static Callback<TArg1, TArg2> operator -(Callback<TArg1, TArg2> left, Action<TArg1, TArg2> right)
    {
        lock (left._lock)
            left._actions.Remove(right);

        return left;
    }

    public static Callback<TArg1, TArg2> operator -(Callback<TArg1, TArg2> left, Func<TArg1, TArg2, Task> right)
    {
        lock (left._lock)
            left._funcs.Remove(right);

        return left;
    }

    public static Callback<TArg1, TArg2> operator -(Callback<TArg1, TArg2> left, Func<TArg1, TArg2, CancellationToken, Task> right)
    {
        lock (left._lock)
            left._cancelableFuncs.Remove(right);

        return left;
    }
    #endregion

    #region Implicit Conversions
    public static implicit operator Callback<TArg1, TArg2>(Action<TArg1, TArg2> action) => new(action);
    public static implicit operator Callback<TArg1, TArg2>(Func<TArg1, TArg2, Task> func) => new(func);
    public static implicit operator Callback<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> func) => new(func);
    #endregion
}
