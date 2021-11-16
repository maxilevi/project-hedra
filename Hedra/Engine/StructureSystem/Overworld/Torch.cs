using System.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Torch : Campfire
    {
        public const int DefaultRadius = 8;

        public Torch(Vector3 Position) : base(Position)
        {
        }

        protected override float LightRadius => DefaultRadius;
        protected override Vector3 LightColor => base.LightColor * 2;
        public override bool CanCraft => false;
    }
}