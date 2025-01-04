using System;
using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace Samples.ConcurrentDictionary;

public class ConcurrentDictionary_Get_Benchmark
{
    public static IEnumerable<int> ValuesForThreadsCount => [1, 2, Environment.ProcessorCount / 2, Environment.ProcessorCount];

    [ParamsSource(nameof(ValuesForThreadsCount))]
    public int ThreadsCount;

    private const int AdditionsCount = 2_000_000;

    private int _chunk;

    private ConcurrentDictionary<int, object> _concurrentDictionary;
    
    private object _dictionaryLock;
    private Dictionary<int, object> _dictionary;
    
    private BenchmarkThreadHelper _threadHelper;

    private void IterationSetup(Action action)
    {
        _concurrentDictionary = [];
        _dictionary = [];
        _dictionaryLock = new();
        _chunk = -1;

        _threadHelper = new BenchmarkThreadHelper(ThreadsCount);   

        for (var i = 0; i < ThreadsCount; i++)
        {
            _threadHelper.Add(action);
        }
    }

    [IterationSetup(Target = nameof(Get_ConcurrentDictionary))]
    public void Setup_Get_ConcurrentDictionary()
    {
        void action()
        {
            var chunkSize = AdditionsCount / ThreadsCount;
            var chunk = Interlocked.Increment(ref _chunk);

            for (var i = chunk * chunkSize; i < chunkSize * (chunk + 1); i++)
            {
                GC.KeepAlive(_concurrentDictionary[i]);
            }
        }

        IterationSetup(action);

        for (var i = 0; i < AdditionsCount; i ++)
            _concurrentDictionary[i] = new();
    }

    [Benchmark]
    public void Get_ConcurrentDictionary()
    {
        _threadHelper.ExecuteAndWait();
    }

    [IterationSetup(Target = nameof(Get_Dictionary))]
    public void Setup_Get_Dictionary()
    {
        void action()
        {
            var chunkSize = AdditionsCount / ThreadsCount;
            var chunk = Interlocked.Increment(ref _chunk);

            for (var i = chunk * chunkSize; i < chunkSize * (chunk + 1); i++)
            {
                lock (_dictionaryLock)
                {
                    GC.KeepAlive(_dictionary[i]);
                }
            }
        }

        IterationSetup(action);

        for (var i = 0; i < AdditionsCount; i ++)
            _dictionary[i] = new();
    }

    [Benchmark(Baseline = true)]
    public void Get_Dictionary()
    {
        _threadHelper.ExecuteAndWait();
    }
}
