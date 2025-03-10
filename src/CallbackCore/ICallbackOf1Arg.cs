﻿namespace CallbackCore;

public interface IInvokeable<TArg>
{
    Task InvokeAsync(TArg arg, CancellationToken ct = default);
    void Invoke(TArg arg, CancellationToken ct = default);
}

/// <remarks>
/// This interface is to allow Callback to be operated just like an event
/// </remarks>
/// <typeparam name="TSelf">
/// TSelf is to determine the underlying type for the + operator when left is null
/// </typeparam>
public interface IDelegateRegistry<TSelf, TArg> where TSelf : new()
{
    void Add(Action<TArg> action);
    void Add(Func<TArg, Task> func);
    void Add(Func<TArg, CancellationToken, Task> func);
    void Remove(Action<TArg> action);
    void Remove(Func<TArg, Task> func);
    void Remove(Func<TArg, CancellationToken, Task> func);

    static IDelegateRegistry<TSelf, TArg> operator +(IDelegateRegistry<TSelf, TArg>? registerable, Action<TArg> @delegate)
    {
        registerable ??= (IDelegateRegistry<TSelf, TArg>)new TSelf();
        registerable.Add(@delegate);
        return registerable;
    }

    static IDelegateRegistry<TSelf, TArg> operator +(IDelegateRegistry<TSelf, TArg>? registerable, Func<TArg, Task> @delegate)
    {
        registerable ??= (IDelegateRegistry<TSelf, TArg>)new TSelf();
        registerable.Add(@delegate);
        return registerable;
    }

    static IDelegateRegistry<TSelf, TArg> operator +(IDelegateRegistry<TSelf, TArg>? registerable, Func<TArg, CancellationToken, Task> @delegate)
    {
        registerable ??= (IDelegateRegistry<TSelf, TArg>)new TSelf();
        registerable.Add(@delegate);
        return registerable;
    }

    static IDelegateRegistry<TSelf, TArg> operator -(IDelegateRegistry<TSelf, TArg> registerable, Action<TArg> @delegate)
    {
        registerable.Remove(@delegate);
        return registerable;
    }

    static IDelegateRegistry<TSelf, TArg> operator -(IDelegateRegistry<TSelf, TArg> registerable, Func<TArg, Task> @delegate)
    {
        registerable.Remove(@delegate);
        return registerable;
    }

    static IDelegateRegistry<TSelf, TArg> operator -(IDelegateRegistry<TSelf, TArg> registerable, Func<TArg, CancellationToken, Task> @delegate)
    {
        registerable.Remove(@delegate);
        return registerable;
    }
}

public interface ICallback<TSelf, TArg> : IInvokeable<TArg>, IDelegateRegistry<TSelf, TArg> where TSelf : new() { }