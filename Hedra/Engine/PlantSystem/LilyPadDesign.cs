using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class LilyPadDesign : PlantDesign
    {
        public override bool AffectedByLod => false;
        public override CacheItem Type => CacheItem.LilyPad;
        public override Matrix4x4 TransMatrix(Vector3 Position, Random Rng)
        {
            return Matrix4x4.CreateScale(Rng.NextFloat() * .5f + .75f) 
                   * Matrix4x4.CreateRotationY(Rng.NextFloat() * 360 * Mathf.Radian) 
                   * Matrix4x4.CreateTranslation((Position + Vector3.UnitY * 1.5f) * new Vector3(1, Chunk.BlockSize, 1));
        }

        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode0, Region.Colors.GrassColor * .75f);
            return Data;
        }
    }
}