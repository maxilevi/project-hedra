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
    public class Claw : Weapon
    {
        public override bool IsMelee { get; protected set; } = true;
        private ObjectMesh SecondBlade;

        public Claw(VertexData Contents) : base(Contents)
        {
            VertexData BaseMesh = Contents.Clone();
            BaseMesh.Scale(Vector3.One * 1.75f);
            this.SecondBlade = ObjectMesh.FromVertexData(BaseMesh);

            AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/RogueBlade-Stance.dae");

            PrimaryAnimations = new Animation[2];
            PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeLeftAttack.dae");
            PrimaryAnimations[1] = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeRightAttack.dae");

            for (int i = 0; i < PrimaryAnimations.Length; i++)
            {
                PrimaryAnimations[i].Speed = 2.75f;
                PrimaryAnimations[i].Loop = false;

                PrimaryAnimations[i].OnAnimationMid += delegate
                {
                    Owner.Attack(Owner.DamageEquation * .8f);
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
                    Owner.Attack(Owner.DamageEquation * 1.10f, delegate (Entity Mob)
                    {

                        if (Utils.Rng.Next(0, 3) == 1)
                            Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);

                        if (Utils.Rng.Next(0, 3) == 1)
                            Mob.AddComponent(new BleedingComponent(Mob, this.Owner, 4f,
                                Owner.DamageEquation * 2f));
                    });
                };
            }
        }

        public override void Update(Humanoid Human)
        {
            base.Update(Human);

            base.SetToDefault(this.MainMesh);
            base.SetToDefault(this.SecondBlade);

            if (Sheathed)
            {
                Matrix4 Mat4 = Owner.Model.Model.MatrixFromJoint(Owner.Model.Chest).ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);

                this.MainMesh.Position = Owner.Model.Position;
                this.MainMesh.BeforeLocalRotation = -Vector3.UnitX * 1.6f - Vector3.UnitY * 2f;
                this.MainMesh.TransformationMatrix = Mat4;
                this.MainMesh.TargetRotation = new Vector3(55 + 180, 0, 0);

                this.SecondBlade.Position = Owner.Model.Position;
                this.SecondBlade.BeforeLocalRotation = Vector3.UnitX * 1.0f - Vector3.UnitY * 2f;
                this.SecondBlade.TransformationMatrix = Mat4;
                this.SecondBlade.TargetRotation = new Vector3(-55 + 180, 180, 0);

            }

            if (base.InAttackStance || Owner.Model.Human.WasAttacking)
            {

                Matrix4 Mat4L = Owner.Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftHandPosition);

                this.MainMesh.TransformationMatrix = Mat4L;
                this.MainMesh.Position = Owner.Model.Position;
                this.MainMesh.TargetRotation = new Vector3(180, 0, 0);
                this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.35f;

                Matrix4 Mat4R = Owner.Model.RightHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.RightHandPosition);

                this.SecondBlade.TransformationMatrix = Mat4R;
                this.SecondBlade.Position = Owner.Model.Position;
                this.SecondBlade.TargetRotation = new Vector3(180, 0, 0);
                this.SecondBlade.BeforeLocalRotation = Vector3.UnitY * -0.35f;
            }

            if (PrimaryAttack || SecondaryAttack)
            {

                Matrix4 Mat4L = Owner.Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftHandPosition);

                this.MainMesh.TransformationMatrix = Mat4L;
                this.MainMesh.Position = Owner.Model.Position;
                this.MainMesh.TargetRotation = new Vector3(180, 0, 0f);
                this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;

                Matrix4 Mat4R = Owner.Model.RightHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.RightHandPosition);

                this.SecondBlade.TransformationMatrix = Mat4R;
                this.SecondBlade.Position = Owner.Model.Position;
                this.SecondBlade.TargetRotation = new Vector3(180, 0, 0);
                this.SecondBlade.BeforeLocalRotation = Vector3.UnitY * -0.7f;
            }

        }
    }
}