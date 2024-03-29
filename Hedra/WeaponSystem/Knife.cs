/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.WeaponSystem
{
    /// <summary>
    ///     Description of TwoHandedSword.
    /// </summary>
    public class Knife : MeleeWeapon
    {
        private static readonly VertexData SheathData;
        private readonly ObjectMesh _knifeSheath;
        private Vector3 _previousPosition;

        static Knife()
        {
            SheathData = AssetManager.PLYLoader("Assets/Items/KnifeSheath.ply", Vector3.One);
            SheathData.Transform(Matrix4x4.CreateRotationY(180f * Mathf.Radian));
            SheathData.Scale(new Vector3(3.75f, 1.8f, 2f));
        }

        public Knife(VertexData Contents) : base(Contents)
        {
            _knifeSheath = ObjectMesh.FromVertexData(SheathData);
        }

        public override uint PrimaryAttackIcon => WeaponIcons.KnifePrimaryAttack;
        public override uint SecondaryAttackIcon => WeaponIcons.KnifeSecondaryAttack;
        protected override string AttackStanceName => "Assets/Chr/ArcherShootStance.dae";
        protected override float PrimarySpeed => 2.0f;

        protected override string[] PrimaryAnimationsNames => new[]
        {
            "Assets/Chr/WarriorSlash-Left.dae",
            "Assets/Chr/WarriorSlash-Right.dae"
        };

        protected override float SecondarySpeed => 3.0f;

        protected override string[] SecondaryAnimationsNames => new[]
        {
            "Assets/Chr/WarriorLunge.dae"
        };

        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if (AttackEventType.Mid != Type) return;
            Owner.AttackSurroundings(Owner.DamageEquation * Options.DamageModifier * 1.75f, Options.IgnoreEntities,
                delegate(IEntity Mob)
                {
                    if (Utils.Rng.Next(0, 5) == 1)
                        Mob.AddComponent(new BleedingComponent(Mob, Owner, 3f,
                            Owner.DamageEquation * 2.0f));
                });
        }

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if (Type != AttackEventType.End) return;
            Owner.AttackSurroundings(Owner.DamageEquation * Options.DamageModifier * 1.75f, Options.IgnoreEntities,
                delegate(IEntity Mob)
                {
                    if (Utils.Rng.Next(0, 3) == 1 && Options.Charge > .75f)
                        Mob.AddComponent(new BleedingComponent(Mob, Owner, 4f,
                            Owner.DamageEquation * 2.0f));
                });
        }

        public override void Update(IHumanoid Human)
        {
            base.Update(Human);
            SetToDefault(MainMesh);

            if (Sheathed)
            {
                var Mat4 = Owner.Model.ChestMatrix.ClearTranslation() *
                           Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);

                MainMesh.Position = Owner.Model.Position;
                MainMesh.BeforeRotation = -Vector3.UnitX * 1.6f - Vector3.UnitY * 2f;
                MainMesh.TransformationMatrix = Mat4;
                MainMesh.LocalRotation = new Vector3(180, 0, 0);
            }

            if (InAttackStance || Owner.WasAttacking)
            {
                var Mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() *
                           Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);

                MainMesh.TransformationMatrix = Mat4;
                MainMesh.Position = Owner.Model.Position;
                MainMesh.LocalRotation = new Vector3(270, 0, 0);
                MainMesh.BeforeRotation = Vector3.UnitY * -0.1f - Vector3.UnitZ * .2f;
            }

            if (PrimaryAttack)
            {
                var Mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() *
                           Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);

                MainMesh.TransformationMatrix = Mat4;
                MainMesh.Position = Owner.Model.Position;
                MainMesh.LocalRotation = new Vector3(180, 0, 0);
                MainMesh.BeforeRotation = Vector3.UnitY * -0.7f;
            }

            if (SecondaryAttack)
            {
                var Mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() *
                           Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);

                MainMesh.TransformationMatrix = Mat4;
                MainMesh.Position = Owner.Model.Position;
                MainMesh.LocalRotation = new Vector3(180, 0, 0);
                MainMesh.BeforeRotation = Vector3.UnitY * -0.7f;

                if (_previousPosition != Owner.Model.Human.Position && Owner.Model.Human.IsGrounded)
                {
                    var underChunk = World.GetChunkAt(Owner.Model.Position);
                    World.Particles.VariateUniformly = true;
                    World.Particles.Color =
                        World.GetHighestBlockAt((int)Owner.Model.Human.Position.X, (int)Owner.Model.Human.Position.Z)
                            .GetColor(underChunk.Biome.Colors); // * new Vector4(.8f, .8f, 1.0f, 1.0f);
                    World.Particles.Position = Owner.Model.Human.Position - Vector3.UnitY;
                    World.Particles.Scale = Vector3.One * .5f;
                    World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                    World.Particles.Direction = (-Owner.Model.Human.Orientation + Vector3.UnitY * 2.75f) * .15f;
                    World.Particles.ParticleLifetime = 1;
                    World.Particles.GravityEffect = .1f;
                    World.Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);

                    if (World.Particles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
                        World.Particles.Color = underChunk.Biome.Colors.GrassColor;

                    for (var i = 0; i < 4; i++) World.Particles.Emit();
                }

                _previousPosition = Owner.Position;
            }

            SetToDefault(_knifeSheath);

            var knifeMat4 = Owner.Model.ChestMatrix.ClearTranslation() *
                            Matrix4x4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);

            _knifeSheath.Position = Owner.Model.Position;
            _knifeSheath.BeforeRotation = -Vector3.UnitX * 1.75f - Vector3.UnitY * 3.0f;
            _knifeSheath.TransformationMatrix = knifeMat4;
        }

        public override void Attack1(IHumanoid Human, AttackOptions Options)
        {
            if (!MeetsRequirements()) return;

            base.Attack1(Human, Options);
            TaskScheduler.After(.25f, () => Trail.Emit = true);
        }

        public override void Dispose()
        {
            base.Dispose();
            _knifeSheath.Dispose();
        }
    }
}