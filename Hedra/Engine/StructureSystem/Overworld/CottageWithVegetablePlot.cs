using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.WorldBuilding;
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
            /* Remove all previous beds */
            var children = Children;
            for (var i = 0; i < children.Length; ++i)
            {
                if(children[i] is SleepingPad)
                    children[i].Dispose();
            }
            /* Add all the new beds */
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
            TaskScheduler.When(() => NPC != null, NPC.Dispose);
        }
    }
}