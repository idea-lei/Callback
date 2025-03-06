namespace Callback;

public class EventCallbackSequential
{
    public async Task InvokeAsync(CancellationToken ct = default)
    {
        foreach(var handler in _handlers)
        {
            switch (handler)
            {
                case Action action:
                    action();
                    break;
                case Func<Task> func:
                    await func();
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

    private readonly List<Delegate> _handlers = new();

    #region implicit conversion & constructors
    public EventCallbackSequential(Action action) => _handlers.Add(action);
    public EventCallbackSequential(Func<Task> func) => _handlers.Add(func);
    public EventCallbackSequential(Func<CancellationToken, Task> func) => _handlers.Add(func);

    public static implicit operator EventCallbackSequential(Action action) => new(action);
    public static implicit operator EventCallbackSequential(Func<Task> func) => new(func);
    public static implicit operator EventCallbackSequential(Func<CancellationToken, Task> func) => new(func);
    #endregion

    #region operators
    public static EventCallbackSequential operator +(EventCallbackSequential? left, Action right)
    {
        if (left == null)
            return new(right);

        left._handlers.Add(right);
        return left;
    }
    public static EventCallbackSequential operator +(EventCallbackSequential? left, Func<Task> right)
    {
        if (left == null)
            return new(right);

        left._handlers.Add(right);
        return left;
    }
    public static EventCallbackSequential operator +(EventCallbackSequential? left, Func<CancellationToken, Task> right)
    {
        if (left == null)
            return new(right);

        left._handlers.Add(right);
        return left;
    }
    public static EventCallbackSequential operator -(EventCallbackSequential left, Action right)
    {
        left._handlers.Remove(right);
        return left;
    }
    public static EventCallbackSequential operator -(EventCallbackSequential left, Func<Task> right)
    {
        left._handlers.Remove(right);
        return left;
    }
    public static EventCallbackSequential operator -(EventCallbackSequential left, Func<CancellationToken, Task> right)
    {
        left._handlers.Remove(right);
        return left;
    }
    #endregion
}
