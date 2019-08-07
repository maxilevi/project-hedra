/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 02/05/2016
 * Time: 09:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using OpenTK;
using Bullet = BulletSharp;

namespace Hedra.Engine.BulletPhysics
{
    public delegate void OnMoveEvent();

    public class PhysicsComponent : EntityComponent, IPhysicsComponent
    {
        private const float NormalSpeedModifier = 2.25f;
        public event OnMoveEvent OnMove;
        public bool UsePhysics { get; set; } = true;
        public float FallTime { get; private set; }
        public bool CanBePushed { get; set; } = true;
        public bool HasFallDamage { get; set; } = true;
        public bool UseTimescale { get; set; } = true;

        private PhysicsComponentMotionState _motionState;
        private float _speedMultiplier;
        private readonly Bullet.RigidBody _body;
        
        public PhysicsComponent(IEntity Parent) : base(Parent)
        {
            var defaultShape = new Bullet.BoxShape(Vector3.One.Compatible());
            _motionState = new PhysicsComponentMotionState();
            _body = new Bullet.RigidBody(new Bullet.RigidBodyConstructionInfo(1, _motionState, defaultShape));
            BulletPhysics.Add(_body);
        }

        public void SetHitbox(Box Dimensions)
        {
            var previous = _body.CollisionShape;
            _body.CollisionShape = new Bullet.BoxShape(Dimensions.Size.Compatible() * .5f);
            previous.Dispose();
        }

        public Vector3 GravityDirection { get; set; } = new Vector3(0, -1f, 0);

        public Vector3 RigidbodyPosition => _body.WorldTransform.Origin.Compatible();

        public override void Update()
        {
            _speedMultiplier = Mathf.Lerp(_speedMultiplier, NormalSpeedModifier * (Parent.IsAttacking ? Parent.AttackingSpeedModifier : 1), Timestep * 2f);
        }
        
        public void ResetSpeed()
        {
            _speedMultiplier = NormalSpeedModifier;
        }

        public Vector3 MoveFormula(Vector3 Direction, bool ApplyReductions = true)
        {
            var movementSpeed = (Parent.IsUnderwater && !Parent.IsGrounded ? 1.25f : 1.0f) * Parent.Speed;
            return Direction * 5f * 1.75f * movementSpeed * (ApplyReductions ? _speedMultiplier : NormalSpeedModifier);
        }

        public void Move(float Scalar = 1)
        {
            DeltaTranslate(MoveFormula(Parent.Orientation * Scalar));
        }

        public void ResetVelocity()
        {
            _body.ClearForces();
        }

        public bool ExecuteTranslate(MoveCommand Command)
        {
            return ProcessCommand(Command);
        }

        public bool Translate(Vector3 Delta)
        {
            return ExecuteTranslate(new MoveCommand(Delta));
        }

        public bool DeltaTranslate(Vector3 Delta, bool OnlyY = false)
        {
            return ExecuteTranslate(new MoveCommand(Delta * Time.DeltaTime)
            {
                OnlyY = OnlyY
            });
        }
        
        private bool ProcessCommand(MoveCommand Command)
        {
            if (Command.Delta == Vector3.Zero) return true;
            _body.Activate(true);
            var previous = _body.WorldTransform.Origin;
            _body.Translate(Command.Delta.Compatible());
            return previous != _body.WorldTransform.Origin;
        }

        public bool EntityRaycast(IEntity[] Entities, Vector3 Addition, float Modifier = 1)
        {
            var success = false;
            EntityRaycast(Entities, Addition, E => { return success = true; }, Modifier);
            return success;
        }

        private void EntityRaycast(IEntity[] Entities, Vector3 Addition, Func<IEntity, bool> OnCollision, float Modifier = 1)
        {
            var chunkSpace = World.ToChunkSpace(Parent.Position + Addition);
            for (var i = Entities.Length - 1; i > -1; i--)
            {
                if (Entities[i] == Parent)
                    continue;
                /* Is a entity is farther than 2 chunks away, just skip it. */
                if ((World.ToChunkSpace(Entities[i].Position) - chunkSpace).LengthSquared > Chunk.Width * Chunk.Width)
                    continue;

                if (!Entities[i].Physics.UsePhysics) continue;
                if (!Entities[i].Physics.CollidesWithEntities) continue;
                var radii = (Parent.Model.Dimensions.Size.LengthFast + Entities[i].Model.Dimensions.Size.LengthFast) * Modifier;
                if (!((Entities[i].Position - Parent.Position + Addition).LengthSquared < radii * radii)) continue;
                if (!Physics.Collides(Entities[i].Model.BroadphaseBox, Parent.Model.BroadphaseBox) ||
                    !Physics.Collides(Entities[i].Model.BroadphaseCollider, Parent.Model.BroadphaseCollider))
                    continue;

                if (OnCollision.Invoke(Entities[i]))
                    return;
            }
        }

        public void ResetFall()
        {
            FallTime = 0.01f;
        }
        
        /// <summary>
        /// If collides with structures
        /// </summary>
        public bool CollidesWithStructures { get; set; } = true;
        /// <summary>
        /// If it pushes entities when moving
        /// </summary>
        public bool PushAround { get; set; } = true;
        /// <summary>
        /// If collides with other entities
        /// </summary>
        public bool CollidesWithEntities { get; set; } = true;
        
        public bool UpdateColliderList { get; set; }

        public bool Raycast(Vector3 End)
        {
            return false;
        }
        
        private float Timestep => Time.IndependentDeltaTime * (UseTimescale ? Time.TimeScale : 1);
        
        public bool CollidesWithOffset(Vector3 Offset)
        {

            return true;
        }
        
        public override void Dispose()
        {
            BulletPhysics.Remove(_body);
        }
    }
}