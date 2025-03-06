namespace CallbackCore;

public class CallbackSequential
{
    public async Task InvokeAsync(CancellationToken ct = default)
    {
        foreach(var handler in _handlers)
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
    public static CallbackSequential operator +(CallbackSequential? left, Action right)
    {
        if (left == null)
            return new(right);

        left._handlers.Add(right);
        return left;
    }
    public static CallbackSequential operator +(CallbackSequential? left, Func<Task> right)
    {
        if (left == null)
            return new(right);

        left._handlers.Add(right);
        return left;
    }
    public static CallbackSequential operator +(CallbackSequential? left, Func<CancellationToken, Task> right)
    {
        if (left == null)
            return new(right);

        left._handlers.Add(right);
        return left;
    }
    public static CallbackSequential operator -(CallbackSequential left, Action right)
    {
        left._handlers.Remove(right);
        return left;
    }
    public static CallbackSequential operator -(CallbackSequential left, Func<Task> right)
    {
        left._handlers.Remove(right);
        return left;
    }
    public static CallbackSequential operator -(CallbackSequential left, Func<CancellationToken, Task> right)
    {
        left._handlers.Remove(right);
        return left;
    }
    #endregion
}
