using System.Globalization;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Retreat : SingleAnimationSkill
    {
        private const float DefaultMultiplier = 100;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Retreat.png");   
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorJump.dae");
        public override bool CanBeCastedWhileAttacking => true;
        protected override bool ShouldCancel => !Player.IsJumping;
        private Vector3 _orientation;
        private Vector3 _accumulated;
        private float _multiplier;
        
        protected override void OnExecution()
        {
            var reached = _accumulated.LengthSquared > Distance * 5 * Distance * 5;
            _multiplier = Mathf.Lerp(_multiplier, reached ? 0 : DefaultMultiplier, Time.DeltaTime * 3f);
            var translation = _orientation * _multiplier;
            Player.Physics.DeltaTranslate(translation);
            _accumulated += translation * Time.DeltaTime;
            AddParticles();
        }

        protected override void DoUse()
        {
            Player.Movement.Orientate();
            Player.Movement.ForceJump(40);
            Player.Movement.CaptureMovement = false;
            _orientation = JumpDirection;
            _accumulated = Vector3.Zero;
            _multiplier = DefaultMultiplier;
            base.DoUse();
        }

        protected override void OnDisable()
        {
            Player.Movement.CaptureMovement = true;
            Player.Movement.CancelJump();
        }

        protected override void OnAnimationStart()
        {
            SoundPlayer.PlaySound(SoundType.SwooshSound, Player.Position);
        }

        private void AddParticles()
        {
            World.Particles.Color = Vector4.One;
            World.Particles.VariateUniformly = true;
            World.Particles.Position =
                Player.Position + Vector3.UnitY * Player.Model.Height * .25f;
            World.Particles.Scale = Vector3.One * .25f;
            World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            World.Particles.Direction = -Player.Orientation * .05f;
            World.Particles.ParticleLifetime = 0.25f;
            World.Particles.GravityEffect = 0.0f;
            World.Particles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
            World.Particles.Emit();
        }

        protected float Distance => .5f + Level * .5f;
        protected virtual Vector3 JumpDirection => -Player.LookingDirection.Xz.ToVector3().NormalizedFast();
        protected override int MaxLevel => 15;
        public override float MaxCooldown => 20 - Level * .5f;
        public override float ManaCost => 0;
        public override string Description => Translations.Get("retreat_desc");
        public override string DisplayName => Translations.Get("retreat_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("retreat_distance_change", Distance.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}