using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace Samples.ConcurrentStacks;

public class ConcurrentStack_Push_Benchmark
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

    [IterationSetup]
    public void IterationSetup()
    {
        _concurrentStack = [];
        _stackLock = new();
        _stack = [];
    }

    private void Setup(Action action)
    {
        _threadHelper = new BenchmarkThreadHelper(ThreadsCount);

        for (var i = 0; i < ThreadsCount; i++)
        {
            _threadHelper.Add(action);
        }
    }

    [GlobalSetup(Target = nameof(Push_ConcurrentStack))]
    public void Setup_Push_ConcurrentStack()
    {
        void action()
        {
            for (var i = 0; i < AdditionsCount; i++)
            {
                _concurrentStack.Push(_testObject);
            }
        }

        Setup(action);
    }

    [Benchmark]
    public void Push_ConcurrentStack()
    {
        _threadHelper.ExecuteAndWait();
    }


    [GlobalSetup(Target = nameof(Push_Locked_Stack))]
    public void Setup_Push_Locked_Stack()
    {
        void action()
        {
            for (var i = 0; i < AdditionsCount; i++)
            {
                lock (_stackLock)
                {
                    _stack.Push(_testObject);
                }
            }
        }

        Setup(action);
    }

    [Benchmark(Baseline = true)]
    public void Push_Locked_Stack()
    {
        _threadHelper.ExecuteAndWait();
    }
}
