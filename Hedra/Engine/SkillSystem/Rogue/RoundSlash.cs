/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Rogue
{
    /// <summary>
    /// Description of WeaponThrow.
    /// </summary>
    public class RoundSlash : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/RoundSlash.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeRoundAttack.dae");
        protected override float AnimationSpeed => 1.25f;
        
        public RoundSlash()
        {
            base.ManaCost = 80f;
            base.MaxCooldown = 8.5f;
        }

        protected override void OnAnimationStart()
        {
            SoundPlayer.PlaySound(SoundType.SwooshSound, Player.Position, false, 0.8f, 1f);
        }

        protected override void OnQuarterSecond()
        {
            for (var i = World.Entities.Count - 1; i > 0; i--)
            {
                if (!Player.InAttackRange(World.Entities[i])) continue;

                var dmg = Player.DamageEquation * .2f * 2f * (1 + base.Level * .1f);
                World.Entities[i].Damage(dmg, Player, out float exp, true);
                Player.XP += exp;
            }
        }

        protected override void OnExecution()
        {
            World.Particles.Color = Vector4.One;
            World.Particles.ParticleLifetime = 1f;
            World.Particles.GravityEffect = .0f;
            World.Particles.Direction = Vector3.Zero;
            World.Particles.Scale = Vector3.One * .15f;
            World.Particles.Position = Player.Model.LeftWeaponPosition;
            World.Particles.PositionErrorMargin = Vector3.One * 0.75f;
            World.Particles.Emit();
        }

        public override string Description => "Cast a special attack which damages surrounding enemies.";
        public override string DisplayName => "Round Slash";
    }
}