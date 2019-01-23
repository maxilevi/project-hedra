using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    public abstract class RogueWeapon : MeleeWeapon
    {
        protected ObjectMesh SecondBlade { get; }
        protected override string AttackStanceName => "Assets/Chr/RogueBlade-Stance.dae";
        protected override float PrimarySpeed => 1.75f;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/RogueBladeLeftAttack.dae",
            "Assets/Chr/RogueBladeRightAttack.dae"
        };
        protected override float SecondarySpeed => 1.5f;
        protected override string[] SecondaryAnimationsNames => new []
        {
            "Assets/Chr/RogueBladeDoubleAttack.dae"
        };    
        
        protected RogueWeapon(VertexData MeshData) : base(MeshData)
        {
            var baseMesh = MeshData.Clone();
            baseMesh.Scale(Vector3.One * 1.75f);
            this.SecondBlade = ObjectMesh.FromVertexData(baseMesh);
        }

        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(AttackEventType.Mid != Type) return;
            Owner.AttackSurroundings(Owner.DamageEquation);
        }
        
        public override void Update(IHumanoid Human)
        {
            base.SetToDefault(SecondBlade);
            base.Update(Human);
            base.ApplyEffects(SecondBlade);
        }
        
        
        protected override void OnSheathed()
        {
            var mat4 = Owner.Model.ChestMatrix.ClearTranslation() 
                       * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.ChestPosition);
            this.MainMesh.Position = Owner.Position;
            this.MainMesh.TransformationMatrix = mat4;

            this.SecondBlade.Position = Owner.Position;
            this.SecondBlade.TransformationMatrix = mat4;
        }

        protected override void OnAttackStance()
        {

            var mat4L = Owner.Model.LeftWeaponMatrix.ClearTranslation() 
                        * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.LeftWeaponPosition);
            this.MainMesh.TransformationMatrix = mat4L;
            this.MainMesh.Position = Owner.Position;

            var mat4R = Owner.Model.RightWeaponMatrix.ClearTranslation() 
                        * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.RightWeaponPosition);
            this.SecondBlade.TransformationMatrix = mat4R;
            this.SecondBlade.Position = Owner.Position;
        }

        protected override void OnAttack()
        {
    
            var mat4L = Owner.Model.LeftWeaponMatrix.ClearTranslation() 
                        * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.LeftWeaponPosition);
                this.MainMesh.TransformationMatrix = mat4L;
            this.MainMesh.Position = Owner.Position;
        
            var mat4R = Owner.Model.RightWeaponMatrix.ClearTranslation() 
                        * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.RightWeaponPosition);
                this.SecondBlade.TransformationMatrix = mat4R;
            this.SecondBlade.Position = Owner.Position;
        }
        
        public override void Dispose()
        {
            base.Dispose();
            SecondBlade.Dispose();
        }
    }
}