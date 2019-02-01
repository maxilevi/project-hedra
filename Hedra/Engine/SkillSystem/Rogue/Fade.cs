/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Rogue
{
    /// <summary>
    /// Description of WeaponThrow.
    /// </summary>
    public class Fade : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Fade.png");
        protected override int MaxLevel => 20;
        private float FadeTime => Math.Min(16, 6 + Level * .5f);
        public override float MaxCooldown => 28 - Level + FadeTime;
        public override float ManaCost => 110f;
        private bool _isFaded;
        private float _timeRemaining;

        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/RogueFade.dae");
        protected override float AnimationSpeed => 5f;
        
        public override void Update()
        {
            base.Update();
            if(!_isFaded) return;

            if (_timeRemaining > 0)
            {
                _timeRemaining -= Time.DeltaTime;
                ShowParticles();
            }
            else
            {
                UnFade();
            }
        }

        private void UnFade()
        {
            Player.Model.Alpha = 1;
            Player.Model.BaseTint = Vector4.Zero;
            Player.IsInvisible = false;
            _isFaded = false;
        }

        protected override void OnAnimationEnd()
        {
            _isFaded = true;
            _timeRemaining = FadeTime;
            Player.Model.Alpha = .5f;
            Player.Model.BaseTint = -new Vector4(.85f, .85f, .85f, 0);
            Player.IsInvisible = true;
            SoundPlayer.PlayUISound(SoundType.DarkSound, 1f, .25f);
        }

        private void ShowParticles()
        {
            World.Particles.Color = new Vector4(.2f, .2f, .2f, .8f);
            World.Particles.VariateUniformly = true;
            World.Particles.Position =
                Player.Position + Vector3.UnitY * Player.Model.Height * .3f;
            World.Particles.Scale = Vector3.One * .25f;
            World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            World.Particles.Direction = -Player.Orientation * .05f;
            World.Particles.ParticleLifetime = 1.0f;
            World.Particles.GravityEffect = 0.0f;
            World.Particles.PositionErrorMargin = new Vector3(1.25f, Player.Model.Height * .3f, 1.25f);
            World.Particles.Emit();
        }

        protected override void OnExecution()
        {
        }

        public override string Description => Translations.Get("fade_desc");
        public override string DisplayName => Translations.Get("fade_skill");
    }
}