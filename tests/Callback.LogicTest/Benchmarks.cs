using BenchmarkDotNet.Attributes;
using CallbackCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests;

public static class Workload
{
    private const int Iterations = 10000;

    public static void SyncWork()
    {
        for (int i = 0; i < Iterations; i++)
        {
            _ = i * i;
        }
    }

    public static Task AsyncWork()
    {
        return Task.Run(() =>
        {
            for (int i = 0; i < Iterations; i++)
            {
                _ = i * i;
            }
        });
    }

    public static Task CancelableWork(CancellationToken ct)
    {
        return Task.Run(() =>
        {
            for (int i = 0; i < Iterations; i++)
            {
                ct.ThrowIfCancellationRequested();
                _ = i * i;
            }
        }, ct);
    }
}

[MemoryDiagnoser]
public class CallbackBenchmarks
{
    [Params(10, 100)]
    public int N { get; set; }

    private Dictionary<int, Callback> _callbacksWithActions = new Dictionary<int, Callback>();
    private Dictionary<int, Callback> _callbacksWithFuncs = new Dictionary<int, Callback>();
    private Dictionary<int, Callback> _callbacksWithCancelableFuncs = new Dictionary<int, Callback>();
    private Dictionary<int, Callback> _callbacksMixed = new Dictionary<int, Callback>();

    private readonly CancellationToken _ct = CancellationToken.None;

    [GlobalSetup]
    public void GlobalSetup()
    {
        int[] counts = new[] { 10, 100 };

        foreach (var count in counts)
        {
            // Setup callbacks with actions
            var cbActions = new Callback();
            for (int i = 0; i < count; i++)
            {
                cbActions += Workload.SyncWork;
            }
            _callbacksWithActions[count] = cbActions;

            // Setup callbacks with funcs
            var cbFuncs = new Callback();
            for (int i = 0; i < count; i++)
            {
                cbFuncs += Workload.AsyncWork;
            }
            _callbacksWithFuncs[count] = cbFuncs;

            // Setup callbacks with cancelable funcs
            var cbCancelable = new Callback();
            for (int i = 0; i < count; i++)
            {
                cbCancelable += Workload.CancelableWork;
            }
            _callbacksWithCancelableFuncs[count] = cbCancelable;

            // Setup mixed callbacks
            var cbMixed = new Callback();
            int perType = count / 3;
            for (int i = 0; i < perType; i++)
            {
                cbMixed += Workload.SyncWork;
                cbMixed += Workload.AsyncWork;
                cbMixed += Workload.CancelableWork;
            }
            int remainder = count - perType * 3;
            for (int i = 0; i < remainder; i++)
            {
                cbMixed += Workload.SyncWork;
            }
            _callbacksMixed[count] = cbMixed;
        }
    }

    [Benchmark]
    public async Task InvokeAsync_WithActions()
    {
        var callback = _callbacksWithActions[N];
        await callback.InvokeAsync(_ct);
    }

    [Benchmark]
    public async Task InvokeAsync_WithFuncs()
    {
        var callback = _callbacksWithFuncs[N];
        await callback.InvokeAsync(_ct);
    }

    [Benchmark]
    public async Task InvokeAsync_WithCancelableFuncs()
    {
        var callback = _callbacksWithCancelableFuncs[N];
        await callback.InvokeAsync(_ct);
    }

    [Benchmark]
    public async Task InvokeAsync_Mixed()
    {
        var callback = _callbacksMixed[N];
        await callback.InvokeAsync(_ct);
    }

    [Benchmark]
    public void Invoke_WithActions()
    {
        var callback = _callbacksWithActions[N];
        callback.Invoke(_ct);
    }

    [Benchmark]
    public void Invoke_WithFuncs()
    {
        var callback = _callbacksWithFuncs[N];
        callback.Invoke(_ct);
    }

    [Benchmark]
    public void Invoke_WithCancelableFuncs()
    {
        var callback = _callbacksWithCancelableFuncs[N];
        callback.Invoke(_ct);
    }

    [Benchmark]
    public void Invoke_Mixed()
    {
        var callback = _callbacksMixed[N];
        callback.Invoke(_ct);
    }
}
