using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ModuleSystem.AnimationEvents;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using OpenTK;

namespace Hedra.AnimationEvents
{
    public class Slash : AnimationEvent
    {
        public Slash(ISkilledAnimableEntity Parent) : base(Parent) {}

        public override void Build()
        {
            var position = Parent.Position + Parent.Orientation * 2;
            World.Particles.VariateUniformly = true;
            World.Particles.GravityEffect = .25f;
            World.Particles.Scale = Vector3.One;
            World.Particles.ScaleErrorMargin = new Vector3(.25f, .25f, .25f);
            World.Particles.PositionErrorMargin = new Vector3(4f, .5f, 4f);
            World.Particles.Shape = ParticleShape.Sphere;
            World.Particles.ParticleLifetime = 1.5f;
            World.Particles.Position = position + new Vector3(Utils.Rng.NextFloat() * 2f - 1f, 0, Utils.Rng.NextFloat() * 2f - 1f) * 24f;
            World.Particles.Direction = (Utils.Rng.NextFloat() * .5f + .5f) * Vector3.UnitY * 2f;
            World.Particles.Color = Vector4.One;
            World.Particles.Emit();

            var entities = World.Entities;
            for (var i = entities.Count - 1; i > -1; i--)
            {
                if (Parent == entities[i] || !Parent.InAttackRange(entities[i], 2.0f)) continue;
                entities[i].Damage(Parent.AttackDamage, Parent, out _);
            }
            SoundPlayer.PlaySound(SoundType.SlashSound, position, false, 1f, 5f);
        }
    }
}