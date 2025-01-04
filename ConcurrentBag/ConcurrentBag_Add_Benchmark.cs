using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace Samples.BlockingCollection_ConcurrentBag;

// BENCHMARK OK
// BenchmarkThreadHelper tworzony na nowo przy każdej iteracji
// AdditionsCount dzielone przez liczbę thread'ów tak aby zawsze była do dodania ta sama liczba elementów
public class ConcurrentBag_Add_Benchmark
{
    public static IEnumerable<int> ValuesForThreadsCount => [1, Environment.ProcessorCount / 2, Environment.ProcessorCount];

    [ParamsSource(nameof(ValuesForThreadsCount))]
    public int ThreadsCount;

    private const int AdditionsCount = 10_000_000;
    private ConcurrentBag<object> _concurrentBag;

    private object _queueLock;
    private Queue<object> _queue;

    private BenchmarkThreadHelper _threadHelper;

    private void IterationSetup(Action action)
    {
        _concurrentBag = [];
        _queue = [];
        _queueLock = new();

        _threadHelper = new BenchmarkThreadHelper(ThreadsCount);

        for (var i = 0; i < ThreadsCount; i++)
        {
            _threadHelper.Add(action);
        }
    }

    [IterationSetup(Target = nameof(Add_ConcurrentBag))]
    public void Setup_Add_ConcurrentBag()
    {
        void action()
        {
            for (var i = 0; i < AdditionsCount / ThreadsCount; i++)
            {
                _concurrentBag.Add(new object());
            }
        }

        IterationSetup(action);
    }

    [Benchmark]
    public void Add_ConcurrentBag()
    {
        _threadHelper.ExecuteAndWait();
    }

    [IterationSetup(Target = nameof(Add_Locked_Queue))]
    public void Setup_Add_Locked_Queue()
    {
        void action()
        {
            for (var i = 0; i < AdditionsCount / ThreadsCount; i++)
            {
                lock (_queueLock)
                {
                    _queue.Enqueue(new object());
                }
            }
        }

        IterationSetup(action);
    }

    [Benchmark(Baseline = true)]
    public void Add_Locked_Queue()
    {
        _threadHelper.ExecuteAndWait();
    }
}
