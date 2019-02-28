/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Globalization;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Rogue
{
    /// <summary>
    /// Description of WeaponThrow.
    /// </summary>
    public class Shuriken : SingleAnimationSkill
    {
        private static readonly VertexData ShurikenData = AssetManager.PLYLoader("Assets/Items/Shuriken.ply", new Vector3(1, 2, 1));
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Shuriken.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/RogueShurikenThrow.dae");
        protected override bool EquipWeapons => false;
        protected override float AnimationSpeed => 1.25f;
        protected override int MaxLevel => 15;
        public override float ManaCost => 35;
        public override float MaxCooldown => Math.Max(8, 12 - base.Level * .5f);

        protected override void OnAnimationMid()
        {
            ShootShuriken();
        }

        protected void ShootShuriken()
        {
            ShootShuriken(Player.View.LookingDirection);
        }

        protected void ShootShuriken(Vector3 Direction)
        {
            ShootShuriken(Player, Direction, Damage, StunChance);
        }
        
        private static void ShootShuriken(IHumanoid Human, Vector3 Direction, float Damage, int KnockChance = -1)
        {
            var weaponData = ShurikenData.Clone().RotateZ(90);
            weaponData.Scale(Vector3.One * 1f);

            var startingLocation = Human.Model.LeftWeaponPosition;

            var weaponProj = new Projectile(Human, startingLocation, weaponData)
            {
                Propulsion = Direction * 2.25f,
                Lifetime = 5f
            };
            weaponProj.MoveEventHandler += delegate
            {
                weaponProj.Mesh.LocalRotation += Time.DeltaTime * Vector3.UnitY * 25f;
            };
            weaponProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
            {
                Hit.Damage(Damage, Human, out var exp);
                Human.XP += exp;
                if (KnockChance == -1) return;
                if(Utils.Rng.Next(0, KnockChance) == 0)
                    Hit.KnockForSeconds(3);
            };
            SoundPlayer.PlaySound(SoundType.BowSound, Human.Position, false,  1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
            World.AddWorldObject(weaponProj);
        }
        
        protected override void OnExecution()
        {
            World.Particles.Color = Vector4.One;
            World.Particles.ParticleLifetime = 1f;
            World.Particles.GravityEffect = .0f;
            World.Particles.Direction = Vector3.Zero;
            World.Particles.Scale = Vector3.One * .25f;
            World.Particles.Position = Player.Model.LeftWeaponPosition;
            World.Particles.PositionErrorMargin = Vector3.One * 0.75f;
            //World.Particles.Emit();            
        }

        private float Damage => 18 + Level * 1.5f;
        private int StunChance => Level > 5 ? 8 : -1;
        public override string Description => Translations.Get("shuriken_desc");
        public override string DisplayName => Translations.Get("shuriken_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("shuriken_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("shuriken_stun_change", StunChance < 0 ? 0 : (int)(1.0f / (float)StunChance * 100))
        };
    }
}