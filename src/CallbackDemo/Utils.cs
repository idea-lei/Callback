namespace CallbackDemo;

internal static class Utils
{
    public static void SleepAndPrint(int milliSec)
    {
        Console.WriteLine($"Start sleep for {milliSec}ms, current thread: {Environment.CurrentManagedThreadId}");
        Thread.Sleep(milliSec);
        Console.WriteLine($"Stop sleep for {milliSec}ms, current thread: {Environment.CurrentManagedThreadId}");
    }

    public static async Task DelayAndPrint(int milliSec, CancellationToken ct = default)
    {
        Console.WriteLine($"Start delay for {milliSec}ms, current thread: {Environment.CurrentManagedThreadId}");
        await Task.Delay(milliSec, ct);
        Console.WriteLine($"Stop delay for {milliSec}ms, current thread: {Environment.CurrentManagedThreadId}");
    }
}
