using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ModuleSystem.AnimationEvents;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.Sound;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using OpenTK;

namespace Hedra.AnimationEvents
{
    public class Quake : AnimationEvent
    {
        public Quake(ISkilledAnimableEntity Parent) : base(Parent) {}

        public override void Build()
        {
            var radius = 16 * Parent.Model.Scale.Average();
            var position = Parent.Position + Parent.Orientation * Parent.Model.Scale * 6f;

            World.Particles.VariateUniformly = true;
            World.Particles.GravityEffect = .25f;
            World.Particles.Scale = Vector3.One;
            World.Particles.ScaleErrorMargin = new Vector3(.25f, .25f, .25f);
            World.Particles.PositionErrorMargin = new Vector3(4f, .5f, 4f);
            World.Particles.Shape = ParticleShape.Sphere;
            World.Particles.ParticleLifetime = 1.5f;

            for (var i = 0; i < 125; i++)
            {
                World.Particles.Position = position + new Vector3(Utils.Rng.NextFloat() * 2f - 1f, 0, Utils.Rng.NextFloat() * 2f -1f) * radius * .5f;
                World.Particles.Direction = (Utils.Rng.NextFloat() * .5f + .5f) * Vector3.UnitY * 2f;
                World.Particles.Color = World.GetRegion(position).Colors.DirtColor;
                World.Particles.Emit();
            }
            World.HighlightArea(position, World.GetRegion(position).Colors.DirtColor * 1.5f, radius, 1.5f);

            var entities = World.Entities;
            foreach (var entity in entities)
            {
                if(!entity.IsGrounded) continue;
                var damage = Parent.AttackDamage * (1-Mathf.Clamp((position - entity.Position).Xz.LengthFast / radius, 0, 1)) * 3.0F;
                if (damage > 0 && Parent != entity)
                {
                    entity.Damage(damage, Parent, out _);
                }           
            }
            SoundPlayer.PlaySound(SoundType.GroundQuake, position, false, 1f, 5f);
        }
    }
}
