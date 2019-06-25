/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Globalization;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    /// <summary>
    /// Description of WeaponThrow.
    /// </summary>
    public class Fade : SingleAnimationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Fade.png");
        protected override int MaxLevel => 20;
        private float FadeTime => 14 + Level / (float) MaxLevel * 15f;
        public override float MaxCooldown => 28 - 8 * (Level / (float)MaxLevel) + FadeTime;
        public override float ManaCost => 110f - 30f * (Level / (float) MaxLevel);
        private bool _isFaded;
        private float _timeRemaining;

        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/RogueFade.dae");
        protected override float AnimationSpeed => 5f;
        
        public override void Update()
        {
            base.Update();
            if (!_isFaded) return;

            if (_timeRemaining < 0 || Level == 0)
            {
                End();
            }
            else
            {
                _timeRemaining -= Time.DeltaTime;
                ShowParticles();
            }
        }

        private void End()
        {
            User.Model.Outline = false;
            User.IsInvisible = false;
            _isFaded = false;
            User.AfterDamaging -= AfterDamaging;
        }
        
        private void Start()
        {
            _isFaded = true;
            _timeRemaining = FadeTime;
            User.Model.Outline = true;
            User.Model.OutlineColor = new Vector4(.15f, .15f, .15f, 1);
            User.IsInvisible = true;
            SoundPlayer.PlayUISound(SoundType.DarkSound, 1f, .25f);
            InvokeStateUpdated();
            User.AfterDamaging += AfterDamaging;
        }

        private void AfterDamaging(IEntity Victim, float Damage)
        {
            End();
        }
        
        protected override void OnAnimationEnd()
        {
            Start();
        }

        private void ShowParticles()
        {
            World.Particles.Color = new Vector4(.2f, .2f, .2f, .8f);
            World.Particles.VariateUniformly = true;
            World.Particles.Position =
                User.Position + Vector3.UnitY * User.Model.Height * .3f;
            World.Particles.Scale = Vector3.One * .25f;
            World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            World.Particles.Direction = -User.Orientation * .05f;
            World.Particles.ParticleLifetime = 1.0f;
            World.Particles.GravityEffect = 0.0f;
            World.Particles.PositionErrorMargin = new Vector3(1.25f, User.Model.Height * .3f, 1.25f);
            World.Particles.Emit();
        }

        public override string Description => Translations.Get("fade_desc");
        public override string DisplayName => Translations.Get("fade_skill");
        public override float IsAffectingModifier => _isFaded ? 1 : 0;
        public override string[] Attributes => new[]
        {
            Translations.Get("fade_time_change", FadeTime.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}