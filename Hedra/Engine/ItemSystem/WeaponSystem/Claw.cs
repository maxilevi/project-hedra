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
    public class Claw : MeleeWeapon
    {
        private readonly ObjectMesh _secondBlade;

        public Claw(VertexData Contents) : base(Contents)
        {
            var baseMesh = Contents.Clone();
            baseMesh.Scale(Vector3.One * 1.75f);
            this._secondBlade = ObjectMesh.FromVertexData(baseMesh);
            this.RegisterWeapon(_secondBlade, baseMesh);

            AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/RogueBlade-Stance.dae");

            PrimaryAnimations = new Animation[2];
            PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeLeftAttack.dae");
            PrimaryAnimations[1] = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeRightAttack.dae");

            for (int i = 0; i < PrimaryAnimations.Length; i++)
            {
                PrimaryAnimations[i].Speed = 1.35f;
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
                SecondaryAnimations[i].Speed = 1.0f;
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
            base.SetToDefault(this._secondBlade);

            if (Sheathed)
            {
                Matrix4 Mat4 = Owner.Model.Model.MatrixFromJoint(Owner.Model.ChestJoint).ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);

                this.MainMesh.Position = Owner.Model.Position;
                this.MainMesh.BeforeLocalRotation = -Vector3.UnitX * 1.6f - Vector3.UnitY * 2f;
                this.MainMesh.TransformationMatrix = Mat4;
                this.MainMesh.TargetRotation = new Vector3(55 + 180, 0, 0);

                this._secondBlade.Position = Owner.Model.Position;
                this._secondBlade.BeforeLocalRotation = Vector3.UnitX * 1.0f - Vector3.UnitY * 2f;
                this._secondBlade.TransformationMatrix = Mat4;
                this._secondBlade.TargetRotation = new Vector3(-55 + 180, 180, 0);

            }

            if (base.InAttackStance || Owner.Model.Human.WasAttacking)
            {

                Matrix4 Mat4L = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);

                this.MainMesh.TransformationMatrix = Mat4L;
                this.MainMesh.Position = Owner.Model.Position;
                this.MainMesh.TargetRotation = new Vector3(180, 180, 0);
                this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.35f;

                Matrix4 Mat4R = Owner.Model.RightWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.RightWeaponPosition);

                this._secondBlade.TransformationMatrix = Mat4R;
                this._secondBlade.Position = Owner.Model.Position;
                this._secondBlade.TargetRotation = new Vector3(180, 0, 0);
                this._secondBlade.BeforeLocalRotation = Vector3.UnitY * -0.35f;
            }

            if (PrimaryAttack || SecondaryAttack)
            {

                Matrix4 Mat4L = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);

                this.MainMesh.TransformationMatrix = Mat4L;
                this.MainMesh.Position = Owner.Model.Position;
                this.MainMesh.TargetRotation = new Vector3(180, 180, 0f);
                this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;

                Matrix4 Mat4R = Owner.Model.RightWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.RightWeaponPosition);

                this._secondBlade.TransformationMatrix = Mat4R;
                this._secondBlade.Position = Owner.Model.Position;
                this._secondBlade.TargetRotation = new Vector3(180, 0, 0);
                this._secondBlade.BeforeLocalRotation = Vector3.UnitY * -0.7f;
            }
            base.ApplyEffects(_secondBlade);
        }
    }
}