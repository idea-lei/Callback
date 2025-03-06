# Callback

Callback mechanism that solves following problems:

+ Unification of async and sync callback due to sync to async transition.
+ parallel async handler invocation (`Task.WhenAll`), even for sync actions.
+ Task cancellation

Callback is flexible `MulticastDelegate`, but not `event`.

Callback mainly uses `Action` and `Func`.

## Types

### Callback

`Callback` is `MulticastDelegate`-like callback mechanism to unify sync and async callback.

Handlers will be executed parallelly and Cancellation is supported. 

### EventCallbackSequential

The only difference between this and `EventCallback` would be that this class will invoke the handler methods in (registration) sequence, just like the normal event does. This class is designed to be separated from `EventCallback` to make sure that users will know what they are doing / using.

**Usage of this class is strongly discouraged.** Use this class only if all the following requirements can be fulfilled:

+ The invocation sequence does matter (which means you've made a bad design, better change it before it gets even worse.)
+ You guarantee that your handler registration sequence is correct.
+ You do not deregister duplicate handlers. (since the target handler can not be determined (FIFO or FILO, or even specific index?)) Or you do not have any duplicate registered handlers.
+ You do not add / remove handler during execution.

`EventCallbackSequential` is not thread safe, since guarantee of registration sequence means that there should be no concurrency. 

### MethodCallback

`MethodCallback` is delegate-like (`Delegate`) callback mechanism that allow **only one handler** and **one time initialization** to unify sync and async callback. It's light-weight and does not allocate.

