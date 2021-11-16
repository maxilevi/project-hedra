using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using Hedra.Numerics;

namespace Hedra.Structures
{
    public class MapBuilder
    {
        private static ConcurrentDictionary<Vector3, StructureDesign> _cache;
        private static readonly CollidableStructure[] EmptyItems = new CollidableStructure[0];
        private static RandomDistribution _distribution;

        static MapBuilder()
        {
            _cache = new ConcurrentDictionary<Vector3, StructureDesign>();
            _distribution = new RandomDistribution(true);
        }

        public static StructureDesign Sample(Vector3 Position, Region Biome)
        {
            if (_cache.TryGetValue(Position, out var cachedDesign))
                return cachedDesign;

            var designAtPosition = World.StructureHandler.StructureItems
                .FirstOrDefault(C => C.MapPosition == World.ToChunkSpace(Position))?.Design;
            if (designAtPosition != null) return designAtPosition;

            var chunkOffset = World.ToChunkSpace(Position);
            for (var i = 0; i < Biome.Structures.Designs.Length; i++)
            {
                var design = Biome.Structures.Designs[i];
                if (SampleDesign(design, chunkOffset, Biome, _distribution, EmptyItems, out _))
                {
                    _cache.TryAdd(Position, design);
                    return design;
                }
            }

            _cache.TryAdd(Position, null);
            return null;
        }

        public static bool SampleDesign(StructureDesign Design, Vector2 ChunkPosition, Region Biome,
            RandomDistribution Distribution, CollidableStructure[] Items, out Vector3 TargetPosition)
        {
            Distribution.Seed = StructureDesign.BuildRngSeed(ChunkPosition);
            TargetPosition = StructureDesign.BuildTargetPosition(ChunkPosition, Distribution);
            return Design.ShouldSetup(ChunkPosition, ref TargetPosition, Items, Biome, Distribution) &&
                   !StructureDesign.InterferesWithAnotherStructure(TargetPosition);
        }

        public static void Discard()
        {
            _cache = new ConcurrentDictionary<Vector3, StructureDesign>();
            _distribution = new RandomDistribution(true);
        }
    }
}