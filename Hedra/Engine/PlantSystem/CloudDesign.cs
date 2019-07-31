using System;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class CloudDesign : PlantDesign
    {
        public override CacheItem Type => CacheItem.Cloud;

        public override bool AffectedByLod => false;

        public override Matrix4 TransMatrix(Vector3 Position, Random Rng)
        {
            var cloudPosition = new Vector3(Position.X, 256f + 1024f + Rng.NextFloat() * 32f - 16f, Position.Z);
            var transMatrix = Matrix4.CreateScale(Rng.NextFloat() * 6.0f + 40f);
            transMatrix *= Matrix4.CreateRotationY(360f * Rng.NextFloat() * Mathf.Radian);
            transMatrix *= Matrix4.CreateTranslation(cloudPosition);
            return transMatrix;
        }

        public override VertexData Paint(VertexData Data, Region Region, Random Rng)
        {
            return Data;
        }
    }
}
