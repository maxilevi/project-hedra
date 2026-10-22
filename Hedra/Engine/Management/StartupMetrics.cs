using System;
using System.Diagnostics;
using System.Globalization;
using Hedra.Engine.IO;

namespace Hedra.Engine.Management
{
    // Startup is separate from world generation and steady-state frame profiling.
    public static class StartupMetrics
    {
        private static readonly bool Enabled = Environment.GetEnvironmentVariable("HEDRA_PROFILE_STARTUP") == "1";
        private static readonly Stopwatch Clock = new();

        public static void Start()
        {
            if (Enabled) Clock.Restart();
        }

        public static Scope Measure(string Name) => Enabled ? new Scope(Name) : default;

        public static void Ready()
        {
            if (!Enabled) return;
            Log.WriteLine(FormattableString.Invariant($"STARTUP_READY elapsed_ms={Clock.Elapsed.TotalMilliseconds:F2}"));
        }

        public readonly struct Scope : IDisposable
        {
            private readonly string _name;
            private readonly long _started;

            public Scope(string Name)
            {
                _name = Name;
                _started = Stopwatch.GetTimestamp();
            }

            public void Dispose()
            {
                if (_name == null) return;
                var milliseconds = (Stopwatch.GetTimestamp() - _started) * 1000.0 / Stopwatch.Frequency;
                Log.WriteLine($"STARTUP_PHASE name={_name} elapsed_ms={milliseconds.ToString("F2", CultureInfo.InvariantCulture)}");
            }
        }
    }
}
