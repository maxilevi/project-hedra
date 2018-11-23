using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.ModuleSystem.AnimationEvents
{
    public class ChargeEffect
    {
        public static void Do(IEntity Entity)
        {
            World.Particles.Position = Entity.Position;
            World.Particles.Color = Vector4.One;
            World.Particles.Shape = ParticleShape.Sphere;
            World.Particles.Direction = Vector3.Zero;
            World.Particles.GravityEffect = 0;
            World.Particles.Scale = new Vector3(1.5f, 1.5f, 1.5f);
            World.Particles.ParticleLifetime = 2;
            World.Particles.PositionErrorMargin = Entity.Size * .5f;
            for (var i = 0; i < 4; i++)
            {
                World.Particles.Emit();
            }
        }
    }
}