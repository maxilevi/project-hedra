using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using HedraTests.Rendering;
using NUnit.Framework;

namespace HedraTests.Generation
{
    public class TerrainBlockPackingTest
    {
        [TestCase(false)]
        [TestCase(true)]
        public unsafe void PackingMatchesSettersAndPreservesWaterOrder(bool PopulatedOutput)
        {
            var rng = new Random(43);
            var samples = new SampledBlock[Chunk.BoundsX * Chunk.BoundsY * Chunk.BoundsZ];
            var expected = new Block[samples.Length];
            var actual = new Block[samples.Length];
            var expectedWater = new List<CoordinateHash3D> { new CoordinateHash3D(3, 7, 11) };
            var water = new List<CoordinateHash3D>(expectedWater);
            var special = new[] { -0f, 0f, .009f, -.009f, 20.47f, -20.47f, float.NaN, float.PositiveInfinity, float.NegativeInfinity };
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = new SampledBlock {
                    Density = i < special.Length ? special[i] : ((float)rng.NextDouble() - .5f) * 600,
                    Type = (BlockType)rng.Next(0, 16)
                };
                actual[i]._bits = expected[i]._bits = PopulatedOutput ? (ushort)rng.Next(0, 65536) : (ushort)0;
                expected[i].Type = samples[i].Type;
                expected[i].Density = samples[i].Density;
                if (samples[i].Type == BlockType.Water)
                    expectedWater.Add(new CoordinateHash3D(i / (Chunk.BoundsY * Chunk.BoundsZ),
                        (i / Chunk.BoundsZ) % Chunk.BoundsY, i % Chunk.BoundsZ));
            }
            var chunk = (Chunk)FormatterServices.GetUninitializedObject(typeof(Chunk));
            HeadlessTerrainFixture.Set(typeof(Chunk), chunk, "_waterLock", new object());
            HeadlessTerrainFixture.Set(typeof(Chunk), chunk, "_waterPositions", water);
            var generator = (LandscapeGenerator)FormatterServices.GetUninitializedObject(typeof(LandscapeGenerator));
            HeadlessTerrainFixture.Set(typeof(BiomeGenerator), generator, "<Parent>k__BackingField", chunk);
            fixed (SampledBlock* ptr = samples)
                typeof(LandscapeGenerator).GetMethod("CopyBlocks", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(generator, new object[] { new SampledBlockWrapper(ptr, Chunk.BoundsX, Chunk.BoundsY, Chunk.BoundsZ), actual });
            CollectionAssert.AreEqual(expected.Select(b => b._bits), actual.Select(b => b._bits));
            CollectionAssert.AreEqual(expectedWater, water);
        }
    }
}
