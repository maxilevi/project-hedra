/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    /// <summary>
    /// Description of TwoHandedSword.
    /// </summary>
    public class DoubleBlades : Weapon
    {
        public override bool IsMelee { get; protected set; } = true;
        private EntityMesh SecondBlade;

        public DoubleBlades(VertexData Contents) : base(Contents)
        {
            VertexData BaseMesh = Contents.Clone();
            BaseMesh.Scale(Vector3.One * 1.75f);

            this.SecondBlade = EntityMesh.FromVertexData(BaseMesh);
            base.Mesh = EntityMesh.FromVertexData(BaseMesh);

            AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/RogueBlade-Stance.dae");

            PrimaryAnimations = new Animation[2];
            PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeLeftAttack.dae");
            PrimaryAnimations[1] = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeRightAttack.dae");

            for (int i = 0; i < PrimaryAnimations.Length; i++)
            {
                PrimaryAnimations[i].Speed = 2.0f;
                PrimaryAnimations[i].Loop = false;

                PrimaryAnimations[i].OnAnimationMid += delegate
                {
                    Model.Human.Attack(Model.Human.DamageEquation * 0.85f);
                };
            }

            SecondaryAnimations = new Animation[1];
            SecondaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeDoubleAttack.dae");

            for (int i = 0; i < SecondaryAnimations.Length; i++)
            {
                SecondaryAnimations[i].Speed = 1.5f;
                SecondaryAnimations[i].Loop = false;
                SecondaryAnimations[i].OnAnimationEnd += delegate
                {
                    Model.Human.Attack(Model.Human.DamageEquation * 1.10f, delegate (Entity Mob)
                    {

                        if (Utils.Rng.Next(0, 3) == 1)
                            Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);

                        if (Utils.Rng.Next(0, 3) == 1)
                            Mob.AddComponent(new BleedingComponent(Mob, this.Model.Human, 4f,
                                Model.Human.DamageEquation * 2f));
                    });
                };
            }
        }

        public override void Update(HumanModel Model)
        {
            base.Update(Model);

            base.SetToDefault(this.Mesh);
            base.SetToDefault(this.SecondBlade);

            if (Sheathed)
            {
                Matrix4 Mat4 = Model.Model.MatrixFromJoint(Model.Chest).ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.ChestPosition);

                this.Mesh.Position = Model.Position;
                this.Mesh.BeforeLocalRotation = new Vector3(.4f, 0.75f, -1.2f);
                this.Mesh.TransformationMatrix = Mat4;
                this.Mesh.TargetRotation = new Vector3(0, 90, 135);

                this.SecondBlade.Position = Model.Position;
                this.SecondBlade.BeforeLocalRotation = new Vector3(-1.0f, 0.75f, -1.2f);
                this.SecondBlade.TransformationMatrix = Mat4;
                this.SecondBlade.TargetRotation = new Vector3(0, 270, 225);

            }

            if (base.InAttackStance || Model.Human.WasAttacking)
            {

                Matrix4 Mat4L = Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.LeftHandPosition);

                this.Mesh.TransformationMatrix = Mat4L;
                this.Mesh.Position = Model.Position;
                this.Mesh.TargetRotation = new Vector3(180, 0, -45);
                this.Mesh.BeforeLocalRotation = Vector3.UnitY * -0.35f;

                Matrix4 Mat4R = Model.RightHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.RightHandPosition);

                this.SecondBlade.TransformationMatrix = Mat4R;
                this.SecondBlade.Position = Model.Position;
                this.SecondBlade.TargetRotation = new Vector3(180, 90, 45);
                this.SecondBlade.BeforeLocalRotation = Vector3.UnitY * -0.35f;
            }

            if (PrimaryAttack || SecondaryAttack)
            {

                Matrix4 Mat4L = Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.LeftHandPosition);

                this.Mesh.TransformationMatrix = Mat4L;
                this.Mesh.Position = Model.Position;
                this.Mesh.TargetRotation = PrimaryAttack ? new Vector3(180, 0, -45) : new Vector3(180, 0, 0f);
                this.Mesh.BeforeLocalRotation = PrimaryAttack ? -Vector3.UnitY * .35f : Vector3.UnitY * -0.7f;

                Matrix4 Mat4R = Model.RightHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.RightHandPosition);

                this.SecondBlade.TransformationMatrix = Mat4R;
                this.SecondBlade.Position = Model.Position;
                this.SecondBlade.TargetRotation = PrimaryAttack ? new Vector3(180, 180, 45) : new Vector3(180, 180, 0f);
                this.SecondBlade.BeforeLocalRotation = PrimaryAttack ? -Vector3.UnitY * .35f : Vector3.UnitY * -0.7f;
            }

        }
    }
}