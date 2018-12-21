using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Game;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.EntitySystem
{
    public static class EntityExtensions
    {
        public static void RotateTowards(this IEntity Parent, IEntity Target)
        {
            var dir = (Target.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
            Parent.Rotation = Physics.DirectionToEuler(dir);
            Parent.Orientation = dir;
        }

        public static bool IsNear(this IEntity Parent, IEntity Target, float Radius)
        {
            return (Parent.Position - Target.Position).LengthSquared < Radius * Radius;
        }
               
        public static bool InAttackRange(this IEntity Parent, IEntity Target, float RadiusModifier = 1f)
        {
            var collider0 = Parent.Model.HorizontalBroadphaseCollider;
            var collider1 = Target.Model.HorizontalBroadphaseCollider;
            var radii = (collider0.BroadphaseRadius + collider1.BroadphaseRadius) * RadiusModifier;
            return (Target.Position - Parent.Position).LengthSquared < radii * radii;
        }
    }
}