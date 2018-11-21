using Hedra.Engine.Rendering;

namespace Hedra.WeaponSystem
{
    public abstract class RangedWeapon : Weapon
    {
        public override bool IsMelee { get; protected set; } = false;
        protected RangedWeapon(VertexData MeshData) : base(MeshData)
        {
        }
    }
}
