using Hedra.BiomeSystem;
using Hedra.Engine.Generation.ChunkSystem;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.BiomeSystem
{
    public interface IBiomePool 
    {
        WorldType Type { get; }
        Region GetRegion(Vector3 Position);
        RegionColor GetAverageRegionColor(Vector3 Offset);
        Region GetPredominantBiome(Chunk Chunk);
        BiomeGenerator GetGenerator(Chunk Chunk);
    }
}