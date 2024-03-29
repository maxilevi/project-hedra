/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/01/2017
 * Time: 05:32 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Linq;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;
using Hedra.WorldObjects;

namespace Hedra.WeaponSystem
{
    public class Bow : RangedWeapon
    {
        private static readonly VertexData QuiverVertexData;
        private readonly ObjectMesh _quiver;
        private ObjectMesh _arrow;
        private string _lastAmmoName;

        static Bow()
        {
            QuiverVertexData = AssetManager.PLYLoader("Assets/Items/Quiver.ply",
                Vector3.One * new Vector3(2.2f, 2.8f, 2.2f) * 1.5f);
        }

        public Bow(VertexData Contents) : base(Contents)
        {
            _quiver = ObjectMesh.FromVertexData(QuiverVertexData);
            _quiver.LocalPosition = SheathedPosition + new Vector3(.3f, -0.75f, -0.2f);
            _quiver.RotationPoint = new Vector3(0, 0, _quiver.LocalPosition.Z);
            _quiver.LocalRotation = new Vector3(SheathedRotation.X, SheathedRotation.Y, SheathedRotation.Z + 90);
            _arrow = ObjectMesh.FromVertexData(VertexData.Empty);
            Ammo = ItemPool.Grab(ItemType.StoneArrow);
        }

        public override uint PrimaryAttackIcon => WeaponIcons.BowPrimaryAttack;
        public override uint SecondaryAttackIcon => WeaponIcons.BowSecondaryAttack;

        protected override string AttackStanceName => "Assets/Chr/ArcherShootStance.dae";
        protected override string ChargeStanceName => "Assets/Chr/ArcherChargeStance.dae";
        protected override float PrimarySpeed => .85f;

        protected override string[] PrimaryAnimationsNames => new[]
        {
            "Assets/Chr/ArcherShoot.dae"
        };

        protected override float SecondarySpeed => 1.5f;

        protected override string[] SecondaryAnimationsNames => new[]
        {
            "Assets/Chr/ArcherTripleShoot.dae"
        };

        protected override Vector3 SheathedPosition => new Vector3(1.75f, -0.0f, 0.0f);
        protected override Vector3 SheathedRotation => new Vector3(-5, 90, -125);

        public Item Ammo { get; set; }
        public float ArrowDownForce { get; set; }
        protected override SoundType Sound => SoundType.BowSound;

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if (AttackEventType.Mid != Type) return;
            var player = Owner as IPlayer;
            var direction = player?.View.LookingDirection ?? Owner.Orientation;
            var left = direction.Xz().PerpendicularLeft().ToVector3() + Vector3.UnitY * direction.Y;
            var right = direction.Xz().PerpendicularRight().ToVector3() + Vector3.UnitY * direction.Y;
            Shoot(right * .15f + direction * .85f, Options, player?.Companion?.Entity);
            TaskScheduler.After(.15f,
                () => Shoot(direction, Options, player?.Companion?.Entity)
            );
            TaskScheduler.After(.25f,
                () => Shoot(left * .15f + direction * .85f, Options, player?.Companion?.Entity)
            );
        }

        protected override void OnSheathed()
        {
            MainMesh.TransformationMatrix =
                Owner.Model.ChestMatrix.ClearTranslation()
                * Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition + Vector3.UnitY * .35f);
            MainMesh.Position = Owner.Model.Position;
            MainMesh.Rotation = SheathedRotation;
            MainMesh.BeforeRotation = SheathedPosition * Scale;
        }

        protected override void OnPostAttackStance()
        {
            var mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation()
                       * Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
            MainMesh.TransformationMatrix = mat4;
            MainMesh.Position = Owner.Model.Position;
            MainMesh.LocalRotation = new Vector3(90, 25, 180);
            MainMesh.BeforeRotation = Vector3.Zero;
        }

        protected override void OnChargeStance()
        {
            OnPostAttackStance();
            MainMesh.LocalRotation = new Vector3(45, 90, 135);
        }

        protected override void OnSecondaryAttack()
        {
            OnPostAttackStance();
            MainMesh.LocalRotation = new Vector3(45, 90, 135);
        }

        public override void Update(IHumanoid Human)
        {
            base.Update(Human);
            SetQuiverPosition();
            SetArrowPosition();
            var newAmmo = Ammo;
            if (_lastAmmoName != newAmmo?.Name)
            {
                _arrow?.Dispose();
                _arrow = ObjectMesh.FromVertexData(RescaleArrow(newAmmo?.Model)?.RotateZ(180) ?? VertexData.Empty);
                _lastAmmoName = newAmmo?.Name;
            }
        }

        private void SetQuiverPosition()
        {
            SetToDefault(_quiver);
            _quiver.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() *
                                           Matrix4x4.CreateTranslation(
                                               -Owner.Model.Position + Owner.Model.ChestPosition);
            _quiver.Position = Owner.Model.Position;
            _quiver.BeforeRotation = (-Vector3.UnitY * 1.1f - Vector3.UnitZ * 2f) * Scale;
        }

        private void SetArrowPosition()
        {
            SetToDefault(_arrow);
            var arrowMat4 = Owner.Model.RightWeaponMatrix.ClearTranslation()
                            * Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.RightWeaponPosition);

            _arrow.TransformationMatrix = arrowMat4;
            _arrow.Position = Owner.Model.Position;
            _arrow.BeforeRotation = Vector3.UnitZ * 0.5f;
            _arrow.Enabled = (InAttackStance || Owner.IsAttacking) && _quiver.Enabled;
        }

        protected override Projectile Shoot(Vector3 Direction, AttackOptions Options, params IEntity[] ToIgnore)
        {
            var currentAmmo = ItemPool.Grab(ItemType.StoneArrow);
            var arrowProj = new Projectile(Owner,
                Owner.Model.LeftWeaponPosition,
                RescaleArrow(currentAmmo.Model).RotateX(90)
            )
            {
                Lifetime = 5f,
                Propulsion = Direction * 2f - Vector3.UnitY * ArrowDownForce,
                IgnoreEntities = ToIgnore.Concat(Options.IgnoreEntities).ToArray()
            };
            arrowProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
            {
                Hit.Damage(Owner.DamageEquation * Options.DamageModifier, Owner, out var exp, true, false);
                Owner.XP += exp;
            };
            AddModifiers(arrowProj);
            World.AddWorldObject(arrowProj);
            return arrowProj;
        }

        private VertexData RescaleArrow(VertexData Model)
        {
            if (Model == null) return null;
            var size = (Model.SupportPoint(Vector3.One) - Model.SupportPoint(-Vector3.One)).LengthFast();
            return Model.Clone().Scale(Vector3.One * 3f * .75f / size);
        }

        public override void Dispose()
        {
            base.Dispose();
            _quiver.Dispose();
            _arrow.Dispose();
        }
    }
}