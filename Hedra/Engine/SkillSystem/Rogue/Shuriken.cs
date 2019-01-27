/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Core;
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
        protected override float AnimationSpeed => 1;

        public Shuriken()
        {
            base.ManaCost = 35f;
            base.MaxCooldown = 8.5f;
        }

        protected override void OnAnimationMid()
        {
            ShootShuriken(Player, Player.View.CrossDirection.NormalizedFast(), 20f * base.Level, 8);
        }

        protected static void ShootShuriken(IHumanoid Human, Vector3 Direction, float Damage, int KnockChance = -1){
            VertexData weaponData = ShurikenData.Clone();
            weaponData.Scale(Vector3.One * 1.75f);

            var startingLocation = Human.Model.LeftWeaponPosition + Human.Orientation * .5f +
                                   Vector3.UnitY * 2f;

            var weaponProj = new Projectile(Human, startingLocation, weaponData)
            {
                Propulsion = Direction * 2f,
                Lifetime = 5f
            };
            weaponProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit) {
                Hit.Damage(Damage, Human, out var exp);
                Human.XP += exp;
                if (KnockChance == -1) return;
                if(Utils.Rng.Next(0, KnockChance) == 0)
                    Hit.KnockForSeconds(3);
            };
            SoundPlayer.PlaySound(SoundType.BowSound, Human.Position, false,  1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
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
            World.Particles.Emit();            
        }
        
        public override string Description => "Throw a shuriken at your foes.";
        public override string DisplayName => "Shuriken";
    }
}