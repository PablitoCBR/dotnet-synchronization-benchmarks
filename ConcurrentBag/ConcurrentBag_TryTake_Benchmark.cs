using System;
using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace Samples.BlockingCollection_ConcurrentBag;

public class ConcurrentBag_TryTake_Benchmark
{
    public static IEnumerable<int> ValuesForThreadsCount => [1, Environment.ProcessorCount / 2, Environment.ProcessorCount];

    [ParamsSource(nameof(ValuesForThreadsCount))]
    public int ThreadsCount;

    public const int _elementsCount = 10_000_000;
    private ConcurrentBag<object> _concurrentBag;

    private object _queueLock;
    private Queue<object> _queue;

    private BenchmarkThreadHelper _threadHelper;

    private void IterationSetup(Action action)
    {
        _concurrentBag = [];
        _queue = [];
        _queueLock = new();
        
        _threadHelper = new BenchmarkThreadHelper(ThreadsCount);

        for (var i = 0; i < ThreadsCount; i ++)
        {
            _threadHelper.Add(action);
        } 
    }

    [IterationSetup(Target = nameof(TryTake_ConcurrentBag))]
    public void Setup_TryTake_ConcurrentBag()
    {
        void action()
        {
            while (_concurrentBag.TryTake(out var el))
            {
                GC.KeepAlive(el);
            }
        }

        IterationSetup(action);

        for (var i = 0; i < _elementsCount; i++)
            _concurrentBag.Add(new());
    }

    [Benchmark]
    public void TryTake_ConcurrentBag()
    {
        _threadHelper.ExecuteAndWait();
    }

    [IterationSetup(Target = nameof(TryDequeue_Locked_Queue))]
    public void Setup_TryDequeue_Locked_Queue()
    {
        void action()
        {
            while (true)
            {
                lock (_queueLock)
                {
                    if (!_queue.TryDequeue(out var el))
                    {
                        break;
                    }
                    
                    GC.KeepAlive(el);
                }
            }
        }

        IterationSetup(action);
        
        for (var i = 0; i < _elementsCount; i++)
            _queue.Enqueue(new());
    }

    [Benchmark(Baseline = true)]
    public void TryDequeue_Locked_Queue()
    {
        _threadHelper.ExecuteAndWait();
    }
}
