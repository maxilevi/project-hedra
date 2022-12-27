using System;
using System.Numerics;
using Hedra.Components.Effects;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.SkillSystem;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering.Particles;
using Hedra.Sound;

namespace Hedra.AnimationEvents;

public class Knockback : AnimationEvent
{
    private const int Radius = 32;
    
    public Knockback(ISkilledAnimableEntity Parent) : base(Parent)
    {
    }

    public override void Build()
    {
        var position = Parent.Position + Parent.Orientation * Parent.Model.Scale * 6f;
        var color = World.GetRegion(position).Colors.StoneColor * .05f;

        World.Particles.VariateUniformly = true;
        World.Particles.GravityEffect = .5f;
        World.Particles.Scale = Vector3.One;
        World.Particles.ScaleErrorMargin = new Vector3(.25f, .25f, .25f);
        World.Particles.PositionErrorMargin = new Vector3(4f, .5f, 4f);
        World.Particles.Shape = ParticleShape.Sphere;
        World.Particles.ParticleLifetime = 1.0f;

        for (var i = 0; i < 125; i++)
        {
            World.Particles.Position = position +
                                       new Vector3(Utils.Rng.NextFloat() * 2f - 1f, 0,
                                           Utils.Rng.NextFloat() * 2f - 1f) * Radius * .5f;
            World.Particles.Direction = (Utils.Rng.NextFloat() * .5f + .5f) * Vector3.UnitY * 2f;
            World.Particles.Color = World.GetRegion(position).Colors.StoneColor * .5f;
            World.Particles.Emit();
        }
        
        var entities = World.Entities;
        foreach (var entity in entities)
        {
            if (entity == Parent || entity.IsStatic) continue;
            if(!IsWithinRange(position, entity)) continue;
            entity.KnockForSeconds(3);
            entity.Physics.ApplyImpulse(Parent.Orientation * 512);
        }
        
        SoundPlayer.PlaySound(SoundType.GroundQuake, position, false, 1f, 5f);
    }

    private bool IsWithinRange(Vector3 Position, IEntity Entity)
    {
        var range = 1 - Mathf.Clamp((Position - Entity.Position).Xz().LengthFast() / Radius, 0, 1);
        return range < 0.005f && Vector3.Dot((Entity.Position - Parent.Position).NormalizedFast(), Parent.Orientation) > 0.9f;
    }
}