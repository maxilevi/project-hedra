/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/01/2017
 * Time: 05:32 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
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
    public delegate void OnModifyArrowEvent(Projectile Arrow);
        
    public class Bow : RangedWeapon
    {           
        public override uint PrimaryAttackIcon => WeaponIcons.BowPrimaryAttack;     
        public override uint SecondaryAttackIcon => WeaponIcons.BowSecondaryAttack;
        
        protected override string AttackStanceName => "Assets/Chr/ArcherShootStance.dae";
        protected override float PrimarySpeed => 0.9f;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/ArcherShoot.dae"
        };
        protected override float SecondarySpeed => 0.9f;
        protected override string[] SecondaryAnimationsNames => new []
        {
            "Assets/Chr/ArcherTripleShoot.dae"
        };
        
        protected override Vector3 SheathedPosition => new Vector3(1.5f,-0.0f,0.0f);
        protected override Vector3 SheathedRotation => new Vector3(-5,90,-125 );
        protected virtual float ArrowDamageModifier => 1.0f;
        
        private static readonly VertexData ArrowVertexData;
        private static readonly VertexData ArrowDataVertexData;
        private static readonly VertexData QuiverVertexData;
        
        static Bow()
        {
            ArrowVertexData = AssetManager.PLYLoader("Assets/Items/Arrow.ply", Vector3.One * 4f * 1.5f,
                Vector3.UnitX * .35f, Vector3.Zero);
            QuiverVertexData = AssetManager.PLYLoader("Assets/Items/Quiver.ply",
                Vector3.One * new Vector3(2.2f, 2.8f, 2.2f) * 1.5f);
            ArrowDataVertexData = AssetManager.PLYLoader("Assets/Items/Arrow.ply", Vector3.One * 5f, Vector3.Zero,
                new Vector3(-90, 0, 90) * Mathf.Radian);
        }
        
        public OnModifyArrowEvent BowModifiers;
        public float ArrowDownForce { get; set; }
        private readonly ObjectMesh _quiver;
        private readonly ObjectMesh _arrow;

        public Bow(VertexData Contents) : base(Contents)
        {
            _arrow = ObjectMesh.FromVertexData(ArrowVertexData);
            _quiver = ObjectMesh.FromVertexData(QuiverVertexData);
            _quiver.TargetPosition = this.SheathedPosition + new Vector3(.3f, -0.75f, -0.2f);
            _quiver.LocalRotationPoint = new Vector3(0, 0, _quiver.TargetPosition.Z);
            _quiver.TargetRotation = new Vector3(SheathedRotation.X, SheathedRotation.Y, SheathedRotation.Z+90);          
        }
        
        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.Mid) return;
            var player = Owner as IPlayer;
            var direction = player?.View.CrossDirection ?? Owner.Orientation;
            Shoot(direction, Options, player?.Pet?.Pet);
            TaskScheduler.After(
                150,
                () => Shoot(direction, Options, player?.Pet?.Pet)
            );
            TaskScheduler.After(
                300,
                () => Shoot(direction, Options, player?.Pet?.Pet)
            );
        }

        protected override void OnSheathed()
        {
            MainMesh.TransformationMatrix = 
                Owner.Model.ChestMatrix.ClearTranslation() 
                * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition - Vector3.UnitY * .25f);
            MainMesh.Position = Owner.Model.Position;
            MainMesh.LocalRotation = this.SheathedRotation;
            MainMesh.BeforeLocalRotation = this.SheathedPosition * this.Scale;
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

        public override void Update(IHumanoid Human)
        {
            base.Update(Human);
            SetQuiverPosition();
            SetArrowPosition();
        }

        private void SetQuiverPosition()
        {
            SetToDefault(_quiver);
            _quiver.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);
            _quiver.Position = Owner.Model.Position;
            _quiver.BeforeLocalRotation = (-Vector3.UnitY * 1.5f - Vector3.UnitZ * 1.8f) * this.Scale;
        }
        
        private void SetArrowPosition()
        {
            base.SetToDefault(_arrow);
            var arrowMat4 = Owner.Model.RightWeaponMatrix.ClearTranslation() 
                            * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.RightWeaponPosition);
            
            _arrow.TransformationMatrix = arrowMat4;
            _arrow.Position = Owner.Model.Position;
            _arrow.BeforeLocalRotation = Vector3.UnitZ * 0.5f;
            _arrow.Enabled = (base.InAttackStance || Owner.IsAttacking) && _quiver.Enabled; 
        }

        private Projectile AddModifiers(Projectile ArrowProj)
        {
            BowModifiers?.Invoke(ArrowProj);
            return ArrowProj;
        }

        protected override void Shoot(Vector3 Direction, AttackOptions Options, params IEntity[] ToIgnore)
        {

            var arrowProj = new Projectile(Owner, Owner.Model.LeftWeaponPosition + Owner.Model.Human.Orientation * 2 - Vector3.UnitY, ArrowDataVertexData)
            {
                Lifetime = 5f,
                Propulsion = Direction * 2f - Vector3.UnitY * ArrowDownForce,
                IgnoreEntities = ToIgnore
            };
            arrowProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
            {
                Hit.Damage(Owner.DamageEquation * ArrowDamageModifier * Options.DamageModifier, Owner, out var exp, true, false);
                Owner.XP += exp;
            };
            arrowProj.LandEventHandler += S => Owner.ProcessHit(false);
            arrowProj.HitEventHandler += (S,V) => Owner.ProcessHit(true);
            arrowProj = this.AddModifiers(arrowProj);
            SoundPlayer.PlaySound(SoundType.BowSound, Owner.Position, false,  1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
        }

        public override void Dispose()
        {
            base.Dispose();
            _quiver.Dispose();
            _arrow.Dispose();
        }
    }
}
