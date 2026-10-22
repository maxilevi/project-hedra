using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.BiomeSystem.ShroomDimension;
using Hedra.BiomeSystem;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace HedraTests.Generation
{
    [NonParallelizable]
    public class HeadlessBlockGenerationTest
    {
        [TestCase(546068172, 4992, 4992, false, "normal")]
        [TestCase(43, -4096, 12288, false, "normal")]
        [TestCase(7, 0, 0, false, "normal")]
        [TestCase(546068172, 4992, 4992, true, "normal")]
        [TestCase(43, -4096, 12288, true, "normal")]
        [TestCase(7, 0, 0, true, "normal")]
        [TestCase(546068172, 4992, 4992, false, "shroom")]
        [TestCase(43, -4096, 12288, false, "shroom")]
        [TestCase(7, 0, 0, false, "shroom")]
        [TestCase(546068172, 4992, 4992, true, "shroom")]
        [TestCase(43, -4096, 12288, true, "shroom")]
        [TestCase(7, 0, 0, true, "shroom")]
        public void RealBlockGenerationMatchesReferenceWithoutStartingTheGame(int Seed, int X, int Z, bool Features, string Biome)
        {
            RunOnGenerationThread(() =>
            {
                GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
                var previous = Environment.GetEnvironmentVariable("HEDRA_VERIFY_TERRAIN");
                try
                {
                    Environment.SetEnvironmentVariable("HEDRA_VERIFY_TERRAIN", "1");
                    using var fixture = new HeadlessGenerationFixture(Seed, X, Z, Features,
                        Biome == "shroom" ? (BiomeGenerationDesign)new ShroomDimensionGenerationDesign() : new NormalBiomeGenerationDesign());
                    var details = fixture.Generate();
                    var expected = fixture.Blocks.Select(b => b._bits).ToArray();
                    var water = fixture.Blocks.Select((b, i) => (b, i)).Where(v => v.b.Type == BlockType.Water)
                        .Select(v => new CoordinateHash3D(v.i / (Chunk.BoundsY * Chunk.BoundsZ),
                            (v.i / Chunk.BoundsZ) % Chunk.BoundsY, v.i % Chunk.BoundsZ)).ToArray();
                    CollectionAssert.AreEqual(water, fixture.WaterPositions);
                    var repeated = fixture.Generate();
                    CollectionAssert.AreEqual(water, fixture.WaterPositions);
                    CollectionAssert.AreEqual(expected, fixture.Blocks.Select(b => b._bits));
                    Assert.AreEqual(details.HasWater, repeated.HasWater);
                    Assert.AreEqual(details.IsRiverConstant, repeated.IsRiverConstant);
                }
                finally { Environment.SetEnvironmentVariable("HEDRA_VERIFY_TERRAIN", previous); }
            });
        }

        [TestCase(546068172, 4992, 4992)]
        [TestCase(43, -4096, 12288)]
        [TestCase(7, 0, 0)]
        public void MeasureRealBlockGeneration(int Seed, int X, int Z)
        {
            if (Environment.GetEnvironmentVariable("HEDRA_BLOCK_BENCHMARK") != "1") Assert.Ignore("Opt-in CPU benchmark");
            var report = "";
            var phases = new List<string>();
            RunOnGenerationThread(() =>
            {
                GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
                using var fixture = new HeadlessGenerationFixture(Seed, X, Z);
                var samples = new double[15];
                var allocations = new long[samples.Length];
                for (var i = -4; i < samples.Length; i++)
                {
                    if (i == 0) GenerationMetrics.Reset();
                    var before = GC.GetAllocatedBytesForCurrentThread();
                    var start = Stopwatch.GetTimestamp();
                    fixture.Generate();
                    if (i >= 0)
                    {
                        samples[i] = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
                        allocations[i] = GC.GetAllocatedBytesForCurrentThread() - before;
                    }
                }
                Array.Sort(samples);
                report = $"BLOCK_BENCH seed={Seed} x={X} z={Z} median_ms={samples[samples.Length / 2]:F4} allocated_bytes={allocations.Average():F0} blocks={fixture.Blocks.Length} runtime={RuntimeInformation.FrameworkDescription}";
                GenerationMetrics.Report(phases.Add);
            });
            TestContext.Progress.WriteLine(report);
            foreach (var phase in phases) TestContext.Progress.WriteLine(phase);
        }

        private static void RunOnGenerationThread(Action Action)
        {
            ExceptionDispatchInfo failure = null;
            // Production workers have dedicated 4 MB stacks; isolate the test harness
            // from NUnit's smaller stack while retaining the real generator.
            var thread = new Thread(() =>
            {
                try { Action(); }
                catch (Exception exception) { failure = ExceptionDispatchInfo.Capture(exception); }
            }, 8 * 1024 * 1024);
            thread.Start();
            thread.Join();
            failure?.Throw();
        }
    }
}
