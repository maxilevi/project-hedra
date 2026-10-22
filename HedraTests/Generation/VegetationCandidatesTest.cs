using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using HedraTests.Rendering;
using NUnit.Framework;

namespace HedraTests.Generation
{
    public class VegetationCandidatesTest
    {
        [TestCase(0, 0)]
        [TestCase(-4096, 12288)]
        [TestCase(4992, 4992)]
        [TestCase(16777216, -16777216)]
        public void EveryColumnMatchesOriginalStructureAndGroundworkScans(int X, int Z)
        {
            var data = Inputs(X, Z, 256);
            var candidates = new VegetationCandidates(X, Z, data.Structures, data.Groundworks, data.Plateaus);
            for (var x = 0; x < Chunk.BoundsX; ++x)
            for (var z = 0; z < Chunk.BoundsZ; ++z)
            {
                var position = new Vector2(X + x * Chunk.BlockSize, Z + z * Chunk.BlockSize);
                OriginalStructures(data.Structures, position, out var grass, out var trees, out _);
                candidates.StructureExclusions(position, out var actualGrass, out var actualTrees);
                Assert.AreEqual(grass, actualGrass, $"Grass exclusion at {position}");
                Assert.AreEqual(trees, actualTrees, $"Tree exclusion at {position}");
                Assert.AreEqual(data.Groundworks.Any(g => g.NoPlants && g.Affects(position)),
                    candidates.PlantGroundworks.Any(g => g.Affects(position)), $"Groundwork plants at {position}");
                Assert.AreEqual(data.Groundworks.Any(g => g.NoTrees && g.Affects(position)),
                    candidates.TreeGroundworks.Any(g => g.Affects(position)), $"Groundwork trees at {position}");
                Assert.AreEqual(data.Plateaus.Any(p => p.NoTrees && p.Collides(position)),
                    candidates.TreePlateaus.Any(p => p.Collides(position)), $"Plateaus at {position}");
            }
        }

        [Test]
        public void RetainsWideLinesAndCustomShapesWhoseBoundsDoNotEncloseTheirEffects()
        {
            var line = new LineGroundwork(new Vector2(0, -8), new Vector2(12, -8)) { Width = 50, NoPlants = true };
            var custom = new CustomGroundwork(Vector3.One * 10000, 1, BlockType.Grass) { NoTrees = true };
            var candidates = new VegetationCandidates(0, 0, Array.Empty<CollidableStructure>(),
                new IGroundwork[] { line, custom }, Array.Empty<BasePlateau>());
            Assert.IsTrue(line.Affects(Vector2.Zero));
            CollectionAssert.Contains(candidates.PlantGroundworks, line);
            CollectionAssert.Contains(candidates.TreeGroundworks, custom);
        }

        [Test]
        public void ExclusionEdgesKeepStrictComparisonsAndOriginalFloatRounding()
        {
            foreach (var radius in new[] { .1f, 20f, 700f, -20f })
            foreach (var distance in new[] { MathF.Abs(radius) * .3f, MathF.Abs(radius) * .5f })
            foreach (var edge in new[] { MathF.BitDecrement(distance), distance, MathF.BitIncrement(distance) })
            {
                var structures = new[] { Structure(typeof(GiantTreeDesign), Mountain(new Vector2(-edge, 0), radius)) };
                var candidates = new VegetationCandidates(0, 0, structures, Array.Empty<IGroundwork>(), Array.Empty<BasePlateau>());
                OriginalStructures(structures, Vector2.Zero, out var grass, out var trees, out _);
                candidates.StructureExclusions(Vector2.Zero, out var actualGrass, out var actualTrees);
                Assert.AreEqual(grass, actualGrass, $"Grass radius={radius} edge={edge:R}");
                Assert.AreEqual(trees, actualTrees, $"Trees radius={radius} edge={edge:R}");
            }
        }

        [Test]
        public void SnapshotsAreRebuiltAfterStructuresAndGroundworksChange()
        {
            var mountain = Mountain(Vector2.Zero, 20);
            var structure = Structure(typeof(GiantTreeDesign), mountain);
            var groundwork = new RoundedGroundwork(Vector3.Zero, 5, BlockType.Grass);
            var first = new VegetationCandidates(0, 0, new[] { structure }, new[] { groundwork }, Array.Empty<BasePlateau>());
            mountain.Radius = 200;
            groundwork.NoPlants = true;
            var second = new VegetationCandidates(0, 0, new[] { structure }, new[] { groundwork }, Array.Empty<BasePlateau>());
            first.StructureExclusions(new Vector2(20, 0), out var oldGrass, out _);
            second.StructureExclusions(new Vector2(20, 0), out var newGrass, out _);
            Assert.IsFalse(oldGrass);
            Assert.IsTrue(newGrass);
            Assert.IsEmpty(first.PlantGroundworks);
            CollectionAssert.Contains(second.PlantGroundworks, groundwork);
        }

        [TestCase(32)]
        [TestCase(256)]
        public void MeasurePlacementExclusionScans(int Count)
        {
            if (Environment.GetEnvironmentVariable("HEDRA_VEGETATION_BENCHMARK") != "1") Assert.Ignore("Opt-in CPU benchmark");
            var inputs = Inputs(-4096, 12288, Count);
            int Original()
            {
                var plants = inputs.Groundworks.Where(g => g.NoPlants).ToArray();
                var trees = inputs.Groundworks.Where(g => g.NoTrees).ToArray();
                var plateaus = inputs.Plateaus.Where(p => p.NoTrees).ToArray();
                var result = 0;
                for (var x = 0; x < Chunk.BoundsX; ++x)
                for (var z = 0; z < Chunk.BoundsZ; ++z)
                {
                    var p = new Vector2(-4096 + x * Chunk.BlockSize, 12288 + z * Chunk.BlockSize);
                    OriginalStructures(inputs.Structures, p, out var grass, out var noTrees, out _);
                    if (grass) result++;
                    if (noTrees) result++;
                    foreach (var g in plants) if (g.Affects(p)) { result++; break; }
                    foreach (var g in trees) if (g.Affects(p)) { result++; break; }
                    foreach (var plateau in plateaus) if (plateau.Collides(p)) { result++; break; }
                }
                return result;
            }
            int Optimized()
            {
                var candidates = new VegetationCandidates(-4096, 12288, inputs.Structures, inputs.Groundworks, inputs.Plateaus);
                var result = 0;
                for (var x = 0; x < Chunk.BoundsX; ++x)
                for (var z = 0; z < Chunk.BoundsZ; ++z)
                {
                    var p = new Vector2(-4096 + x * Chunk.BlockSize, 12288 + z * Chunk.BlockSize);
                    candidates.StructureExclusions(p, out var grass, out var noTrees);
                    if (grass) result++;
                    if (noTrees) result++;
                    foreach (var g in candidates.PlantGroundworks) if (g.Affects(p)) { result++; break; }
                    foreach (var g in candidates.TreeGroundworks) if (g.Affects(p)) { result++; break; }
                    foreach (var plateau in candidates.TreePlateaus) if (plateau.Collides(p)) { result++; break; }
                }
                return result;
            }
            Assert.AreEqual(Original(), Optimized());
            var old = new double[15];
            var next = new double[15];
            for (var i = -3; i < old.Length; ++i)
            {
                var a = (i & 1) == 0 ? Measure(Original) : Measure(Optimized);
                var b = (i & 1) == 0 ? Measure(Optimized) : Measure(Original);
                if (i >= 0) { old[i] = (i & 1) == 0 ? a : b; next[i] = (i & 1) == 0 ? b : a; }
            }
            Array.Sort(old);
            Array.Sort(next);
            TestContext.Progress.WriteLine($"VEGETATION_BENCH structures={Count} original_ms={old[7]:F4} optimized_ms={next[7]:F4} speedup={old[7] / next[7]:F2} runtime={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        }

        private static double Measure(Func<int> Scan)
        {
            var start = Stopwatch.GetTimestamp();
            var result = 0;
            for (var i = 0; i < 10; ++i) result += Scan();
            GC.KeepAlive(result);
            return (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency / 10;
        }

        private static void OriginalStructures(CollidableStructure[] Structures, Vector2 Position, out bool Grass, out bool Trees, out bool Merchant)
        {
            Grass = Trees = Merchant = false;
            foreach (var structure in Structures)
            {
                if (structure.Design is GiantTreeDesign)
                {
                    var radius = structure.Mountain.Radius;
                    if ((structure.Mountain.Position - Position).LengthSquared() < radius * .3f * radius * .3f) Grass = true;
                    if (!Trees && (structure.Mountain.Position - Position).LengthSquared() < radius * .5f * radius * .5f) Trees = true;
                }
                // The old scan also tested travelling merchant zones; neither
                // vegetation caller used the result, but the work still occurred.
                if (structure.Design is TravellingMerchantDesign)
                {
                    var radius = structure.Mountain.Radius;
                    if ((structure.Mountain.Position - Position).LengthSquared() < radius * .5f * radius * .5f)
                        Merchant = true;
                }
            }
        }

        private static (CollidableStructure[] Structures, IGroundwork[] Groundworks, BasePlateau[] Plateaus) Inputs(int X, int Z, int Count)
        {
            var rng = new Random(47);
            var structures = new CollidableStructure[Count];
            var groundworks = new IGroundwork[Count];
            var plateaus = new BasePlateau[8];
            for (var i = 0; i < Count; ++i)
            {
                var position = new Vector2(X, Z) + new Vector2(i % 16 == 0 ? rng.Next(-20, 160) : rng.Next(1500, 3000), rng.Next(-100, 200));
                structures[i] = Structure(i % 3 == 0 ? typeof(GiantTreeDesign) : typeof(TravellingMerchantDesign),
                    Mountain(position, i % 4 == 0 ? 700 : 50));
                var radius = (i % 5 == 0 ? .1f : 30) + (float)rng.NextDouble() * 20;
                groundworks[i] = i % 2 == 0 ? new RoundedGroundwork(new Vector3(position.X, 0, position.Y), radius, BlockType.Grass)
                    : new SquaredGroundwork(new Vector3(position.X, 0, position.Y), radius * 2, BlockType.Grass);
                groundworks[i].NoPlants = i % 3 != 0;
                groundworks[i].NoTrees = i % 4 != 0;
                if (i < plateaus.Length) { plateaus[i] = Mountain(position, radius); plateaus[i].NoTrees = i % 2 == 0; }
            }
            return (structures, groundworks, plateaus);
        }

        private static CollidableStructure Structure(Type Design, RoundedPlateau Mountain)
        {
            var structure = (CollidableStructure)FormatterServices.GetUninitializedObject(typeof(CollidableStructure));
            HeadlessTerrainFixture.Set(typeof(CollidableStructure), structure, "<Design>k__BackingField", FormatterServices.GetUninitializedObject(Design));
            HeadlessTerrainFixture.Set(typeof(CollidableStructure), structure, "<Mountain>k__BackingField", Mountain);
            return structure;
        }

        private static RoundedPlateau Mountain(Vector2 Position, float Radius)
        {
            var plateau = (RoundedPlateau)FormatterServices.GetUninitializedObject(typeof(RoundedPlateau));
            HeadlessTerrainFixture.Set(typeof(BasePlateau), plateau, "<Position>k__BackingField", Position);
            plateau.Radius = Radius;
            return plateau;
        }

        private sealed class CustomGroundwork : RoundedGroundwork
        {
            public CustomGroundwork(Vector3 Position, float Radius, BlockType Type) : base(Position, Radius, Type) { }
            public override bool Affects(Vector2 Position) => true;
        }
    }
}
