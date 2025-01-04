using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace Samples.ConcurrentStacks;

public class ConcurrentStack_TryPop_Benchmark
{
    [Params(1, 4, 8, 16)]
    public int ThreadsCount;

    private const int AdditionsCount = 1_000;
    private readonly object _testObject = new();

    private ConcurrentStack<object> _concurrentStack = [];

    private object _stackLock = new();
    private Stack<object> _stack;

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

    [GlobalSetup(Target = nameof(TryPop_Locked_Stack))]
    public void Setup_TryPop_Locked_Stack()
    {
        void action()
        {
            bool hasNext = true;

            while (hasNext)
            {
                lock (_stackLock)
                {
                    hasNext = _stack.TryPop(out var _);
                }
            }
        }

        Setup(action);
    }

    [IterationSetup(Target = nameof(TryPop_Locked_Stack))]
    public void Setup_Iteration_TryPop_Locked_Stack()
    {
        _stack = [];
        _stackLock = new();
        
        for (var i = 0; i < AdditionsCount; i++)
            _stack.Push(_testObject);
    }

    [Benchmark(Baseline = true)]
    public void TryPop_Locked_Stack()
    {
        _threadHelper.ExecuteAndWait();
    }

    [GlobalSetup(Target = nameof(TryPop_ConcurrentStack))]
    public void Setup_TryPop_ConcurrentStack()
    {
        _concurrentStack = [];
        
        void action()
        {
            while (_concurrentStack.TryPop(out var _))
            {
            }
        }

        Setup(action);
    }

    [IterationSetup(Target = nameof(TryPop_ConcurrentStack))]
    public void Setup_Iteration_TryPop_ConcurrentStack()
    {
        for (var i = 0; i < AdditionsCount; i++)
            _concurrentStack.Push(_testObject);
    }

    [Benchmark]
    public void TryPop_ConcurrentStack()
    {
        _threadHelper.ExecuteAndWait();
    }
}
