using System;
using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace Samples.ConcurrentQueue;

public class ConcurrentQueue_EnqueueDequeue_Benchmark
{
    private const int AdditionsCount = 100_000_000;

    [Params(1, 2, 4, 6)]
    public int EnqueueThreadsCount;

    [Params(1, 2, 4, 6)]
    public int DequeueThreadsCount;

    private int _additionsCompleted;


    private ConcurrentQueue<object> _concurrentQueue;

    private object _queueLock;
    private Queue<object> _queue;
    
    
    private BenchmarkThreadHelper _threadHelper;


    private void IterationSetup(
        Action enqueueAction,
        Action dequeueAction, 
        int enqueueThreadsCount,
        int dequeueThreadsCount)
    {
        _additionsCompleted = 0;
        _concurrentQueue = [];
        _queueLock = new object();
        _queue = [];

        _threadHelper = new BenchmarkThreadHelper(enqueueThreadsCount + dequeueThreadsCount);


        while (enqueueThreadsCount > 0 && dequeueThreadsCount > 0)
        {
            var addEnqueueAction = enqueueThreadsCount > 0 && Random.Shared.NextDouble() > 0.5;

            if (addEnqueueAction)
            {
                _threadHelper.Add(enqueueAction);
                enqueueThreadsCount--;
            }
            else
            {
                _threadHelper.Add(dequeueAction);
                dequeueThreadsCount--;
            }
        }
    }

    [IterationSetup(Target = nameof(EnqueueDequeue_ConcurrentQueue))]
    public void Setup_EnqueueDequeue_ConcurrentQueue()
    {
        void enqueueAction()
        {
            for (var i = 0; i < AdditionsCount / EnqueueThreadsCount; i ++)
            {
                _concurrentQueue.Enqueue(new());
            }

            Interlocked.Increment(ref _additionsCompleted);
        }

        void dequeueAction()
        {
            while (_additionsCompleted != EnqueueThreadsCount && _concurrentQueue.TryDequeue(out var element))
            {

            }
        }

        IterationSetup(enqueueAction, dequeueAction, EnqueueThreadsCount, DequeueThreadsCount);
    }

    [Benchmark]
    public void EnqueueDequeue_ConcurrentQueue()
    {
        _threadHelper.ExecuteAndWait();
    }

        [IterationSetup(Target = nameof(EnqueueDequeue_Queue))]
    public void Setup_EnqueueDequeue_Queue()
    {
        void enqueueAction()
        {
            for (var i = 0; i < AdditionsCount / EnqueueThreadsCount; i ++)
            {
                lock (_queueLock)
                {
                    _queue.Enqueue(new());
                }
            }

            Interlocked.Increment(ref _additionsCompleted);
        }

        void dequeueAction()
        {
            bool hasElements = true;

            while (_additionsCompleted != EnqueueThreadsCount && hasElements)
            {
                lock (_queueLock)
                {
                    hasElements = _queue.TryDequeue(out var element);
                }
            }
        }

        IterationSetup(enqueueAction, dequeueAction, EnqueueThreadsCount, DequeueThreadsCount);
    }

    [Benchmark]
    public void EnqueueDequeue_Queue()
    {
        _threadHelper.ExecuteAndWait();
    }
}
