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

            //var designAtPosition = World.StructureHandler.StructureItems
            //    .FirstOrDefault(C => C.MapPosition == World.ToChunkSpace(Position))?.Design;
            //if (designAtPosition != null) return designAtPosition;

            var chunkOffset = World.ToChunkSpace(Position);
            var design = StructureGrid.Sample(chunkOffset, Biome.Structures.Designs);
            if (design != null && SampleDesign(design, chunkOffset, Biome, _distribution, EmptyItems))
            {
                _cache.TryAdd(Position, design);
                return design;
            }
            
            _cache.TryAdd(Position, null);
            return null;
        }
        
        private static bool SampleDesign(StructureDesign Design, Vector2 ChunkPosition, Region Biome,
            RandomDistribution Distribution, CollidableStructure[] Items)
        {
            Distribution.Seed = StructureDesign.BuildRngSeed(ChunkPosition);
            var targetPosition = StructureDesign.BuildTargetPosition(ChunkPosition, Distribution);
            return Design.ShouldSetup(ChunkPosition, ref targetPosition, Items, Biome, Distribution) &&
                   !StructureDesign.InterferesWithAnotherStructure(targetPosition);
        }

        public static void Discard()
        {
            _cache = new ConcurrentDictionary<Vector3, StructureDesign>();
            _distribution = new RandomDistribution(true);
        }
    }
}