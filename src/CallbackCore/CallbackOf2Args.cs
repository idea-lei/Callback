using System.Runtime.CompilerServices;

namespace CallbackCore;

public class Callback<TArg1, TArg2> : ICallback<Callback<TArg1, TArg2>, TArg1, TArg2>
{
    public async Task InvokeAsync(TArg1 arg1, TArg2 arg2, CancellationToken ct = default)
    {
        Task[] tasks;
        lock (_lock)
        {
            var actionTasks = _actions.Select(a => Task.Run(() => a(arg1, arg2)));
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

    #region Fields & Constructors
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Action<TArg1, TArg2> action)
    {
        lock (_lock)
            _actions.Add(action);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<TArg1, TArg2, Task> func)
    {
        lock (_lock)
            _funcs.Add(func);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<TArg1, TArg2, CancellationToken, Task> func)
    {
        lock (_lock)
            _cancelableFuncs.Add(func);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Action<TArg1, TArg2> action)
    {
        lock (_lock)
            _actions.Remove(action);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<TArg1, TArg2, Task> func)
    {
        lock (_lock)
            _funcs.Remove(func);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<TArg1, TArg2, CancellationToken, Task> func)
    {
        lock (_lock)
            _cancelableFuncs.Remove(func);
    }

    public static Callback<TArg1, TArg2> operator +(Callback<TArg1, TArg2>? left, Action<TArg1, TArg2> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static Callback<TArg1, TArg2> operator +(Callback<TArg1, TArg2>? left, Func<TArg1, TArg2, Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static Callback<TArg1, TArg2> operator +(Callback<TArg1, TArg2>? left, Func<TArg1, TArg2, CancellationToken, Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static Callback<TArg1, TArg2> operator -(Callback<TArg1, TArg2> left, Action<TArg1, TArg2> right)
    {
        left.Remove(right);
        return left;
    }

    public static Callback<TArg1, TArg2> operator -(Callback<TArg1, TArg2> left, Func<TArg1, TArg2, Task> right)
    {
        left.Remove(right);
        return left;
    }

    public static Callback<TArg1, TArg2> operator -(Callback<TArg1, TArg2> left, Func<TArg1, TArg2, CancellationToken, Task> right)
    {
        left.Remove(right);
        return left;
    }
    #endregion

    #region Conversions
    public static implicit operator Callback<TArg1, TArg2>(Action<TArg1, TArg2> action) => new(action);
    public static implicit operator Callback<TArg1, TArg2>(Func<TArg1, TArg2, Task> func) => new(func);
    public static implicit operator Callback<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> func) => new(func);
    #endregion
}