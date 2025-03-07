namespace CallbackDemo;

internal static class Utils
{
    public static void SleepAndPrint(int milliSec)
    {
        Console.WriteLine($"Start sleep for {milliSec}ms, current thread: {Environment.CurrentManagedThreadId}");
        Thread.Sleep(milliSec);
        Console.WriteLine($"Stop skeep for {milliSec}ms, current thread: {Environment.CurrentManagedThreadId}");
    }

    public static async Task DelayAndPrint(int milliSec)
    {
        Console.WriteLine($"Start delay for {milliSec}ms, current thread: {Environment.CurrentManagedThreadId}");
        await Task.Delay(milliSec);
        Console.WriteLine($"Stop delay for {milliSec}ms, current thread: {Environment.CurrentManagedThreadId}");
    }
}
