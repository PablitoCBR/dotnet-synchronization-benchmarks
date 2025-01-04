using System;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;

namespace Samples.ImmutableDictionary;

[MemoryDiagnoser]
public class ImmutableDictionary_Add_IncrementalKey_Benchmark
{
    public static IEnumerable<int> ValuesForThreadsCount => [1, 2, Environment.ProcessorCount / 2, Environment.ProcessorCount];

    [ParamsSource(nameof(ValuesForThreadsCount))]
    public int ThreadsCount;
    
    private int _addedCount;
    private const int AdditionsCount = 1_000_000;

    private ImmutableDictionary<int, object> _immutableDictionary;

    private Dictionary<int, object> _dictionary;
    private object _dictionaryLock;

    private BenchmarkThreadHelper _threadHelper;


    private void IterationSetup(Action action)
    {
        _addedCount = 0;
        _immutableDictionary = ImmutableDictionary<int, object>.Empty;
        _dictionary = [];
        _dictionaryLock = new();

        _threadHelper = new BenchmarkThreadHelper(ThreadsCount);   

        for (var i = 0; i < ThreadsCount; i++)
        {
            _threadHelper.Add(action);
        }
    }

    [IterationSetup(Target = nameof(Add_ImmutableDictionary))]
    public void Setup_Add_ImmutableDictionary()
    {
        void action()
        {
            while (true)
            {
                var nextKey = Interlocked.Increment(ref _addedCount);

                if (nextKey > AdditionsCount)
                {
                    break;
                }

                var source = ImmutableDictionary<int, object>.Empty;
                var result = ImmutableDictionary<int, object>.Empty;

                do
                {
                    source = _immutableDictionary;
                    result = source.Add(nextKey, new());
                }
                while (Interlocked.CompareExchange(ref _immutableDictionary, result, source) != source);
            }
        }

        IterationSetup(action);
    }

    [Benchmark]
    public void Add_ImmutableDictionary()
    {
        _threadHelper.ExecuteAndWait();
    }

    [IterationSetup(Target = nameof(Add_Dictionary))]
    public void Setup_Add_Dictionary()
    {
        void action()
        {
            while (true)
            {
                var nextKey = Interlocked.Increment(ref _addedCount);

                if (nextKey > AdditionsCount)
                {
                    break;
                }

                lock (_dictionaryLock)
                {
                    _dictionary.Add(nextKey, new());
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
