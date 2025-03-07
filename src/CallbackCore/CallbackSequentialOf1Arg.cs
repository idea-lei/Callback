using System.Runtime.CompilerServices;

namespace CallbackCore;

public class CallbackSequential<TArg> : ICallback<CallbackSequential<TArg>, TArg>
{
    public async Task InvokeAsync(TArg arg, CancellationToken ct = default)
    {
        foreach (var handler in _handlers)
        {
            switch (handler)
            {
                case Action<TArg> action:
                    await Task.Run(() => action(arg), ct);
                    break;
                case Func<TArg, Task> func:
                    await Task.Run(() => func(arg), ct);
                    break;
                case Func<TArg, CancellationToken, Task> cancelableFunc:
                    await cancelableFunc(arg, ct);
                    break;
                default:
                    throw new NotImplementedException();
            }
            ct.ThrowIfCancellationRequested();
        }
    }

    public void Invoke(TArg arg, CancellationToken ct = default)
        => InvokeAsync(arg, ct).ConfigureAwait(false).GetAwaiter().GetResult();

    private readonly List<Delegate> _handlers = [];

    #region Constructors & Conversions
    public CallbackSequential() { }
    public CallbackSequential(Action<TArg> action) => _handlers.Add(action);
    public CallbackSequential(Func<TArg, Task> func) => _handlers.Add(func);
    public CallbackSequential(Func<TArg, CancellationToken, Task> func) => _handlers.Add(func);

    public static implicit operator CallbackSequential<TArg>(Action<TArg> action) => new(action);
    public static implicit operator CallbackSequential<TArg>(Func<TArg, Task> func) => new(func);
    public static implicit operator CallbackSequential<TArg>(Func<TArg, CancellationToken, Task> func) => new(func);
    #endregion

    #region Operators
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Action<TArg> action) => _handlers.Add(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<TArg, Task> func) => _handlers.Add(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<TArg, CancellationToken, Task> func) => _handlers.Add(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Action<TArg> action) => _handlers.Remove(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<TArg, Task> func) => _handlers.Remove(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<TArg, CancellationToken, Task> func) => _handlers.Remove(func);

    public static CallbackSequential<TArg> operator +(CallbackSequential<TArg>? left, Action<TArg> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static CallbackSequential<TArg> operator +(CallbackSequential<TArg>? left, Func<TArg, Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static CallbackSequential<TArg> operator +(CallbackSequential<TArg>? left, Func<TArg, CancellationToken, Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }

    public static CallbackSequential<TArg> operator -(CallbackSequential<TArg> left, Action<TArg> right)
    {
        left.Remove(right);
        return left;
    }

    public static CallbackSequential<TArg> operator -(CallbackSequential<TArg> left, Func<TArg, Task> right)
    {
        left.Remove(right);
        return left;
    }

    public static CallbackSequential<TArg> operator -(CallbackSequential<TArg> left, Func<TArg, CancellationToken, Task> right)
    {
        left.Remove(right);
        return left;
    }
    #endregion
}