using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    public abstract class BaseUpdatableModel
    {
        public abstract Vector3[] Vertices { get; }
        public abstract CollisionShape[] Colliders { get; }
        public abstract CollisionShape BroadphaseCollider { get; }
        public abstract Box BroadphaseBox { get; }
        public abstract Box BaseBroadphaseBox { get; protected set; }
        public abstract bool IsAttacking { get; protected set; }
        public abstract bool IsWalking { get; protected set; }
        public abstract bool IsIdling { get; protected set; }
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
        public abstract Vector3 Rotation { get; set; }
        public abstract Vector3 Scale { get; set; }
        public abstract Vector3 TargetRotation { get; set; }
        public abstract void Update();
        public abstract void Idle();
        public abstract void Run();
        public abstract void Attack(Entity Victim);
        public abstract void Attack(Entity Victim, float RangeModifier);
        public abstract void Draw();
        public abstract void Dispose();
    }
}
