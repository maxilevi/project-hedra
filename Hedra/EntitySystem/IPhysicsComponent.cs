using BulletSharp;
using Hedra.Engine.Bullet;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;
using Vector3 = System.Numerics.Vector3;

namespace Hedra.EntitySystem
{
    public interface IPhysicsComponent
    {
        event OnMoveEvent OnMove;
        bool UsePhysics { get; set; }
        bool CanBePushed { get; set; }
        bool UseTimescale { get; set; }
        bool UpdateColliderList { get; set; }
        Vector3 MoveFormula(Vector3 Direction, bool ApplyReductions = true);
        void Move(float Scalar = 1);
        void Update();
        void SetHitbox(Box Dimensions);
        void ResetVelocity();
        bool Translate(Vector3 Delta);
        bool DeltaTranslate(Vector3 Delta);
        void ResetFall();
        void ResetSpeed();
        void Dispose();
        void Draw();
        Vector3 GravityDirection { get; set; }
        bool CollidesWithOffset(Vector3 Offset);
        void MoveTowards(Vector3 Position);
        void ApplyImpulse(Vector3 Impulse);
        Vector3 Impulse { get; }
        bool StaticRaycast(Vector3 End);
        bool CollidesWithStructures { get; set; }
        bool CollidesWithEntities { get; set; }
        Vector3 RigidbodyPosition { get; }
        bool IsStuck { get; }
        bool ContactResponse { get; set; }
        Vector3 LinearVelocity { get; }

    }
}