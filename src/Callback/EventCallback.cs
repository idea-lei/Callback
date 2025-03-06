﻿namespace Callback;

public class EventCallback
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
    private readonly List<Action> _actions = new();
    private readonly List<Func<Task>> _funcs = new();
    private readonly List<Func<CancellationToken, Task>> _cancelableFuncs = new();

#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    public EventCallback(Action action) => _actions.Add(action);
    public EventCallback(Func<Task> func) => _funcs.Add(func);
    public EventCallback(Func<CancellationToken, Task> func) => _cancelableFuncs.Add(func);
    #endregion

    #region Operators
    public static EventCallback operator +(EventCallback? left, Action right)
    {
        if (left == null)
            return new EventCallback(right);

        lock (left._lock)
            left._actions.Add(right);

        return left;
    }
    public static EventCallback operator +(EventCallback? left, Func<Task> right)
    {
        if (left == null)
            return new EventCallback(right);

        lock (left._lock)
            left._funcs.Add(right);

        return left;
    }
    public static EventCallback operator +(EventCallback? left, Func<CancellationToken, Task> right)
    {
        if (left == null)
            return new EventCallback(right);

        lock (left._lock)
            left._cancelableFuncs.Add(right);

        return left;
    }
    public static EventCallback operator -(EventCallback left, Action right)
    {
        lock (left._lock)
            left._actions.Remove(right);

        return left;
    }
    public static EventCallback operator -(EventCallback left, Func<Task> right)
    {
        lock (left._lock)
            left._funcs.Remove(right);

        return left;
    }
    public static EventCallback operator -(EventCallback left, Func<CancellationToken, Task> right)
    {
        lock (left._lock)
            left._cancelableFuncs.Remove(right);

        return left;
    }
    #endregion

    #region Implicit Conversions
    public static implicit operator EventCallback(Action action) => new(action);
    public static implicit operator EventCallback(Func<Task> func) => new(func);
    public static implicit operator EventCallback(Func<CancellationToken, Task> func) => new(func);
    #endregion
}
