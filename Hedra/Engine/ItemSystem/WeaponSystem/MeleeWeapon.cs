using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    public abstract class MeleeWeapon : Weapon
    {
        public override bool IsMelee { get; protected set; } = true;
        public ObjectMeshCollider MainCollider { get; protected set; }

        protected MeleeWeapon(VertexData MeshData) : base(MeshData)
        {
            //MainCollider = new ObjectMeshCollider(MeshData);
        }
    }
}
