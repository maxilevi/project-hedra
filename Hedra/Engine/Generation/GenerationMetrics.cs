using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Hedra.Engine.IO;

namespace Hedra.Engine.Generation
{
    // Per-operation timestamps: overlapping workers must never share a stopwatch.
    public static class GenerationMetrics
    {
        private sealed class Timing
        {
            public string Name;
            public long Ticks;
            public long Count;
        }

        private static readonly ConcurrentDictionary<string, Timing> Timings = new();
        private static readonly ConcurrentDictionary<string, Timing> Counters = new();
        public static bool Enabled { get; } = Environment.GetEnvironmentVariable("HEDRA_PROFILE_GENERATION") == "1"
                                              || GenerationBenchmark.Enabled;
        public static Scope Measure(string Phase) => new(Enabled ? Timings.GetOrAdd(Phase, Name => new Timing { Name = Name }) : null);

        public static void Reset()
        {
            Timings.Clear();
            Counters.Clear();
        }

        public static void Count(string Name, long Value)
        {
            if (!Enabled || Value == 0) return;
            Interlocked.Add(ref Counters.GetOrAdd(Name, name => new Timing { Name = name }).Count, Value);
        }

        public static void Report() => Report(line => Log.WriteLine(line));

        public static void Report(Action<string> WriteLine)
        {
            foreach (var counter in Counters.OrderBy(p => p.Key))
                WriteLine($"GEN_COUNT counter={counter.Key} count={Interlocked.Read(ref counter.Value.Count)}");
            foreach (var entry in Timings.OrderBy(P => P.Key))
            {
                var count = Interlocked.Read(ref entry.Value.Count);
                var ms = Interlocked.Read(ref entry.Value.Ticks) * 1000.0 / Stopwatch.Frequency;
                WriteLine($"GEN_PHASE phase={entry.Key} count={count} total_ms={ms:F2} mean_ms={ms / Math.Max(1, count):F2}");
            }
        }

        public readonly struct Scope : IDisposable
        {
            private readonly object _timing;
            private readonly long _start;

            internal Scope(object Timing)
            {
                _timing = Timing;
                _start = Timing == null ? 0 : Stopwatch.GetTimestamp();
            }

            public void Dispose()
            {
                if (_timing is not Timing timing) return;
                var elapsed = Stopwatch.GetTimestamp() - _start;
                Interlocked.Add(ref timing.Ticks, elapsed);
                Interlocked.Increment(ref timing.Count);
                if (elapsed > Stopwatch.Frequency / 10 && timing.Name.StartsWith("frame_"))
                    Log.WriteLine($"GEN_SLOW phase={timing.Name} elapsed_ms={elapsed * 1000.0 / Stopwatch.Frequency:F2}");
            }
        }
    }
}
