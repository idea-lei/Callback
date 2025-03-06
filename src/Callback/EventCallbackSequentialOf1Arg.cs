namespace Callback;

public class EventCallbackSequential<TArg>
{
    public async Task InvokeAsync(TArg arg, CancellationToken ct = default)
    {
        foreach (var handler in _handlers)
        {
            switch (handler)
            {
                case Action<TArg> action:
                    action(arg);
                    break;
                case Func<TArg, Task> func:
                    await func(arg);
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

    private readonly List<Delegate> _handlers = new();

    public EventCallbackSequential(Action<TArg> action) => _handlers.Add(action);
    public EventCallbackSequential(Func<TArg, Task> func) => _handlers.Add(func);
    public EventCallbackSequential(Func<TArg, CancellationToken, Task> func) => _handlers.Add(func);

    public static EventCallbackSequential<TArg> operator +(EventCallbackSequential<TArg>? left, Action<TArg> right)
    {
        if (left == null)
            return new(right);

        left._handlers.Add(right);
        return left;
    }
    public static EventCallbackSequential<TArg> operator +(EventCallbackSequential<TArg>? left, Func<TArg, Task> right)
    {
        if (left == null)
            return new(right);

        left._handlers.Add(right);
        return left;
    }
    public static EventCallbackSequential<TArg> operator +(EventCallbackSequential<TArg>? left, Func<TArg, CancellationToken, Task> right)
    {
        if (left == null)
            return new(right);

        left._handlers.Add(right);
        return left;
    }
    public static EventCallbackSequential<TArg> operator -(EventCallbackSequential<TArg> left, Action<TArg> right)
    {
        left._handlers.Remove(right);
        return left;
    }
    public static EventCallbackSequential<TArg> operator -(EventCallbackSequential<TArg> left, Func<TArg, Task> right)
    {
        left._handlers.Remove(right);
        return left;
    }
    public static EventCallbackSequential<TArg> operator -(EventCallbackSequential<TArg> left, Func<TArg, CancellationToken, Task> right)
    {
        left._handlers.Remove(right);
        return left;
    }

    public static implicit operator EventCallbackSequential<TArg>(Action<TArg> action) => new(action);
    public static implicit operator EventCallbackSequential<TArg>(Func<TArg, Task> func) => new(func);
    public static implicit operator EventCallbackSequential<TArg>(Func<TArg, CancellationToken, Task> func) => new(func);
}
