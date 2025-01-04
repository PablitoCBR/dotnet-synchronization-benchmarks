using System;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;

namespace Samples.ImmutableDictionary;

[MemoryDiagnoser]
public class ImmutableDictionary_Builder_RandomKey_Benchmark
{
    [Params(1_000, 10_000)]
    public int Elements;

    [Benchmark(Baseline = true)]
    public void Add_ImmutableDictionary()
    {
        var dict = ImmutableDictionary<string, object>.Empty;

        for (var i = 0; i < Elements; i ++)
        {
            dict = dict.Add(Guid.NewGuid().ToString(), new());
        }

        GC.KeepAlive(dict);
    }

    [Benchmark]
    public void Add_ImmutableDictionary_Builder()
    {
        var dict = ImmutableDictionary<string, object>.Empty.ToBuilder();

        for (var i = 0; i < Elements; i ++)
        {
            dict.Add(Guid.NewGuid().ToString(), new());
        }

        var result = dict.ToImmutableDictionary();

        GC.KeepAlive(result);
    }
}
