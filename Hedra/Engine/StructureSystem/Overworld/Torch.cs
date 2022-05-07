using System.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Torch : Campfire
    {
        public const int DefaultRadius = 8;

        public Torch(Vector3 Position) : base(Position, Vector3.One) {}
        
        public Torch(Vector3 Position, Vector3 Radius) : base(Position, Radius)
        {
        }

        protected override float LightRadius => DefaultRadius;
        protected override Vector3 LightColor => base.LightColor * 2;
        public override bool CanCraft => false;
    }
}