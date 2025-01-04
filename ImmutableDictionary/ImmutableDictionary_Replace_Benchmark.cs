using System;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;

namespace Samples.ImmutableDictionary;

[MemoryDiagnoser]
public class ImmutableDictionary_Replace_Benchmark
{
    [Params(100)]
    public int IterationsCount;

    [Params(100, 1_000, 10_000)]
    public int DictionarySize;
    private ImmutableDictionary<string, object> _immutableDictionary;

    [GlobalSetup]
    public void Setup()
    {
        _immutableDictionary = ImmutableDictionary<string, object>.Empty;

        for (var i = 0; i < DictionarySize; i++)
        {
            _immutableDictionary = _immutableDictionary.Add(Guid.NewGuid().ToString(), new());
        }
    }


    [Benchmark(Baseline = true)]
    public void Replace_ImmutableDictionary()
    {
        // for (var i = 0; i < IterationsCount; i++)
        // {
            var key = _immutableDictionary.Keys.ElementAt(Random.Shared.Next(0, DictionarySize));
            _immutableDictionary = _immutableDictionary.Remove(key).Add(key, new());
        // }
    }

    [Benchmark]
    public void Replace_ImmutableDictionary_Builder()
    {
        // for (var i = 0; i < IterationsCount; i++)
        // {
            var key = _immutableDictionary.Keys.ElementAt(Random.Shared.Next(0, DictionarySize));
            var builder = _immutableDictionary.ToBuilder();
            builder.Remove(key);
            builder.Add(key, new());
            _immutableDictionary = builder.ToImmutable();
        // }
    }

    [Benchmark]
    public void Replace_ImmutableDictionary_SetItem()
    {
        // for (var i = 0; i < IterationsCount; i++)
        // {
            var key = _immutableDictionary.Keys.ElementAt(Random.Shared.Next(0, DictionarySize));
            _immutableDictionary = _immutableDictionary.SetItem(key, new());
        // }
    }
}
