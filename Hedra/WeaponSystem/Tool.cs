using System.Numerics;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.WeaponSystem
{
    public abstract class Tool : Sword
    {
        protected Tool(VertexData Contents) : base(Contents)
        {
        }

        protected override void OnPrimaryAttack()
        {
            base.OnPrimaryAttack();
            MainMesh.BeforeRotation = Vector3.Zero;
            MainMesh.LocalRotation = new Vector3(0, 90, 180);
        }

        protected override void OnAttackStance()
        {
            var mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation()
                       * Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
            MainMesh.TransformationMatrix = mat4;
            MainMesh.Position = Owner.Model.Position;
            MainMesh.LocalRotation = new Vector3(90, 25, 180);
            MainMesh.BeforeRotation = Vector3.Zero;
        }

        public override void Attack1(IHumanoid Human, AttackOptions Options)
        {
            if (!MeetsRequirements()) return;
            BasePrimaryAttack(Human, Options);
        }

        public override void Attack2(IHumanoid Human, AttackOptions Options)
        {
            if (!MeetsRequirements()) return;
            BaseSecondaryAttack(Human, Options);
        }
    }
}