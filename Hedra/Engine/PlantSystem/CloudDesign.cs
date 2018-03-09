using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class CloudDesign : PlantDesign
    {
        public override VertexData Model => CacheManager.GetModel(CacheItem.Cloud);

        public override Matrix4 TransMatrix(Vector3 Position, Random Rng)
        {
            var cloudPosition = new Vector3(Position.X, 800f + Rng.NextFloat() * 32f - 16f, Position.Z);
            Matrix4 transMatrix = Matrix4.CreateScale(Rng.NextFloat() * 6.0f + 40f);
            transMatrix *= Matrix4.CreateRotationY(360f * Rng.NextFloat());
            transMatrix *= Matrix4.CreateTranslation(cloudPosition);
            return transMatrix;
        }

        public override VertexData Paint(Vector3 Position, VertexData Data, Random Rng)
        {
            return Data;
        }
    }
}
