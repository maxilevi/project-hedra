using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.EntitySystem
{
    public interface IPhysicsComponent
    {
        event OnHitGroundEvent OnHitGround;
        event OnMoveEvent OnMove;
        bool UsePhysics { get; set; }
        float Falltime { get; }
        bool CanBePushed { get; set; }
        bool UseTimescale { get; set; }
        bool InFrontOfWall { get; }
        bool IsDrifting { get; }
        bool IsOverAShape { get; }
        bool UpdateColliderList { get; set; }
        Vector3 TargetPosition { get; set; }
        Vector3 MoveFormula(Vector3 Direction, bool ApplyReductions = true);
        void Move(float Scalar = 1);
        void Update();
        void ResetVelocity();
        bool Translate(Vector3 Delta);
        bool DeltaTranslate(Vector3 Delta, bool OnlyY = false);
        bool DeltaTranslate(MoveCommand Command);
        void ResetFall();
        void ResetSpeed();
        void Dispose();
        void Draw();
        Vector3 GravityDirection { get; set; }  
        float VelocityCap { get; set; }  
        Vector3 Velocity { get; set; }
        bool Raycast(Vector3 Length);
        bool EntityRaycast(IEntity[] Entities, Vector3 Length);
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
    }
}