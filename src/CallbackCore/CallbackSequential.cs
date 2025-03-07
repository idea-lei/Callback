using System.Runtime.CompilerServices;

namespace CallbackCore;

public class CallbackSequential : ICallback<CallbackSequential>
{
    public async Task InvokeAsync(CancellationToken ct = default)
    {
        foreach (var handler in _handlers)
        {
            switch (handler)
            {
                case Action action:
                    await Task.Run(action, ct);
                    break;
                case Func<Task> func:
                    await Task.Run(func, ct);
                    break;
                case Func<CancellationToken, Task> cancelableFunc:
                    await cancelableFunc(ct);
                    break;
                default:
                    throw new NotImplementedException();
            }
            ct.ThrowIfCancellationRequested();
        }
    }

    public void Invoke(CancellationToken ct = default)
        => InvokeAsync(ct).ConfigureAwait(false).GetAwaiter().GetResult();

    private readonly List<Delegate> _handlers = [];

    #region implicit conversion & constructors
    public CallbackSequential() { }
    public CallbackSequential(Action action) => _handlers.Add(action);
    public CallbackSequential(Func<Task> func) => _handlers.Add(func);
    public CallbackSequential(Func<CancellationToken, Task> func) => _handlers.Add(func);

    public static implicit operator CallbackSequential(Action action) => new(action);
    public static implicit operator CallbackSequential(Func<Task> func) => new(func);
    public static implicit operator CallbackSequential(Func<CancellationToken, Task> func) => new(func);
    #endregion

    #region operators
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Action action) => _handlers.Add(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<Task> func) => _handlers.Add(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Func<CancellationToken, Task> func) => _handlers.Add(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Action action) => _handlers.Remove(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<Task> func) => _handlers.Remove(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Func<CancellationToken, Task> func) => _handlers.Remove(func);

    public static CallbackSequential operator +(CallbackSequential? left, Action right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }
    public static CallbackSequential operator +(CallbackSequential? left, Func<Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }
    public static CallbackSequential operator +(CallbackSequential? left, Func<CancellationToken, Task> right)
    {
        left ??= new();
        left.Add(right);
        return left;
    }
    public static CallbackSequential operator -(CallbackSequential left, Action right)
    {
        left.Remove(right);
        return left;
    }
    public static CallbackSequential operator -(CallbackSequential left, Func<Task> right)
    {
        left.Remove(right);
        return left;
    }
    public static CallbackSequential operator -(CallbackSequential left, Func<CancellationToken, Task> right)
    {
        left.Remove(right);
        return left;
    }
    #endregion
}
