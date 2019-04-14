using Hedra.Core;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage
{
    public abstract class RadiusEffectSkill : SingleAnimationSkill
    {
        protected sealed override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/MageStaffGroundHit.dae");
        protected sealed override bool CanMoveWhileCasting => false;
        protected sealed override float AnimationSpeed => 1.5f;
        
        protected override void OnAnimationEnd()
        {
            base.OnAnimationEnd();
            World.HighlightArea(Player.Position, HighlightColor, Radius, .5f);
            SpawnParticles();
            SoundPlayer.PlaySound(SoundType.GroundQuake, Player.Position);
            SkillUtils.DoNearby(Player, Radius, Apply);
        }
        
        
        private void SpawnParticles()
        {
            World.Particles.Color = HighlightColor;
            World.Particles.VariateUniformly = true;
            World.Particles.Position = Player.Position;
            World.Particles.GravityEffect = 0f;
            World.Particles.Scale = Vector3.One * .75f;
            World.Particles.ScaleErrorMargin = Vector3.One * .35f;
            World.Particles.PositionErrorMargin = Vector3.One * 2.5f;
            World.Particles.Shape = ParticleShape.Sphere;       
            World.Particles.ParticleLifetime = .05f * Radius;       
            for(var i = 0; i < 500; ++i)
            {
                var dir = new Vector3(Utils.Rng.NextFloat() * 2 - 1, 0f, Utils.Rng.NextFloat() * 2 - 1).NormalizedFast();
                World.Particles.Direction = dir * 2;
                World.Particles.Emit();
            }
        }
        
        protected abstract void Apply(IEntity Entity);
        protected abstract Vector4 HighlightColor { get; }
        protected abstract float Radius { get; }
    }
}