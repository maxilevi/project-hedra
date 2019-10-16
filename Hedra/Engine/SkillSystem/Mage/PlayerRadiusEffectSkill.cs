using Hedra.Core;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.SkillSystem.Mage
{
    public abstract class PlayerRadiusEffectSkill : RadiusEffectSkill<IPlayer>
    {     
    }
    
    public abstract class RadiusEffectSkill<T> : SingleAnimationSkill<T> 
        where T : class, ISkilledAnimableEntity
    {
        protected sealed override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/MageStaffGroundHit.dae");
        protected sealed override bool CanMoveWhileCasting => false;
        protected sealed override float AnimationSpeed => 1.5f;
        
        protected override void OnAnimationMid()
        {
            base.OnAnimationMid();
            World.HighlightArea(User.Position, HighlightColor, Radius, .5f);
            SpawnParticles();
            SoundPlayer.PlaySound(SoundType.GroundQuake, User.Position);
            SkillUtils.DoNearby(User, Radius, Apply);
        }


        private void SpawnParticles()
        {
            SpawnParticles(User.Position, Radius, HighlightColor);
        }
        
        public static void SpawnParticles(Vector3 Position, float Radius, Vector4 HighlightColor)
        {
            World.Particles.Color = HighlightColor;
            World.Particles.VariateUniformly = true;
            World.Particles.Position = Position;
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