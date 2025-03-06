using CallbackCore;

var callback = new Callback(() => Thread.Sleep(5000));
var cts = new CancellationTokenSource(1000);
Console.WriteLine("start");
callback.Invoke(cts.Token);
Console.WriteLine("finished");