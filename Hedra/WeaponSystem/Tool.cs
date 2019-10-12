using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

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
                       * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
            this.MainMesh.TransformationMatrix = mat4;
            this.MainMesh.Position = Owner.Model.Position;
            this.MainMesh.LocalRotation = new Vector3(90, 25, 180);
            this.MainMesh.BeforeRotation = Vector3.Zero;
        }
        public override void Attack1(IHumanoid Human, AttackOptions Options)
        {
            if (!MeetsRequirements()) return;
            base.BasePrimaryAttack(Human, Options);
        }
        public override void Attack2(IHumanoid Human, AttackOptions Options)
        {
            if (!MeetsRequirements()) return;
            base.BaseSecondaryAttack(Human, Options);
        }
    }
}