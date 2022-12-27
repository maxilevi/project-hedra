using System;
using System.Numerics;
using Hedra.Components.Effects;
using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using Hedra.WorldObjects;
using SixLabors.ImageSharp;

namespace Hedra.AnimationEvents;

public class Laser : AnimationEvent
{
    public Laser(ISkilledAnimableEntity Parent) : base(Parent)
    {
    }

    public override void Build()
    {
        var origin = Parent.Position + Parent.Orientation * 2f + Vector3.UnitY * 4f;
        const int count = 7;
        const float step = (float) (2 * Math.PI / count);
        for (var i = 0; i < count; ++i)
        {
            var rotation = Matrix4x4.CreateRotationY(i * step);
            var direction = Vector3.Transform(Parent.Orientation, rotation);
            var laser = new ParticleProjectile(Parent, origin)
            {
                Propulsion = direction * 4f,
                Color = Color.Red.AsVector4(),
                UseLight = true,
                ParticleLifetime = 1f,
                Size = Vector3.One * 0.5f,
            };
            laser.HitEventHandler += delegate(Projectile Projectile, IEntity Hit)
            {
                Hit.AddComponent(new BurningComponent(Hit, Parent, 4, Parent.AttackDamage * 1.5f));
                Hit.Damage(Parent.AttackDamage, Parent, out _);
            };
            World.AddWorldObject(laser);
        }
        SoundPlayer.PlaySoundWithVariation(SoundType.TeleportSound, Parent.Position);
    }

}