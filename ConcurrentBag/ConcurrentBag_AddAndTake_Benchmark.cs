using System;
using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Samples.BlockingCollection_ConcurrentBag;

public class ConcurrentBag_AddTake_Benchmark
{
    public static IEnumerable<int> ValuesForThreadsCount => [2, Environment.ProcessorCount / 2, Environment.ProcessorCount];

    [ParamsSource(nameof(ValuesForThreadsCount))]
    public int ThreadsCount;

    [Params(0.25, 0.5, 0.75)]
    public double AddingToTakingThreadsDistribution;

    public int AddingThreads { get => (int)Math.Max(1m, (decimal)(ThreadsCount / AddingToTakingThreadsDistribution)); }
    public int TakingThreads { get => (int)Math.Max(1m, (decimal)(ThreadsCount / AddingToTakingThreadsDistribution)); }

    private int _addingCompletedCount;
    private const int AdditionsCount = 10_000_000;

    private ConcurrentBag<object> _concurrentBag ;

    private object _queueLock;
    private Queue<object> _queue;

    private BenchmarkThreadHelper _threadHelper;


    public void IterationSetup(Action additionAction, Action takeAction)
    {
        _addingCompletedCount = 0;
        _concurrentBag = [];
        _queueLock = new();
        _queue = [];

        _threadHelper = new BenchmarkThreadHelper(ThreadsCount);

        for (var i = 0; i < AddingThreads; i++)
        {
            _threadHelper.Add(additionAction);
        }
        
        for (var i = 0; i < TakingThreads; i++)
        {
            _threadHelper.Add(takeAction);
        }
    }


    [IterationSetup(Target = nameof(AddAndTake_ConcurrentBag))]
    public void Setup_AddAndTake_ConcurrentBag()
    {
        void addAction()
        {
            for (var i = 0; i < AdditionsCount / AddingThreads; i++)
            {
                _concurrentBag.Add(new());
            }

            Interlocked.Increment(ref _addingCompletedCount);
        }

        void takeAction()
        {
            while(_concurrentBag.TryTake(out var el) || _addingCompletedCount != AddingThreads)
            {
                GC.KeepAlive(el);
            }
        }

        IterationSetup(addAction, takeAction);
    }

    [Benchmark]
    public void AddAndTake_ConcurrentBag()
    {
        _threadHelper.ExecuteAndWait();
    }

    [IterationSetup(Target = nameof(AddAndTake_Locked_Queue))]
    public void Setup_AddAndTake_Locked_Queue()
    {
        void addAction()
        {
            for (var i = 0; i < AdditionsCount / AddingThreads; i++)
            {
                lock (_queueLock)
                {
                    _queue.Enqueue(new());
                }
            }

            Interlocked.Increment(ref _addingCompletedCount);
        }

        void takeAction()
        {
            var itemDequeued = false;

            do
            {
                lock (_queueLock)
                {
                    itemDequeued = _queue.TryDequeue(out var el);
                    GC.KeepAlive(el);
                }
            } while (itemDequeued || _addingCompletedCount != AddingThreads);
        }

        IterationSetup(addAction, takeAction);
    }

    [Benchmark(Baseline = true)]
    public void AddAndTake_Locked_Queue()
    {
        _threadHelper.ExecuteAndWait();
    }
}
