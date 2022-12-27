using System.Numerics;
using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Sound;
using Hedra.WorldObjects;
using SixLabors.ImageSharp;

namespace Hedra.AnimationEvents;

public class Spit : AnimationEvent
{
    public Spit(ISkilledAnimableEntity Parent) : base(Parent)
    {
    }

    public override void Build()
    {
        //Parent.LookAt(Victim);
        var direction = Parent.Orientation;//(Victim.Position - Parent.Position).NormalizedFast();
        var spit = new ParticleProjectile(Parent,
            Parent.Position + Parent.Orientation * 2f + Vector3.UnitY * 4f)
        {
            Propulsion = direction * 2f,
            Color = Color.LawnGreen.AsVector4() * .5f,
            UseLight = false,
            Size = Vector3.One * 2.5f,
        };
        spit.HitEventHandler += delegate(Projectile Projectile, IEntity Hit)
        {
            Hit.KnockForSeconds(3);
            Hit.Physics.ApplyImpulse(Parent.Orientation * 256);
            Hit.Damage(Parent.AttackDamage * 2, Parent, out _);
            Parent.AddBonusSpeedForSeconds(1.5f, 3);
        };
        World.AddWorldObject(spit);
        SoundPlayer.PlaySoundWithVariation(SoundType.BeetleSpitSound, Parent.Position);
    }
}