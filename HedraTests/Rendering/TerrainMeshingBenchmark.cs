using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Runtime.InteropServices;
using Hedra;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Generation;
using System.Numerics;
using Hedra.Framework;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    [NonParallelizable]
    public class TerrainMeshingBenchmark
    {
        [TestCase("flat")]
        [TestCase("hills")]
        [TestCase("caves")]
        [TestCase("water")]
        [TestCase("materials")]
        public void MeasureFullCpuTerrainMeshing(string Shape)
        {
            if (Environment.GetEnvironmentVariable("HEDRA_MESH_BENCHMARK") != "1") Assert.Ignore("Opt-in CPU benchmark");
            TestContext.Progress.WriteLine($"MESH_METRICS enabled={GenerationMetrics.Enabled} env={Environment.GetEnvironmentVariable("HEDRA_PROFILE_GENERATION")}");
            GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var native = Path.Combine(GameLoader.AppPath, "native", "osx-" + RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(), "libhedracore.dylib");
                TestContext.Progress.WriteLine($"MESH_NATIVE sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(native))).ToLowerInvariant()}");
            }
            using var terrain = new HeadlessTerrainFixture(Shape);
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 128, Allocator.Megabyte * 4);
            var dense = terrain.DenseBuilder();
            foreach (var lod in new[] { 1, 2, 4, 8 })
            {
                foreach (var masked in new[] { false, true })
                {
                    GenerationMetrics.Reset();
                    var times = new double[15];
                    var allocated = new long[times.Length];
                    var vertices = 0;
                    for (var iteration = -4; iteration < times.Length; iteration++)
                    {
                        if (iteration == 0) GenerationMetrics.Reset();
                        Utils.Rng = new Random(43);
                        var beforeBytes = GC.GetAllocatedBytesForCurrentThread();
                        var start = Stopwatch.GetTimestamp();
                        ChunkTerrainMeshBuilder.ClearMapping(Vector3.Zero);
                        var output = (masked ? terrain.Builder : dense).CreateTerrainMesh(allocator, lod, terrain.Cache);
                        var elapsed = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
                        if (iteration >= 0) { times[iteration] = elapsed; allocated[iteration] = GC.GetAllocatedBytesForCurrentThread() - beforeBytes; }
                        Assert.IsFalse(output.Failed);
                        vertices = output.StaticData.Vertices.Count;
                        output.Dispose();
                    }
                    Array.Sort(times);
                    TestContext.Progress.WriteLine($"MESH_BENCH shape={Shape} lod={lod} masks={masked} median_ms={times[times.Length / 2]:F4} mean_ms={times.Average():F4} allocated_bytes={allocated.Average():F0} vertices={vertices} runtime={RuntimeInformation.FrameworkDescription}");
                    TestContext.Progress.WriteLine($"MESH_MODE fused_indexing={masked} surface_masks={masked}");
                    if (GenerationMetrics.Enabled) GenerationMetrics.Report(line => TestContext.Progress.WriteLine(line));
                }
            }
        }
    }
}
