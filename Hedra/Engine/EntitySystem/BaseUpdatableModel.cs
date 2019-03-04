using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    public abstract class BaseUpdatableModel
    {
        public abstract CollisionShape[] Colliders { get; }
        public abstract CollisionShape BroadphaseCollider { get; }
        public abstract CollisionShape HorizontalBroadphaseCollider { get; }
        public abstract Box BroadphaseBox { get; }
        public abstract Box BaseBroadphaseBox { get; protected set; }
        public abstract Box Dimensions { get; protected set; }
        public abstract bool IsAttacking { get; protected set; }
        public abstract bool IsWalking { get; protected set; }
        public abstract bool IsMoving { get; protected set; }
        public abstract bool IsIdling { get; protected set; }
        public abstract bool Outline { get; set; }
        public abstract float Height { get; }
        public abstract float Alpha { get; set; }
        public abstract float AnimationSpeed { get; set; }
        public abstract bool Pause { get; set; }
        public abstract bool ApplyFog { get; set; }
        public abstract bool Enabled { get; set; }
        public abstract bool Disposed { get; protected set; }
        public abstract bool IsStatic { get; }
        public abstract Vector4 BaseTint { get; set; }
        public abstract Vector4 Tint { get; set; }
        public abstract Vector3 Position { get; set; }
        public abstract Vector3 LocalRotation { get; set; }
        public abstract Vector3 Scale { get; set; }
        public abstract Vector3 TargetRotation { get; set; }     
        public abstract Vector4 OutlineColor { get; set; }
        public abstract void Update();
        public abstract bool CanAttack(IEntity Victim, float RangeModifier);
        public abstract void Attack(IEntity Victim, float RangeModifier);
        public abstract void Dispose();
        public bool IsUndead { get; protected set; }
    }
}
