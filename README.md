# Callback

Callback mechanism that solves following problems regarding native C# event/delegate system:

+ Unification of async and sync callback due to sync to async transition.
+ parallel invocation (`Task.WhenAll`), even for sync actions.
+ Waiting for Execution
+ Task cancellation

Callback is flexible wrapper upon `MulticastDelegate` that accepts different delegate types, it's for `Action` and `Func`

**Good For**: Use cases needing lightweight, customizable async callback orchestration, where libraries such as Rx are too heavy.

## Types

### Callback

The main class of this library. Handlers will be executed parallelly and Cancellation is supported. (See [Remarks](#Remarks) for detail)

`Callback` is thread-safe, it creates a local copy of current handler collection and send it to `Task.WhenAll`. (Although the `Task.WhenAll` will also create a copy if I do not, but not a thread-safe way.)

### CallbackSequential

The only difference between this and `Callback` would be that this class will invoke the handler methods in (registration) sequence, just like the normal `MulticastDelegate` does. This class is designed to be separated from `Callback` to make sure that users will know what they are doing / using.

**Usage of this class is strongly discouraged.** Use this class only if all the following requirements can be fulfilled:

+ The invocation sequence does matter (which means you've made a bad design, better change it before it gets even worse.)
+ You guarantee that your handler registration sequence is correct.
+ You do not deregister duplicate handlers. (since the target handler can not be determined (FIFO or FILO, or even specific index?)) Or you do not have any duplicate registered handlers.
+ You do not add / remove handler during execution.

`EventCallbackSequential` is **not thread safe**, since guarantee of registration sequence means that there should be no concurrency. 

### MethodCallback

`MethodCallback` is read-only delegate-like (`Delegate`) callback mechanism that allow **only one handler** and **one time initialization** to handle sync and async callback in a unified way. It's light-weight and does not allocate. (similar to `EventCallback` from Blazor)

## Remarks

1. Even `MulticastDelegate` represents a collection of `Delegate`, and both `Action` and `Func` are `MulticastDelegate`, I still have chosen `List<Action>` and `List<Func<Task>>` to store the delegates, it's even slightly faster and allocate less than using `Action` and `Func` directly. (maybe `GetInvocationList` is slow?)

2. Check demos if you do want to have a event-like Callback. It's sadly not elegant enough like the native `event`.

   ```c#
   private Callback _callback => (Callback)Callback;
   public IDelegateRegistry<Callback> Callback = new Callback();
   ```

3. You need to handle exceptions by your self. You may also want to be informed if task is canceled, so `Callback` will not swallow any exception.

4. Can not actually cancel a running sync action due to its nature. cancellation for `Action` and `Func<Task>` only work when corresponding task is not started. (like if you have many subscriptions and some of them are not started.) So the option with `CancelltionToken` is preferred.

## Best Practices

- **Prefer Async Handlers**: Minimize `Action` usage to avoid `Task.Run` overhead.
- **Avoid `CallbackSequential`**: Redesign workflows to avoid order dependencies, or wire up relevant methods and register them as one delegate.

## Alternatives

Consider these native .NET tools for complex scenarios:

- **System.Reactive (Rx)**: For event-driven pipelines with backpressure/transformation.
- **TPL Dataflow**: For parallel workflows with buffering/batching.
- **Channels**: For producer-consumer patterns.
