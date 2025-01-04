using BenchmarkDotNet.Running;
using Samples.BlockingCollection_ConcurrentBag;
using Samples.ConcurrentDictionary;
using Samples.ConcurrentQueue;
using Samples.ConcurrentStacks;
using Samples.ImmutableDictionary;
using Samples.ReaderWriterLockSlim;

// BenchmarkRunner.Run<ConcurrentBag_Add_Benchmark>();
// BenchmarkRunner.Run<ConcurrentBag_TryTake_Benchmark>();
// BenchmarkRunner.Run<ConcurrentBag_AddTake_Benchmark>();
// BenchmarkRunner.Run<ConcurrentStack_Push_Benchmark>();
// BenchmarkRunner.Run<ConcurrentStack_TryPop_Benchmark>();
// BenchmarkRunner.Run<ConcurrentStack_TryPopRange_Benchmark>();
// BenchmarkRunner.Run<ConcurrentDictionary_Add_Benchmark>();
// BenchmarkRunner.Run<ConcurrentDictionary_Get_Benchmark>();
// BenchmarkRunner.Run<ConcurrentQueue_EnqueueDequeue_Benchmark>();

// BenchmarkRunner.Run<ImmutableDictionary_Add_IncrementalKey_Benchmark>();
// BenchmarkRunner.Run<ImmutableDictionary_Add_RandomKey_Benchmark>();

// BenchmarkRunner.Run<ImmutableDictionary_Builder_RandomKey_Benchmark>();

BenchmarkRunner.Run<ImmutableDictionary_Replace_Benchmark>();

//BenchmarkRunner.Run<ReaderWriterLockSlim_EnterReadBenchmark>();