using System;
using System.Numerics;
using Hedra.Engine;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Numerics;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;

namespace Hedra.EntitySystem
{
    public static class EntityExtensions
    {
        public static bool IsLookingAt(this IHumanoid Parent, IHumanoid Target, float Angle)
        {
            var comparer = Target.LookingDirection;
            if (comparer == Vector3.Zero) comparer = Vector3.UnitZ;
            var dot = Vector3.Dot(Parent.LookingDirection, -comparer);
            return Math.Abs(dot) > Angle;
        }

        public static void LookAt(this IEntity Parent, IEntity Target)
        {
            var dir = (Target.Position - Parent.Position).Xz().NormalizedFast().ToVector3();
            Parent.Rotation = Physics.DirectionToEuler(dir);
            Parent.Orientation = dir;
        }

        public static bool IsNear(this IEntity Parent, IEntity Target, float Radius)
        {
            return (Parent.Position - Target.Position).LengthSquared() < Radius * Radius;
        }

        public static bool InAttackRange(this IEntity Parent, IEntity Target, float RadiusModifier = 1f)
        {
            var collider0 = Parent.Model.HorizontalBroadphaseCollider;
            var collider1 = Target.Model.HorizontalBroadphaseCollider;
            var radii = (collider0.BroadphaseRadius + collider1.BroadphaseRadius) * RadiusModifier;
            return (Target.Position - Parent.Position).LengthSquared() < radii * radii;
        }

        public static void ShowText(this IEntity Parent, string Text, Color FontColor, float FontSize,
            float Lifetime = 4.0f)
        {
            ShowText(Parent, Parent.Position, Text, FontColor, FontSize, Lifetime);
        }

        public static void ShowText(this IEntity Parent, Vector3 Position, string Text, Color FontColor, float FontSize,
            float Lifetime = 4.0f)
        {
            if (Parent.Model.Enabled)
            {
                var newLabel = new TextBillboard(Lifetime, Text, FontColor,
                    FontCache.GetBold(FontSize), Position)
                {
                    Vanish = true,
                    VanishSpeed = 4
                };
            }
        }

        public static float Distance(this IEntity Parent, Vector3 Position)
        {
            return (Parent.Position - Position).LengthFast();
        }

        public static float Distance(this IEntity Parent, IEntity Target)
        {
            return Parent.Distance(Target.Position);
        }

        public static void RemoveComponent<T>(this IEntity Parent) where T : IComponent<IEntity>
        {
            var component = Parent.SearchComponent<T>();
            if (component != null)
                Parent.RemoveComponent(component);
        }

        public static bool IsBoss(this IEntity Parent)
        {
            return Parent.SearchComponent<BossHealthBarComponent>() != null;
        }
    }
}