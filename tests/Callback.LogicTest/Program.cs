using Callback;
using Callback.LogicTest;

MulticastCallback callback = new Action(() => TestUtils.SleepAndPrint(1000));
callback += () => TestUtils.SleepAndPrint(1000);
callback += () => TestUtils.DelayAndPrint(1000);

await callback.InvokeAsync();