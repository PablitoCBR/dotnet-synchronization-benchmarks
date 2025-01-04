using System;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;

namespace Samples.ImmutableDictionary;

[MemoryDiagnoser]
public class ImmutableDictionary_Add_RandomKey_Benchmark
{
   public static IEnumerable<int> ValuesForThreadsCount => [1, 2, Environment.ProcessorCount / 2, Environment.ProcessorCount];

    [ParamsSource(nameof(ValuesForThreadsCount))]
    public int ThreadsCount;
    
    private const int AdditionsCount = 1_000_000;

    private ImmutableDictionary<Guid, object> _immutableDictionary;

    private Dictionary<Guid, object> _dictionary;
    private object _dictionaryLock;

    private BenchmarkThreadHelper _threadHelper;


    private void IterationSetup(Action action)
    {
        _immutableDictionary = ImmutableDictionary<Guid, object>.Empty;
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
                if (_immutableDictionary.Count >= AdditionsCount)
                {
                    break;
                }
                
                var source = ImmutableDictionary<Guid, object>.Empty;
                var result = ImmutableDictionary<Guid, object>.Empty;

                do
                {
                    source = _immutableDictionary;
                    result = source.Add(Guid.NewGuid(), new());
                } while (Interlocked.CompareExchange(ref _immutableDictionary, result, source) != source);
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
                lock (_dictionaryLock)
                {
                    if (_dictionary.Count >= AdditionsCount)
                    {
                        break;
                    }

                    _dictionary.Add(Guid.NewGuid(), new());
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
