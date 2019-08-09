using BulletSharp;
using Hedra.Engine.BulletPhysics;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;
using Vector3 = OpenTK.Vector3;

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
        bool DeltaTranslate(Vector3 Delta, bool OnlyY = false);
        void ResetFall();
        void ResetSpeed();
        void Dispose();
        void Draw();
        Vector3 GravityDirection { get; set; }
        bool CollidesWithOffset(Vector3 Offset);
        void MoveTowards(Vector3 Position);
        bool Raycast(Vector3 End);
        bool EntityRaycast(IEntity[] Entities, Vector3 Length, float Modifier = 1);
        /// <summary>   
        /// If collides with structures
        /// </summary>
        bool CollidesWithStructures { get; set; }
        /// <summary>
        /// If it pushes entities when moving
        /// </summary>
        bool PushAround { get; set; }
        /// <summary>
        /// If collides with other entities
        /// </summary>
        bool CollidesWithEntities { get; set; }

        Vector3 RigidbodyPosition { get; }

    }
}