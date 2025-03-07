namespace CallbackCore;

public interface IInvokeable
{
    Task InvokeAsync(CancellationToken ct = default);
    void Invoke(CancellationToken ct = default);
}

/// <remarks>
/// This interface is to allow Callback to be operated just like an event
/// </remarks>
/// <typeparam name="TSelf">
/// TSelf is to determine the underlying type for the + operator when left is null
/// </typeparam>
public interface IDelegateRegistry<TSelf> where TSelf : new()
{
    void Add(Action action);
    void Add(Func<Task> func);
    void Add(Func<CancellationToken, Task> func);
    void Remove(Action action);
    void Remove(Func<Task> func);
    void Remove(Func<CancellationToken, Task> func);

    static IDelegateRegistry<TSelf> operator +(IDelegateRegistry<TSelf>? registerable, Action @delegate)
    {
        registerable ??= (IDelegateRegistry<TSelf>)new TSelf();
        registerable.Add(@delegate);
        return registerable;
    }

    static IDelegateRegistry<TSelf> operator +(IDelegateRegistry<TSelf>? registerable, Func<Task> @delegate)
    {
        registerable ??= (IDelegateRegistry<TSelf>)new TSelf();
        registerable.Add(@delegate);
        return registerable;
    }

    static IDelegateRegistry<TSelf> operator +(IDelegateRegistry<TSelf>? registerable, Func<CancellationToken, Task> @delegate)
    {
        registerable ??= (IDelegateRegistry<TSelf>)new TSelf();
        registerable.Add(@delegate);
        return registerable;
    }

    static IDelegateRegistry<TSelf> operator -(IDelegateRegistry<TSelf> registerable, Action @delegate)
    {
        registerable.Remove(@delegate);
        return registerable;
    }

    static IDelegateRegistry<TSelf> operator -(IDelegateRegistry<TSelf> registerable, Func<Task> @delegate)
    {
        registerable.Remove(@delegate);
        return registerable;
    }

    static IDelegateRegistry<TSelf> operator -(IDelegateRegistry<TSelf> registerable, Func<CancellationToken, Task> @delegate)
    {
        registerable.Remove(@delegate);
        return registerable;
    }
}

public interface ICallback<TSelf> : IInvokeable, IDelegateRegistry<TSelf> where TSelf : new() { }
