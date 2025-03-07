using System.Runtime.CompilerServices;

namespace CallbackCore;

public class Callback<TArg> : ICallback<Callback<TArg>, TArg>
{
    public async Task InvokeAsync(TArg arg, CancellationToken ct = default)
    {
        Task[] tasks;
        lock (_lock)
        {
            var actionTasks = _actions.Select(a => Task.Run(() => a(arg)));
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

    #region Fields & Constructors
    private readonly List<Action<TArg>> _actions = [];
    private readonly List<Func<TArg, Task>> _funcs = [];
    private readonly List<Func<TArg, CancellationToken, Task>> _cancelableFuncs = [];

#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    public Callback() { }
    public Callback(Action<TArg> action) => _actions.Add(action);
    public Callback(Func<TArg, Task> func) => _funcs.Add(func);
    public Callback(Func<TArg, CancellationToken, Task> func) => _cancelableFuncs.Add(func);
    #endregion

    #region Operators
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Action<TArg> action)
    {
        lock (_lock)
            _actions.Add(action);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<TArg, Task> func)
    {
        lock (_lock)
            _funcs.Add(func);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<TArg, CancellationToken, Task> func)
    {
        lock (_lock)
            _cancelableFuncs.Add(func);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Action<TArg> action)
    {
        lock (_lock)
            _actions.Remove(action);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<TArg, Task> func)
    {
        lock (_lock)
            _funcs.Remove(func);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<TArg, CancellationToken, Task> func)
    {
        lock (_lock)
            _cancelableFuncs.Remove(func);
    }

    public static Callback<TArg> operator +(Callback<TArg>? left, Action<TArg> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static Callback<TArg> operator +(Callback<TArg>? left, Func<TArg, Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static Callback<TArg> operator +(Callback<TArg>? left, Func<TArg, CancellationToken, Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static Callback<TArg> operator -(Callback<TArg> left, Action<TArg> right)
    {
        left.Remove(right);
        return left;
    }

    public static Callback<TArg> operator -(Callback<TArg> left, Func<TArg, Task> right)
    {
        left.Remove(right);
        return left;
    }

    public static Callback<TArg> operator -(Callback<TArg> left, Func<TArg, CancellationToken, Task> right)
    {
        left.Remove(right);
        return left;
    }
    #endregion

    #region Conversions
    public static implicit operator Callback<TArg>(Action<TArg> action) => new(action);
    public static implicit operator Callback<TArg>(Func<TArg, Task> func) => new(func);
    public static implicit operator Callback<TArg>(Func<TArg, CancellationToken, Task> func) => new(func);
    #endregion
}