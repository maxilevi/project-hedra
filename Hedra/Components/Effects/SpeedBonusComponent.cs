using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Components.Effects
{
    public class SpeedBonusComponent : EntityComponent
    {
        private readonly float _speedBonus;

        public SpeedBonusComponent(IEntity Parent, float Speed) : base(Parent)
        {
            _speedBonus = Speed;
             Parent.Speed += _speedBonus;
        }

        public override void Update()
        {
            if(Parent is Humanoid human && human.IsRiding || !Parent.IsMoving) return;
            if (_speedBonus > 0)
            {
                World.Particles.Color = Vector4.One;
                World.Particles.VariateUniformly = true;
                World.Particles.Position =
                    Parent.Position + Vector3.UnitY * Parent.Model.Height * .25f;
                World.Particles.Scale = Vector3.One * .25f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = -Parent.Orientation * .05f;
                World.Particles.ParticleLifetime = 0.25f;
                World.Particles.GravityEffect = 0.0f;
                World.Particles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
                World.Particles.Emit();
            }
            else if(_speedBonus < 0)
            {
                World.Particles.Color = new Vector4(.2f,.2f,.2f,.6f);
                World.Particles.VariateUniformly = true;
                World.Particles.Position = Parent.Position + Vector3.UnitY * Parent.Model.Height * .15f;
                World.Particles.Scale = Vector3.One * .25f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = -Parent.Orientation * .05f;
                World.Particles.ParticleLifetime = 0.25f;
                World.Particles.GravityEffect = .0f;
                World.Particles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
                World.Particles.Emit();
            }
        }

        public override void Dispose()
        {
            this.Parent.Speed -= _speedBonus;
        }
    }
}
