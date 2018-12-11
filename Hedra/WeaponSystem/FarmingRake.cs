using Hedra.Engine;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    public class FarmingRake : Sword
    {
        public FarmingRake(VertexData Contents) : base(Contents)
        {
        }
        
        protected override float PrimarySpeed => 0.75f;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorRake.dae",
        };
        protected override float SecondarySpeed => 0.75f;

        protected override string[] SecondaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorRake.dae"
        };

        protected override void OnPrimaryAttack()
        {
            base.OnPrimaryAttack();
            MainMesh.BeforeLocalRotation = Vector3.Zero;
            MainMesh.TargetRotation = new Vector3(0, 90, 180);
        }
        
        protected override void OnAttackStance()
        {
            var mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() 
                       * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);                  
            this.MainMesh.TransformationMatrix = mat4;
            this.MainMesh.Position = Owner.Model.Position;
            this.MainMesh.TargetRotation = new Vector3(90,25,180);
            this.MainMesh.BeforeLocalRotation = Vector3.Zero;   
        }

        public override void Attack1(IHumanoid Human, AttackOptions Options)
        {
            if (!MeetsRequirements()) return;
            base.BasePrimaryAttack(Human, Options);
        }

        public override void Attack2(IHumanoid Human, AttackOptions Options)
        {
            if (!MeetsRequirements()) return;
            base.BasePrimaryAttack(Human, Options);
        }
    }
}