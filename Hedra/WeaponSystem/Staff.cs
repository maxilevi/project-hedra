﻿using System.Linq;
using System.Numerics;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;
using Hedra.WorldObjects;

namespace Hedra.WeaponSystem
{
    public sealed class Staff : RangedWeapon
    {
        public Staff(VertexData Contents) : base(Contents)
        {
        }

        public override uint PrimaryAttackIcon => WeaponIcons.StaffPrimaryAttack;
        public override uint SecondaryAttackIcon => WeaponIcons.StaffSecondaryAttack;

        protected override string AttackStanceName => "Assets/Chr/MageStaff-Stance.dae";
        protected override float PrimarySpeed => 1.1f;

        protected override string[] PrimaryAnimationsNames => new[]
        {
            "Assets/Chr/MageStaff-PrimaryAttack.dae"
        };

        protected override float SecondarySpeed => 1.0f;

        protected override string[] SecondaryAnimationsNames => new[]
        {
            "Assets/Chr/MageStaff-SecondaryAttack.dae"
        };

        protected override Vector3 SheathedPosition => new Vector3(1.5f, -1.0f, -0.75f);
        protected override Vector3 SheathedRotation => new Vector3(-5, 90, -125);
        protected override SoundType Sound => SoundType.SlashSound;

        protected override Projectile Shoot(Vector3 Direction, AttackOptions Options, params IEntity[] ToIgnore)
        {
            var fireball = Fireball.Create(
                Owner,
                Owner.Position + Vector3.UnitY * 6f,
                Direction,
                Owner.DamageEquation * Options.DamageModifier,
                ToIgnore.Concat(Options.IgnoreEntities).ToArray()
            );
            AddModifiers(fireball);
            return fireball;
        }

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if (Type != AttackEventType.End) return;
            Firewave.Create(Owner, Owner.DamageEquation * 3.5f * Options.Charge, Options.Charge,
                Options.IgnoreEntities);
        }

        protected override void OnSheathed()
        {
            MainMesh.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() *
                                            Matrix4x4.CreateTranslation(-Owner.Model.Position +
                                                Owner.Model.ChestPosition - Vector3.UnitY * .25f);
            MainMesh.Position = Owner.Model.Position;
            MainMesh.Rotation = SheathedRotation;
            MainMesh.BeforeRotation = SheathedPosition * Scale;
        }

        protected override void OnAttackStance()
        {
            var mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() *
                       Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);

            MainMesh.TransformationMatrix = mat4;
            MainMesh.Position = Owner.Model.Position;
            MainMesh.LocalRotation = new Vector3(90, 25, 180);
            MainMesh.BeforeRotation = Vector3.Zero;
        }

        protected override void OnSecondaryAttack()
        {
            var mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation()
                       * Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
            MainMesh.TransformationMatrix = mat4;
            MainMesh.Position = Owner.Model.Position;
            MainMesh.LocalRotation = new Vector3(90, 0, 180);
            MainMesh.BeforeRotation = Vector3.Zero;
        }
    }
}