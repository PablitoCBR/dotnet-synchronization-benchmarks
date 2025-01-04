using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace Samples.ConcurrentDictionary;

public class ConcurrentDictionary_Add_Benchmark
{
    public static IEnumerable<int> ValuesForThreadsCount => [1, 2, Environment.ProcessorCount / 2, Environment.ProcessorCount];

    [ParamsSource(nameof(ValuesForThreadsCount))]
    public int ThreadsCount;

    private const int AdditionsCount = 1_000_000;

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

    [IterationSetup(Target = nameof(Add_ConcurrentDictionary))]
    public void Setup_Add_ConcurrentDictionary()
    {
        void action()
        {
            var chunkSize = AdditionsCount / ThreadsCount;
            var chunk = Interlocked.Increment(ref _chunk);

            for (var i = chunk * chunkSize; i < chunkSize * (chunk + 1); i++)
            {
                _concurrentDictionary[i] = new object();
            }
        }

        IterationSetup(action);
    }

    [Benchmark]
    public void Add_ConcurrentDictionary()
    {
        _threadHelper.ExecuteAndWait();
    }

    [IterationSetup(Target = nameof(Add_Dictionary))]
    public void Setup_Add_Dictionary()
    {
        void action()
        {
            var chunkSize = AdditionsCount / ThreadsCount;
            var chunk = Interlocked.Increment(ref _chunk);

            for (var i = chunk * chunkSize; i < chunkSize * (chunk + 1); i++)
            {
                lock (_dictionaryLock)
                {
                    _dictionary[i] = new object();
                }
            }
        }

        IterationSetup(action);
    }

    [Benchmark(Baseline = true)]
    public void Add_Dictionary()
    {
        _threadHelper.ExecuteAndWait();
    }
}
