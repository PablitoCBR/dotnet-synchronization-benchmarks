using System;
using BenchmarkDotNet.Attributes;

namespace Samples.ReaderWriterLockSlim;

public class ReaderWriterLockSlim_EnterReadBenchmark
{
    public static IEnumerable<int> ValuesForThreadsCount => [1, Environment.ProcessorCount / 2, Environment.ProcessorCount];

    [ParamsSource(nameof(ValuesForThreadsCount))]
    public int ThreadsCount;

    [Params(1_000, 100_000)]
    public int ElementsCount;
    private int readCount;


    private readonly object _lock = new();
    private readonly System.Threading.ReaderWriterLockSlim _rwLock = new();
    private IReadOnlyDictionary<int, object> _dictionary;
    private BenchmarkThreadHelper _threadHelper;

    [GlobalSetup]
    public void Setup()
    {
        _dictionary = Enumerable.Range(0, ElementsCount).ToDictionary(num => num, _ => new object());
    }

    private void IterationSetup(Action action)
    {
        _threadHelper = new BenchmarkThreadHelper(ThreadsCount);
        readCount = 0;

        for (var i = 0; i < ThreadsCount; i++)
        {
            _threadHelper.Add(action);
        }
    }

    [IterationSetup(Target = nameof(Get_Lock))]
    public void Setup_Get_Lock()
    {
        void action()
        {
            while(true)
            {
                var count = Interlocked.Increment(ref readCount);

                if (count >= ElementsCount)
                {
                    break;
                }

                lock (_lock)
                {
                    var el = _dictionary[Random.Shared.Next(0, ElementsCount)];
                    GC.KeepAlive(el);
                }
            }
        }

        IterationSetup(action);
    }

    [Benchmark(Baseline = true)]
    public void Get_Lock()
    {
        _threadHelper.ExecuteAndWait();
    }

    [IterationSetup(Target = nameof(Get_ReaderWriterLockSlim))]
    public void Setup_Get_ReaderWriterLockSlim()
    {
        void action()
        {
            while(true)
            {
                var count = Interlocked.Increment(ref readCount);

                if (count >= ElementsCount)
                {
                    break;
                }

                _rwLock.EnterReadLock();

                try
                {
                    var el = _dictionary[Random.Shared.Next(0, ElementsCount)];
                    GC.KeepAlive(el);
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
        }

        IterationSetup(action);
    }


    [Benchmark]
    public void Get_ReaderWriterLockSlim()
    {
        _threadHelper.ExecuteAndWait();
    }
}
