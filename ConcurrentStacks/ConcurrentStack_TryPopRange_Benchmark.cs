using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace Samples.ConcurrentStacks;

public class ConcurrentStack_TryPopRange_Benchmark
{
    [Params(1, 4, 8, 16)] public int ThreadsCount;

    private const int AdditionsCount = 10_000;
    private readonly object _testObject = new();

    private ConcurrentStack<object> _concurrentStack = [];

    private BenchmarkThreadHelper _threadHelper;

    [GlobalCleanup]
    public void Cleanup()
    {
        _threadHelper.Dispose();
    }

    private void Setup(Action action)
    {
        _threadHelper = new BenchmarkThreadHelper(ThreadsCount);

        for (var i = 0; i < ThreadsCount; i++)
        {
            _threadHelper.Add(action);
        }
    }

    [GlobalSetup(Target = nameof(TryPop_ConcurrentStack))]
    public void Setup_TryPop_ConcurrentStack()
    {
        _concurrentStack = [];

        void action()
        {
            while (_concurrentStack.TryPop(out var element))
            {
            }
        }

        Setup(action);
    }

    [IterationSetup(Target = nameof(TryPop_ConcurrentStack))]
    public void Setup_Iteration_TryPop_ConcurrentStack()
    {
        for (var i = 0; i < AdditionsCount * ThreadsCount; i++)
            _concurrentStack.Push(_testObject);
    }

    [Benchmark(Baseline = true)]
    public void TryPop_ConcurrentStack()
    {
        _threadHelper.ExecuteAndWait();
    }

    [GlobalSetup(Target = nameof(TryPopRange_ConcurrentStack))]
    public void Setup_TryPopRange_ConcurrentStack()
    {
        _concurrentStack = [];

        void action()
        {
            var buffer = new object[100];
            while (_concurrentStack.TryPopRange(buffer) > 0)
            {
            }
        }

        Setup(action);
    }

    [IterationSetup(Target = nameof(TryPopRange_ConcurrentStack))]
    public void Setup_Iteration_TryPopRange_ConcurrentStack()
    {
        for (var i = 0; i < AdditionsCount; i++)
            _concurrentStack.Push(_testObject);
    }

    [Benchmark]
    public void TryPopRange_ConcurrentStack()
    {
        _threadHelper.ExecuteAndWait();
    }
}