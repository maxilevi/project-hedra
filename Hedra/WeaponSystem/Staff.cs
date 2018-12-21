﻿using Hedra.Engine;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    public sealed class Staff : RangedWeapon
    {
        public override uint PrimaryAttackIcon => WeaponIcons.StaffPrimaryAttack;     
        public override uint SecondaryAttackIcon => WeaponIcons.StaffSecondaryAttack;
        
        protected override string AttackStanceName => "Assets/Chr/MageStaff-Stance.dae";
        protected override float PrimarySpeed => 0.9f;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/MageStaff-PrimaryAttack.dae"
        };
        protected override float SecondarySpeed => 1.0f;
        protected override string[] SecondaryAnimationsNames => new []
        {
            "Assets/Chr/MageStaff-SecondaryAttack.dae"
        };

        protected override Vector3 SheathedPosition => new Vector3(1.5f,-1.0f,-0.75f);
        protected override Vector3 SheathedRotation => new Vector3(-5,90,-125 );


        public Staff(VertexData Contents) : base(Contents)
        {  
        }

        protected override void Shoot(Vector3 Direction, AttackOptions Options, params IEntity[] ToIgnore)
        {
            Fireball.Create(
                Owner,
                Owner.Position + Vector3.UnitY * 4f,
                Direction,
                Owner.DamageEquation * Options.DamageModifier,
                ToIgnore
            );
        }

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.End) return;
            Firewave.Create(Owner, Owner.DamageEquation * 3 * Options.Charge, Options.Charge);
        }
        
        protected override void OnSheathed()
        {
            this.MainMesh.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition - Vector3.UnitY * .25f);
            this.MainMesh.Position = Owner.Model.Position;
            this.MainMesh.Rotation = this.SheathedRotation;
            this.MainMesh.BeforeRotation = this.SheathedPosition * this.Scale;
        }

        protected override void OnAttackStance()
        {
            var mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
                    
            this.MainMesh.TransformationMatrix = mat4;
            this.MainMesh.Position = Owner.Model.Position;
            this.MainMesh.LocalRotation = new Vector3(90,25,180);
            this.MainMesh.BeforeRotation = Vector3.Zero;     
        } 

        protected override void OnSecondaryAttack()
        {
            var mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() 
                       * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);   
            MainMesh.TransformationMatrix = mat4;
            MainMesh.Position = Owner.Model.Position;
            MainMesh.LocalRotation = new Vector3(90, 0, 180);
            MainMesh.BeforeRotation = Vector3.Zero;
        }
    }
}