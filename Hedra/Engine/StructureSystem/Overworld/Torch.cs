using System.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Torch : Campfire
    {
        public Torch(Vector3 Position) : base(Position)
        {
        }

        protected override float LightRadius => 20;
        protected override Vector3 LightColor => base.LightColor * 2;
    }
}