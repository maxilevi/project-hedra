using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    public interface IPhysicsComponent
    {
        event OnHitGroundEvent OnHitGround;
        bool UsePhysics { get; set; }
        float Falltime { get; }
        bool CanBePushed { get; set; }
        bool UseTimescale { get; set; }
        bool InFrontOfWall { get; }
        bool IsDrifting { get; }
        Vector3 TargetPosition { get; set; }
        float Timestep { get; }
        bool Renderable { get; }
        void Update();
        void ResetVelocity();
        void ExecuteTranslate(MoveCommand Command);
        void Translate(Vector3 Delta);
        void DeltaTranslate(Vector3 Delta);
        void DeltaTranslate(MoveCommand Command);
        void ProccessCommand(MoveCommand Command);
        void ResetFall();
        void Dispose();
        void Draw();
        Vector3 GravityDirection { get; set; }  
        float VelocityCap { get; set; }  
        Vector3 Velocity { get; set; }      
        bool HasFallDamage { get; set; }
        
        /// <summary>
        /// If collides with structures
        /// </summary>
        bool CanCollide { get; set; }
        /// <summary>
        /// If it pushes entities when moving
        /// </summary>
        bool PushAround { get; set; }
        /// <summary>
        /// If collides with other entities
        /// </summary>
        bool HasCollision { get; set; }
    }
}