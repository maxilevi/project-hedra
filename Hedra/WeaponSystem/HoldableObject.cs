using System.Numerics;
using Hedra.Rendering;

namespace Hedra.WeaponSystem
{
    public class HoldableObject : Hands
    {
        public HoldableObject(VertexData Contents) : base(Contents)
        {
        }

        protected override Vector3 SheathedOffset => Vector3.Zero;

        protected override void OnAttackStance()
        {
            base.SetToMainHand(MainMesh);
            MainMesh.LocalRotation = new Vector3(135, 00, 0);
        }
    }
}