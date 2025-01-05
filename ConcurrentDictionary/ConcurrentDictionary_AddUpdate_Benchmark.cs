namespace Samples.ConcurrentDictionary;

using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

public class ConcurrentDictionary_AddUpdate_Benchmark
{
    public static IEnumerable<int> ValuesForThreadsCount => [1, 2, Environment.ProcessorCount / 2, Environment.ProcessorCount];

    [ParamsSource(nameof(ValuesForThreadsCount))]
    public int ThreadsCount;

    private const int AdditionsCount = 10_000_000;

    private int _chunk = -1;

    private ConcurrentDictionary<int, object> _concurrentDictionary;
    
    private object _dictionaryLock;
    private Dictionary<int, object> _dictionary;
    
    private BenchmarkThreadHelper _threadHelper;

    private void IterationSetup(Action action)
    {
        _concurrentDictionary = [];
        _dictionary = [];
        _dictionaryLock = new();

        _threadHelper = new BenchmarkThreadHelper(ThreadsCount);   

        for (var i = 0; i < ThreadsCount; i++)
        {
            _threadHelper.Add(action);
        }
    }

    [IterationSetup(Target = nameof(AddUpdate_ConcurrentDictionary))]
    public void Setup_AddUpdate_ConcurrentDictionary()
    {
        void action()
        {
            var chunkSize = AdditionsCount / ThreadsCount;
            var chunk = Interlocked.Increment(ref _chunk);

            for (var i = chunk * chunkSize; i < chunkSize * (chunk + 1); i++)
            {
                _concurrentDictionary[Random.Shared.Next(0, 1_000)] = new object();
            }
        }

        IterationSetup(action);
    }

    [Benchmark]
    public void AddUpdate_ConcurrentDictionary()
    {
        _threadHelper.ExecuteAndWait();
    }

    [IterationSetup(Target = nameof(AddUpdate_Dictionary))]
    public void Setup_AddUpdate_Dictionary()
    {
        void action()
        {
            var chunkSize = AdditionsCount / ThreadsCount;
            var chunk = Interlocked.Increment(ref _chunk);

            for (var i = chunk * chunkSize; i < chunkSize * (chunk + 1); i++)
            {
                lock (_dictionaryLock)
                {
                    _dictionary[Random.Shared.Next(0, 1_000)] = new object();
                }
            }
        }

        IterationSetup(action);
    }

    [Benchmark(Baseline = true)]
    public void AddUpdate_Dictionary()
    {
        _threadHelper.ExecuteAndWait();
    }
}