using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    public class SpeedBonusComponent : EntityComponent
    {
        private readonly float _speedBonus;

        public SpeedBonusComponent(Entity Parent, float Speed) : base(Parent)
        {
            _speedBonus = Speed;
            Parent.Speed += _speedBonus;
        }

        public override void Update()
        {
            World.WorldParticles.Color = Vector4.One;
            World.WorldParticles.VariateUniformly = false;
            World.WorldParticles.Position =
                Parent.Position + Vector3.UnitY * (Parent.HitBox.Max.Y - Parent.HitBox.Min.Y) * .5f;
            World.WorldParticles.Scale = Vector3.One * .25f;
            World.WorldParticles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            World.WorldParticles.Direction = -Parent.Orientation * .05f;
            World.WorldParticles.ParticleLifetime = 0.25f;
            World.WorldParticles.GravityEffect = 0.0f;
            World.WorldParticles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
            World.WorldParticles.Emit();
        }

        public override void Dispose()
        {
            Parent.Speed -= _speedBonus;
        }
    }
}
