using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
    public interface IBiomePool
    {
        BiomeDesign GetBiomeDesign(Vector3 Offset);
        Region GetRegion(Vector3 Position);
        Region GetAverageRegion(Vector3 Offset, float Spacing);
        RegionColor GetAverageRegionColor(Vector3 Offset);
        float VoronoiFormula(Vector3 Offset);
        Region GetPredominantBiome(Chunk Chunk);
        Region GetPredominantBiome(Vector2 ChunkOffset);
    }
}