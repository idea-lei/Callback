namespace CallbackCore;

public interface IInvokeable<TArg1, TArg2>
{
    Task InvokeAsync(TArg1 arg1, TArg2 arg2, CancellationToken ct = default);
    void Invoke(TArg1 arg1, TArg2 arg2, CancellationToken ct = default);
}

public interface IDelegateRegistry<TSelf, TArg1, TArg2> where TSelf : new()
{
    // Registration methods
    void Add(Action<TArg1, TArg2> action);
    void Add(Func<TArg1, TArg2, Task> func);
    void Add(Func<TArg1, TArg2, CancellationToken, Task> func);

    // Removal methods
    void Remove(Action<TArg1, TArg2> action);
    void Remove(Func<TArg1, TArg2, Task> func);
    void Remove(Func<TArg1, TArg2, CancellationToken, Task> func);

    // Addition operators
    public static IDelegateRegistry<TSelf, TArg1, TArg2> operator +(
        IDelegateRegistry<TSelf, TArg1, TArg2>? registerable,
        Action<TArg1, TArg2> @delegate)
    {
        registerable ??= (IDelegateRegistry<TSelf, TArg1, TArg2>)new TSelf();
        registerable.Add(@delegate);
        return registerable;
    }

    public static IDelegateRegistry<TSelf, TArg1, TArg2> operator +(
        IDelegateRegistry<TSelf, TArg1, TArg2>? registerable,
        Func<TArg1, TArg2, Task> @delegate)
    {
        registerable ??= (IDelegateRegistry<TSelf, TArg1, TArg2>)new TSelf();
        registerable.Add(@delegate);
        return registerable;
    }

    public static IDelegateRegistry<TSelf, TArg1, TArg2> operator +(
        IDelegateRegistry<TSelf, TArg1, TArg2>? registerable,
        Func<TArg1, TArg2, CancellationToken, Task> @delegate)
    {
        registerable ??= (IDelegateRegistry<TSelf, TArg1, TArg2>)new TSelf();
        registerable.Add(@delegate);
        return registerable;
    }

    // Subtraction operators
    public static IDelegateRegistry<TSelf, TArg1, TArg2> operator -(
        IDelegateRegistry<TSelf, TArg1, TArg2> registerable,
        Action<TArg1, TArg2> @delegate)
    {
        registerable.Remove(@delegate);
        return registerable;
    }

    public static IDelegateRegistry<TSelf, TArg1, TArg2> operator -(
        IDelegateRegistry<TSelf, TArg1, TArg2> registerable,
        Func<TArg1, TArg2, Task> @delegate)
    {
        registerable.Remove(@delegate);
        return registerable;
    }

    public static IDelegateRegistry<TSelf, TArg1, TArg2> operator -(
        IDelegateRegistry<TSelf, TArg1, TArg2> registerable,
        Func<TArg1, TArg2, CancellationToken, Task> @delegate)
    {
        registerable.Remove(@delegate);
        return registerable;
    }
}

public interface ICallback<TSelf, TArg1, TArg2> : IInvokeable<TArg1, TArg2>, IDelegateRegistry<TSelf, TArg1, TArg2> where TSelf : new() { }