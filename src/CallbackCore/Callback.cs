using System.Runtime.CompilerServices;

namespace CallbackCore;

public class Callback : ICallback<Callback>
{
    public async Task InvokeAsync(CancellationToken ct = default)
    {
        Task[] tasks;
        lock (_lock)
        {
            var actionTasks = _actions.Select(Task.Run);
            var funcTasks = _funcs.Select(f => f());
            var cancelableFuncTasks = _cancelableFuncs.Select(f => f(ct));

            tasks = actionTasks
                .Concat(funcTasks)
                .Concat(cancelableFuncTasks)
                .ToArray();
        }

        await Task.WhenAll(tasks).WaitAsync(ct);
    }

    public void Invoke(CancellationToken ct = default)
        => InvokeAsync(ct).ConfigureAwait(false).GetAwaiter().GetResult();

    #region Constructors & Fields & Properties
    private readonly List<Action> _actions = [];
    private readonly List<Func<Task>> _funcs = [];
    private readonly List<Func<CancellationToken, Task>> _cancelableFuncs = [];

#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    public Callback() { }
    public Callback(Action action) => _actions.Add(action);
    public Callback(Func<Task> func) => _funcs.Add(func);
    public Callback(Func<CancellationToken, Task> func) => _cancelableFuncs.Add(func);
    #endregion

    #region Operators
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Action action)
    {
        lock (_lock)
            _actions.Add(action);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<Task> func)
    {
        lock (_lock)
            _funcs.Add(func);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<CancellationToken, Task> func)
    {
        lock (_lock)
            _cancelableFuncs.Add(func);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Action action)
    {
        lock (_lock)
            _actions.Remove(action);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<Task> func)
    {
        lock (_lock)
            _funcs.Remove(func);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<CancellationToken, Task> func)
    {
        lock (_lock)
            _cancelableFuncs.Remove(func);
    }

    public static Callback operator +(Callback? left, Action right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }
    public static Callback operator +(Callback? left, Func<Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }
    public static Callback operator +(Callback? left, Func<CancellationToken, Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }
    public static Callback operator -(Callback left, Action right)
    {
        left.Remove(right);
        return left;
    }
    public static Callback operator -(Callback left, Func<Task> right)
    {
        left.Remove(right);
        return left;
    }
    public static Callback operator -(Callback left, Func<CancellationToken, Task> right)
    {
        left.Remove(right);
        return left;
    }
    #endregion

    #region Implicit Conversions
    public static implicit operator Callback(Action action) => new(action);
    public static implicit operator Callback(Func<Task> func) => new(func);
    public static implicit operator Callback(Func<CancellationToken, Task> func) => new(func);
    #endregion
}
