/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/01/2017
 * Time: 05:32 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;

namespace Hedra.WeaponSystem
{
    public delegate void OnArrowEvent(Projectile Arrow);
        
    public class Bow : RangedWeapon
    {
        public override uint PrimaryAttackIcon => WeaponIcons.BowPrimaryAttack;     
        public override uint SecondaryAttackIcon => WeaponIcons.BowSecondaryAttack;
        
        protected override string AttackStanceName => "Assets/Chr/ArcherShootStance.dae";
        protected override string ChargeStanceName => "Assets/Chr/ArcherChargeStance.dae";
        protected override float PrimarySpeed => .85f;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/ArcherShoot.dae"
        };
        protected override float SecondarySpeed => 1.5f;
        protected override string[] SecondaryAnimationsNames => new []
        {
            "Assets/Chr/ArcherTripleShoot.dae"
        };
        
        protected override Vector3 SheathedPosition => new Vector3(1.5f,-0.0f,0.0f);
        protected override Vector3 SheathedRotation => new Vector3(-5,90,-125 );
        
        private static readonly VertexData QuiverVertexData;
        
        static Bow()
        {
            QuiverVertexData = AssetManager.PLYLoader("Assets/Items/Quiver.ply",
                Vector3.One * new Vector3(2.2f, 2.8f, 2.2f) * 1.5f);
        }
        
        public OnArrowEvent BowModifiers;
        public OnArrowEvent Hit;
        public OnArrowEvent Miss;
        public float ArrowDownForce { get; set; }
        private readonly ObjectMesh _quiver;
        private ObjectMesh _arrow;
        private string _lastAmmoName;

        public Bow(VertexData Contents) : base(Contents)
        {
            _quiver = ObjectMesh.FromVertexData(QuiverVertexData);
            _quiver.LocalPosition = this.SheathedPosition + new Vector3(.3f, -0.75f, -0.2f);
            _quiver.RotationPoint = new Vector3(0, 0, _quiver.LocalPosition.Z);
            _quiver.LocalRotation = new Vector3(SheathedRotation.X, SheathedRotation.Y, SheathedRotation.Z+90);
            //_arrow = ObjectMesh.FromVertexData(VertexData.Empty);
            _arrow = ObjectMesh.FromVertexData(RescaleArrow((ItemPool.Grab(ItemType.StoneArrow))?.Model)?.RotateZ(180) ?? VertexData.Empty);
        }
        
        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(AttackEventType.Mid != Type) return;
            var player = Owner as IPlayer;
            var direction = player?.View.LookingDirection ?? Owner.Orientation;
            var left = direction.Xz.PerpendicularLeft.ToVector3() + Vector3.UnitY * direction.Y;
            var right = direction.Xz.PerpendicularRight.ToVector3() + Vector3.UnitY * direction.Y;
            Shoot(right * .15f + direction * .85f, Options, player?.Pet?.Pet);
            TaskScheduler.After(.15f,
                () => Shoot(direction, Options, player?.Pet?.Pet)
            );
            TaskScheduler.After(.25f,
                () => Shoot(left * .15f + direction * .85f, Options, player?.Pet?.Pet)
            );
        }

        protected override void OnSheathed()
        {
            MainMesh.TransformationMatrix = 
                Owner.Model.ChestMatrix.ClearTranslation() 
                * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition - Vector3.UnitY * .25f);
            MainMesh.Position = Owner.Model.Position;
            MainMesh.Rotation = this.SheathedRotation;
            MainMesh.BeforeRotation = this.SheathedPosition * this.Scale;
        }

        protected override void OnPostAttackStance()
        {
            var mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() 
                           * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);                  
            MainMesh.TransformationMatrix = mat4;
            MainMesh.Position = Owner.Model.Position;
            MainMesh.LocalRotation = new Vector3(90,25,180);
            MainMesh.BeforeRotation = Vector3.Zero;   
        }
        
        protected override void OnChargeStance()
        {
            OnPostAttackStance();
            this.MainMesh.LocalRotation = new Vector3(45, 90, 135);
        }

        protected override void OnSecondaryAttack()
        {
            OnPostAttackStance();
            this.MainMesh.LocalRotation = new Vector3(45, 90, 135);
        }
        
        public override void Update(IHumanoid Human)
        {
            base.Update(Human);
            SetQuiverPosition();
            SetArrowPosition();
            //var newAmmo = ItemPool.Grab()//Human?.Inventory.Ammo;
            //if (_lastAmmoName != newAmmo?.Name)
            //{
            //    _arrow?.Dispose();
            //    _arrow = ObjectMesh.FromVertexData(RescaleArrow(newAmmo?.Model)?.RotateZ(180) ?? VertexData.Empty);
            //    _lastAmmoName = newAmmo?.Name;
           // }
        }

        private void SetQuiverPosition()
        {
            SetToDefault(_quiver);
            _quiver.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);
            _quiver.Position = Owner.Model.Position;
            _quiver.BeforeRotation = (-Vector3.UnitY * 1.5f - Vector3.UnitZ * 1.8f) * this.Scale;
        }
        
        private void SetArrowPosition()
        {
            base.SetToDefault(_arrow);
            var arrowMat4 = Owner.Model.RightWeaponMatrix.ClearTranslation() 
                            * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.RightWeaponPosition);
            
            _arrow.TransformationMatrix = arrowMat4;
            _arrow.Position = Owner.Model.Position;
            _arrow.BeforeRotation = Vector3.UnitZ * 0.5f;
            _arrow.Enabled = (base.InAttackStance || Owner.IsAttacking) && _quiver.Enabled; 
        }

        private Projectile AddModifiers(Projectile ArrowProj)
        {
            BowModifiers?.Invoke(ArrowProj);
            return ArrowProj;
        }

        protected override void Shoot(Vector3 Direction, AttackOptions Options, params IEntity[] ToIgnore)
        {
            //var currentAmmo = Owner.Inventory.Ammo;
            //if (currentAmmo == null) return;
            var currentAmmo = ItemPool.Grab(ItemType.StoneArrow);
            var arrowProj = new Projectile(Owner,
                Owner.Model.LeftWeaponPosition,
                RescaleArrow(currentAmmo.Model).RotateX(90)
            )
            {
                Lifetime = 5f,
                Propulsion = Direction * 2f - Vector3.UnitY * ArrowDownForce,
                IgnoreEntities = ToIgnore
            };
            arrowProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
            {
                Hit.Damage(Owner.DamageEquation * Options.DamageModifier, Owner, out var exp, true, false);
                Owner.XP += exp;
            };
            arrowProj.LandEventHandler += S =>
            {
                Miss?.Invoke(arrowProj);
                Owner.ProcessHit(false);
            };
            arrowProj.HitEventHandler += (S,V) =>
            {
                Hit?.Invoke(arrowProj);
                Owner.ProcessHit(true);
            };
            arrowProj = AddModifiers(arrowProj);
            SoundPlayer.PlaySound(SoundType.BowSound, Owner.Position, false,  1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
        }

        private VertexData RescaleArrow(VertexData Model)
        {
            if (Model == null) return null;
            var size = (Model.SupportPoint(Vector3.One) - Model.SupportPoint(-Vector3.One)).LengthFast;
            return Model.Clone().Scale(Vector3.One * 3.5f * .75f / size);
        }

        public override bool CanDoAttack1 => true;//Owner?.Inventory.Ammo != null;
        
        public override bool CanDoAttack2 => true;//Owner?.Inventory.Ammo != null;

        public override void Dispose()
        {
            base.Dispose();
            _quiver.Dispose();
            _arrow.Dispose();
        }
    }
}
