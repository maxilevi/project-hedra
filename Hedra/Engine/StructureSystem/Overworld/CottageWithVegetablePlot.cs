using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CottageWithVegetablePlot : Cottage
    {
        private bool _setup;
        public Vector3[] BanditPositions { get; set; }
        public Vector3 Scale { get; set; }
        
        public CottageWithVegetablePlot(Vector3 Position, float Radius) : base(Position, Radius)
        {
        }

        public void Setup(CollidableStructure Structure)
        {
            if(_setup) return;
            _setup = true;
            for (var i = 0; i < BanditPositions.Length; ++i)
            {
                var rotation = Vector3.UnitY * Utils.Rng.NextFloat() * 360f;
                CampfireDesign.SpawnMat(
                    BanditPositions[i],
                    rotation,
                    Matrix4x4.CreateScale(Scale) *  Matrix4x4.CreateRotationY(rotation.Y) * Matrix4x4.CreateTranslation(BanditPositions[i] - Vector3.UnitY),
                    Structure
                );
            }
        }

        public void MakeEmpty()
        {
            NPC?.Dispose();
        }
    }
}