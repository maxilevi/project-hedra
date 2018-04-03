using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
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
            if(Parent is Humanoid && !(this.Parent as Humanoid).IsMoving) return;
            if (_speedBonus > 0)
            {
                World.WorldParticles.Color = Vector4.One;
                World.WorldParticles.VariateUniformly = true;
                World.WorldParticles.Position =
                    Parent.Position + Vector3.UnitY * (Parent.HitBox.Max.Y - Parent.HitBox.Min.Y) * .25f;
                World.WorldParticles.Scale = Vector3.One * .25f;
                World.WorldParticles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.WorldParticles.Direction = -Parent.Orientation * .05f;
                World.WorldParticles.ParticleLifetime = 0.25f;
                World.WorldParticles.GravityEffect = 0.0f;
                World.WorldParticles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
                World.WorldParticles.Emit();
            }
            else if(_speedBonus < 0)
            {
                World.WorldParticles.Color = new Vector4(.2f,.2f,.2f,.6f);
                World.WorldParticles.VariateUniformly = true;
                World.WorldParticles.Position = Parent.Position + Vector3.UnitY * (Parent.HitBox.Max.Y - Parent.HitBox.Min.Y) * .15f;
                World.WorldParticles.Scale = Vector3.One * .25f;
                World.WorldParticles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.WorldParticles.Direction = -Parent.Orientation * .05f;
                World.WorldParticles.ParticleLifetime = 0.25f;
                World.WorldParticles.GravityEffect = .0f;
                World.WorldParticles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
                World.WorldParticles.Emit();
            }
        }

        public override void Dispose()
        {
            this.Parent.Speed -= _speedBonus;
        }
    }
}
