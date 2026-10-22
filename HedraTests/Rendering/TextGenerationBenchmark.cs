using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Hedra.Engine.Rendering.UI;
using Hedra.Game;
using Hedra.Rendering.UI;
using NUnit.Framework;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace HedraTests.Rendering
{
    [TestFixture, NonParallelizable]
    public class TextGenerationBenchmark
    {
        [Test]
        public void MeasureTextGeneration()
        {
            if (Environment.GetEnvironmentVariable("HEDRA_UI_BENCHMARK") != "1")
                Assert.Ignore("Set HEDRA_UI_BENCHMARK=1 to time warmed text generation.");
            var height = GameSettings.Height;
            var scaling = GameSettings.UIScaling;
            var workers = Configuration.Default.MaxDegreeOfParallelism;
            try
            {
                GameSettings.Height = 1080;
                GameSettings.UIScaling = 1;
                Configuration.Default.MaxDegreeOfParallelism = 1;
                TestContext.WriteLine($"UI_RUNTIME {RuntimeInformation.FrameworkDescription}; single-thread CPU; 7 paired rounds");
                var font = FontCache.GetNormal(18);
                var current = new TextProvider();
                var reference = new ReferenceTextProvider();
                foreach (var text in new[] { "Health: 123 / 456", "Collect $(BOLD)(ORANGE){3 MUSHROOMS}\nReturn to $(GREEN){the village}." })
                {
                    var name = text.Contains('$') ? "formatted" : "plain";
                    var options = new TextOptions();
                    var parameters = TextProvider.BuildParams(text, font, Color.White, options);
                    Measure(name + "-parse", 500,
                        () => TextProvider.BuildParams(text, font, Color.White, options),
                        () => ReferenceTextProvider.BuildParams(text, font, Color.White, options));
                    Measure(name + "-layout", 100,
                        () => TextProvider.CalculateNeededSize(parameters),
                        () => ReferenceTextProvider.CalculateNeededSize(parameters));
                    Measure(name + "-repeated", 15,
                        () => { using var image = current.BuildText(text, font, Color.White, options); },
                        () => { using var image = reference.BuildText(text, font, Color.White, options); });
                    var currentSequence = 0;
                    var referenceSequence = 0;
                    Measure(name + "-unique-raster", 15,
                        () => { using var image = current.BuildText(text + (currentSequence++).ToString("D8"), font, Color.White, options); },
                        () => { using var image = reference.BuildText(text + (referenceSequence++).ToString("D8"), font, Color.White, options); });
                    options.HasStroke = true;
                    Measure(name + "-repeated-stroke", 15,
                        () => { using var image = current.BuildText(text, font, Color.White, options); },
                        () => { using var image = reference.BuildText(text, font, Color.White, options); });
                    currentSequence = 0;
                    referenceSequence = 0;
                    Measure(name + "-unique-stroke", 15,
                        () => { using var image = current.BuildText("stroke " + text + (currentSequence++).ToString("D8"), font, Color.White, options); },
                        () => { using var image = reference.BuildText("stroke " + text + (referenceSequence++).ToString("D8"), font, Color.White, options); });
                }
            }
            finally
            {
                GameSettings.Height = height;
                GameSettings.UIScaling = scaling;
                Configuration.Default.MaxDegreeOfParallelism = workers;
                TextProvider.ClearBitmapCache();
            }
        }

        private static void Measure(string Name, int Iterations, Action Current, Action Reference)
        {
            for (var i = 0; i < 10; i++) { Current(); Reference(); }
            var current = new double[7];
            var reference = new double[7];
            var allocations = new long[7];
            var referenceAllocations = new long[7];
            for (var round = 0; round < 7; round++)
            {
                if ((round & 1) == 0)
                {
                    (reference[round], referenceAllocations[round]) = Run(Reference, Iterations);
                    (current[round], allocations[round]) = Run(Current, Iterations);
                }
                else
                {
                    (current[round], allocations[round]) = Run(Current, Iterations);
                    (reference[round], referenceAllocations[round]) = Run(Reference, Iterations);
                }
            }
            var old = reference.OrderBy(x => x).ElementAt(3);
            var now = current.OrderBy(x => x).ElementAt(3);
            TestContext.WriteLine($"UI_BENCH {Name}: reference={old:F2}us current={now:F2}us speedup={old / now:F2}x " +
                $"bytes={referenceAllocations.OrderBy(x => x).ElementAt(3)}->{allocations.OrderBy(x => x).ElementAt(3)}");
        }

        private static (double, long) Run(Action Action, int Iterations)
        {
            var allocated = GC.GetAllocatedBytesForCurrentThread();
            var start = Stopwatch.GetTimestamp();
            for (var i = 0; i < Iterations; i++) Action();
            return ((Stopwatch.GetTimestamp() - start) * 1_000_000.0 / Stopwatch.Frequency / Iterations,
                (GC.GetAllocatedBytesForCurrentThread() - allocated) / Iterations);
        }
    }
}
