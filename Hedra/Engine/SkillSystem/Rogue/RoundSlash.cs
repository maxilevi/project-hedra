/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.SkillSystem.Rogue
{
    /// <summary>
    ///     Description of WeaponThrow.
    /// </summary>
    public class RoundSlash : SingleAnimationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/RoundSlash.png");

        protected override Animation SkillAnimation { get; } =
            AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeRoundAttack.dae");

        protected override float AnimationSpeed => 2.0f;
        protected override int MaxLevel => 20;
        public override float ManaCost => 75;
        public override float MaxCooldown => Math.Max(8, 16 - Level * .5f);
        private float Damage => User.DamageEquation * DamageMultiplier;
        private float DamageMultiplier => 1.0f + Level * 0.175f;

        public override string Description => Translations.Get("round_slash_desc");
        public override string DisplayName => Translations.Get("round_slash_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("round_slash_damage_change",
                (int)(DamageMultiplier * (SkillAnimation.Length / SkillAnimation.Speed) * 100f))
        };

        protected override void OnAnimationStart()
        {
            SoundPlayer.PlaySound(SoundType.SwooshSound, User.Position, false, 0.8f);
        }

        protected override void OnQuarterSecond()
        {
            for (var i = World.Entities.Count - 1; i > 0; i--)
            {
                if (!User.InAttackRange(World.Entities[i], 2)) continue;

                World.Entities[i].Damage(Damage, User, out var exp);
                User.XP += exp;
            }
        }

        protected override void OnExecution()
        {
            World.Particles.Color = Vector4.One;
            World.Particles.ParticleLifetime = 1f;
            World.Particles.GravityEffect = .0f;
            World.Particles.Direction = Vector3.Zero;
            World.Particles.Scale = Vector3.One * .15f;
            World.Particles.PositionErrorMargin = Vector3.One * 0.75f;

            World.Particles.Position = User.Model.LeftWeaponPosition;
            World.Particles.Emit();

            World.Particles.Position = User.Model.RightWeaponPosition;
            World.Particles.Emit();
        }
    }
}