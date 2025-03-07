using CallbackCore;
using CallbackDemo;

#region basic usage
var callbackBasic = new Callback(() => Utils.SleepAndPrint(5000));
Console.WriteLine("start");
callbackBasic.Invoke();
Console.WriteLine("finished");
#endregion


#region Cancellation
var callbackCancellation = new Callback(() => Utils.SleepAndPrint(5000));
var cts = new CancellationTokenSource(1000);
Console.WriteLine("start");
try
{
    callbackCancellation.Invoke(cts.Token);
    Console.WriteLine("finished");
}
catch (TaskCanceledException)
{
    Console.WriteLine("Task was canceled");
}
#endregion


#region event-like usage
var callbackEventLike = new EventLikeCallbackUsage();

callbackEventLike.Callback += () => Utils.SleepAndPrint(5000);

Console.WriteLine("start");
await callbackEventLike.TriggerEventAfter(1000);
Console.WriteLine("finished");
#endregion