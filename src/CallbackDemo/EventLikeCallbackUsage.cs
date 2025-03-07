using CallbackCore;

namespace CallbackDemo;

/// <summary>
/// A demo to show how to use Callback like an event.
/// </summary>
internal class EventLikeCallbackUsage
{
    // Since we do not have the `event` syntax sugar, we need to declare callback in the following way
    // But I can not stop you from casting IEventRegistry to Callback outside the class.
    private Callback _callback => (Callback)Callback;
    public IDelegateRegistry<Callback> Callback = new Callback();

    public async Task TriggerEvent()
    {
        await _callback.InvokeAsync();
    }
}
