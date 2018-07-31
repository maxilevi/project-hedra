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
    public class DoubleBlades : MeleeWeapon
    {
        private readonly ObjectMesh SecondBlade;

        public DoubleBlades(VertexData Contents) : base(Contents)
        {
            VertexData BaseMesh = Contents.Clone();
            BaseMesh.Scale(Vector3.One * 1.75f);
            this.SecondBlade = ObjectMesh.FromVertexData(BaseMesh);
            this.RegisterWeapon(SecondBlade, BaseMesh);

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
                    Owner.Attack(Owner.DamageEquation * 0.85f);
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
                Matrix4 Mat4 = Human.Model.Model.MatrixFromJoint(Human.Model.ChestJoint).ClearTranslation() * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.ChestPosition);

                this.MainMesh.Position = Owner.Position;
                this.MainMesh.BeforeLocalRotation = new Vector3(.4f, 0.75f, -1.2f);
                this.MainMesh.TransformationMatrix = Mat4;
                this.MainMesh.TargetRotation = new Vector3(0, 90, 135);

                this.SecondBlade.Position = Owner.Position;
                this.SecondBlade.BeforeLocalRotation = new Vector3(-1.0f, 0.75f, -1.2f);
                this.SecondBlade.TransformationMatrix = Mat4;
                this.SecondBlade.TargetRotation = new Vector3(0, 270, 225);

            }

            if (base.InAttackStance || Owner.WasAttacking)
            {

                Matrix4 Mat4L = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.LeftWeaponPosition);

                this.MainMesh.TransformationMatrix = Mat4L;
                this.MainMesh.Position = Owner.Position;
                this.MainMesh.TargetRotation = new Vector3(180, 0, -45);
                this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.35f;

                Matrix4 Mat4R = Owner.Model.RightWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.RightWeaponPosition);

                this.SecondBlade.TransformationMatrix = Mat4R;
                this.SecondBlade.Position = Owner.Position;
                this.SecondBlade.TargetRotation = new Vector3(180, 90, 45);
                this.SecondBlade.BeforeLocalRotation = Vector3.UnitY * -0.35f;
            }

            if (PrimaryAttack || SecondaryAttack)
            {

                Matrix4 Mat4L = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.LeftWeaponPosition);

                this.MainMesh.TransformationMatrix = Mat4L;
                this.MainMesh.Position = Owner.Position;
                this.MainMesh.TargetRotation = PrimaryAttack ? new Vector3(180, 0, -45) : new Vector3(180, 0, 0f);
                this.MainMesh.BeforeLocalRotation = PrimaryAttack ? -Vector3.UnitY * .35f : Vector3.UnitY * -0.7f;

                Matrix4 Mat4R = Owner.Model.RightWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.RightWeaponPosition);

                this.SecondBlade.TransformationMatrix = Mat4R;
                this.SecondBlade.Position = Owner.Position;
                this.SecondBlade.TargetRotation = PrimaryAttack ? new Vector3(180, 180, 45) : new Vector3(180, 180, 0f);
                this.SecondBlade.BeforeLocalRotation = PrimaryAttack ? -Vector3.UnitY * .35f : Vector3.UnitY * -0.7f;
            }
            base.ApplyEffects(SecondBlade);
        }
    }
}