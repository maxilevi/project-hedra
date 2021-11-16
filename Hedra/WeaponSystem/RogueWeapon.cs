using System.Numerics;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.WeaponSystem
{
    public abstract class RogueWeapon : MeleeWeapon
    {
        protected RogueWeapon(VertexData MeshData) : base(MeshData)
        {
            var baseMesh = MeshData.Clone();
            baseMesh.Scale(Vector3.One * 1.75f);
            SecondBlade = ObjectMesh.FromVertexData(baseMesh);
        }

        protected ObjectMesh SecondBlade { get; }
        protected override string AttackStanceName => "Assets/Chr/RogueBlade-Stance.dae";
        protected override float PrimarySpeed => 2.0f;

        protected override string[] PrimaryAnimationsNames => new[]
        {
            "Assets/Chr/RogueBladeLeftAttack.dae",
            "Assets/Chr/RogueBladeRightAttack.dae"
        };

        protected override float SecondarySpeed => 1.75f;

        protected override string[] SecondaryAnimationsNames => new[]
        {
            "Assets/Chr/RogueBladeDoubleAttack.dae"
        };

        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if (AttackEventType.Mid != Type) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 1f, Options.IgnoreEntities);
        }

        public override void Update(IHumanoid Human)
        {
            SetToDefault(SecondBlade);
            base.Update(Human);
            ApplyEffects(SecondBlade);
        }


        protected override void OnSheathed()
        {
            var mat4 = Owner.Model.ChestMatrix.ClearTranslation()
                       * Matrix4x4.CreateTranslation(-Owner.Position + Owner.Model.ChestPosition);
            MainMesh.Position = Owner.Position;
            MainMesh.TransformationMatrix = mat4;

            SecondBlade.Position = Owner.Position;
            SecondBlade.TransformationMatrix = mat4;
        }

        protected override void OnAttackStance()
        {
            var mat4L = Owner.Model.LeftWeaponMatrix.ClearTranslation()
                        * Matrix4x4.CreateTranslation(-Owner.Position + Owner.Model.LeftWeaponPosition);
            MainMesh.TransformationMatrix = mat4L;
            MainMesh.Position = Owner.Position;

            var mat4R = Owner.Model.RightWeaponMatrix.ClearTranslation()
                        * Matrix4x4.CreateTranslation(-Owner.Position + Owner.Model.RightWeaponPosition);
            SecondBlade.TransformationMatrix = mat4R;
            SecondBlade.Position = Owner.Position;
        }

        protected override void OnAttack()
        {
            var mat4L = Owner.Model.LeftWeaponMatrix.ClearTranslation()
                        * Matrix4x4.CreateTranslation(-Owner.Position + Owner.Model.LeftWeaponPosition);
            MainMesh.TransformationMatrix = mat4L;
            MainMesh.Position = Owner.Position;

            var mat4R = Owner.Model.RightWeaponMatrix.ClearTranslation()
                        * Matrix4x4.CreateTranslation(-Owner.Position + Owner.Model.RightWeaponPosition);
            SecondBlade.TransformationMatrix = mat4R;
            SecondBlade.Position = Owner.Position;
        }

        public override void Dispose()
        {
            base.Dispose();
            SecondBlade.Dispose();
        }
    }
}