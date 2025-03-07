using System.Runtime.CompilerServices;

namespace CallbackCore;

public class CallbackSequential<TArg1, TArg2> : ICallback<CallbackSequential<TArg1, TArg2>, TArg1, TArg2>
{
    public async Task InvokeAsync(TArg1 arg1, TArg2 arg2, CancellationToken ct = default)
    {
        foreach (var handler in _handlers)
        {
            switch (handler)
            {
                case Action<TArg1, TArg2> action:
                    await Task.Run(() => action(arg1, arg2), ct);
                    break;
                case Func<TArg1, TArg2, Task> func:
                    await Task.Run(() => func(arg1, arg2), ct);
                    break;
                case Func<TArg1, TArg2, CancellationToken, Task> cancelableFunc:
                    await cancelableFunc(arg1, arg2, ct);
                    break;
                default:
                    throw new NotImplementedException();
            }
            ct.ThrowIfCancellationRequested();
        }
    }

    public void Invoke(TArg1 arg1, TArg2 arg2, CancellationToken ct = default)
        => InvokeAsync(arg1, arg2, ct).ConfigureAwait(false).GetAwaiter().GetResult();

    private readonly List<Delegate> _handlers = [];

    #region Constructors & Conversions
    public CallbackSequential() { }
    public CallbackSequential(Action<TArg1, TArg2> action) => _handlers.Add(action);
    public CallbackSequential(Func<TArg1, TArg2, Task> func) => _handlers.Add(func);
    public CallbackSequential(Func<TArg1, TArg2, CancellationToken, Task> func) => _handlers.Add(func);

    public static implicit operator CallbackSequential<TArg1, TArg2>(Action<TArg1, TArg2> action) => new(action);
    public static implicit operator CallbackSequential<TArg1, TArg2>(Func<TArg1, TArg2, Task> func) => new(func);
    public static implicit operator CallbackSequential<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> func) => new(func);
    #endregion

    #region Operators
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Action<TArg1, TArg2> action) => _handlers.Add(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<TArg1, TArg2, Task> func) => _handlers.Add(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<TArg1, TArg2, CancellationToken, Task> func) => _handlers.Add(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Action<TArg1, TArg2> action) => _handlers.Remove(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<TArg1, TArg2, Task> func) => _handlers.Remove(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<TArg1, TArg2, CancellationToken, Task> func) => _handlers.Remove(func);

    public static CallbackSequential<TArg1, TArg2> operator +(CallbackSequential<TArg1, TArg2>? left, Action<TArg1, TArg2> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static CallbackSequential<TArg1, TArg2> operator +(CallbackSequential<TArg1, TArg2>? left, Func<TArg1, TArg2, Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static CallbackSequential<TArg1, TArg2> operator +(CallbackSequential<TArg1, TArg2>? left, Func<TArg1, TArg2, CancellationToken, Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static CallbackSequential<TArg1, TArg2> operator -(CallbackSequential<TArg1, TArg2> left, Action<TArg1, TArg2> right)
    {
        left.Remove(right);
        return left;
    }

    public static CallbackSequential<TArg1, TArg2> operator -(CallbackSequential<TArg1, TArg2> left, Func<TArg1, TArg2, Task> right)
    {
        left.Remove(right);
        return left;
    }

    public static CallbackSequential<TArg1, TArg2> operator -(CallbackSequential<TArg1, TArg2> left, Func<TArg1, TArg2, CancellationToken, Task> right)
    {
        left.Remove(right);
        return left;
    }
    #endregion
}