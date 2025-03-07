using CallbackCore;
using CallbackDemo;

#region basic usage
var callbackBasic = new Callback(() => Utils.SleepAndPrint(2000));
Console.WriteLine("start basic usage");
callbackBasic.Invoke();
Console.WriteLine("finished");
#endregion



Console.WriteLine("\n-----------------------------------------------------------------------------------\n");



#region Cancellation
var callbackCancellation = new Callback(async (ct) => await Utils.DelayAndPrint(2000, ct));
callbackCancellation += () => Utils.SleepAndPrint(2000);
var ctsSync = new CancellationTokenSource(1000);
Console.WriteLine("start cancellation - sync Invoke");
try
{
    callbackCancellation.Invoke(ctsSync.Token);
    Console.WriteLine("finished");
}
catch (TaskCanceledException)
{
    Console.WriteLine("Task was canceled");
}

await Task.Delay(2500); // sync methods can not be canceled when started
Console.WriteLine("-----------------------------------");
var ctsAsync = new CancellationTokenSource(1000);
Console.WriteLine("start cancellation - InvokeAsync");
try
{
    await callbackCancellation.InvokeAsync(ctsAsync.Token);
    Console.WriteLine("finished");
}
catch (TaskCanceledException)
{
    Console.WriteLine("Task was canceled");
}
await Task.Delay(2500); // sync methods can not be canceled when started, even with InvokeAsync
#endregion



Console.WriteLine("\n-----------------------------------------------------------------------------------\n");



#region event-like usage
var callbackEventLike = new EventLikeCallbackUsage();

callbackEventLike.Callback += () => Utils.SleepAndPrint(2000);

Console.WriteLine("start event-like");
//await callbackEventLike.Callback.InvokeAsync(); // can not call Invoke outside the declaring class.
await callbackEventLike.TriggerEvent();
Console.WriteLine("finished");
#endregion