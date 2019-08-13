/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 02/05/2016
 * Time: 09:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;
using Bullet = BulletSharp;

namespace Hedra.Engine.BulletPhysics
{
    public delegate void OnMoveEvent();

    public class PhysicsComponent : EntityComponent, IPhysicsComponent
    {
        private const float NormalSpeedModifier = 2.25f;
        public event OnMoveEvent OnMove;
        public float FallTime { get; private set; }
        public bool CanBePushed { get; set; } = true;
        public bool HasFallDamage { get; set; } = true;
        public bool UseTimescale { get; set; } = true;

        private readonly PhysicsComponentMotionState _motionState;
        private readonly Bullet.RigidBody _body;
        private readonly Bullet.RigidBody _sensor;
        private Vector3 _gravityDirection;
        private float _speedMultiplier;
        private Vector3 _accumulatedMovement;
        private int _sensorContacts;

        public PhysicsComponent(IEntity Parent) : base(Parent)
        {
            _gravityDirection = -Vector3.UnitY;
            _motionState = new PhysicsComponentMotionState();
            var defaultShape = new Bullet.BoxShape(Vector3.One.Compatible());
            using (var bodyInfo = new Bullet.RigidBodyConstructionInfo(1, _motionState, defaultShape))
            {
                _body = new Bullet.RigidBody(bodyInfo);
                /* FIXME: Ugly */
                if (Parent is IPlayer)
                {
                    _body.CollisionFlags |= Bullet.CollisionFlags.CharacterObject;
                    _body.ActivationState = Bullet.ActivationState.DisableDeactivation;
                }
                BulletPhysics.Add(_body, Bullet.CollisionFilterGroups.CharacterFilter, Bullet.CollisionFilterGroups.AllFilter);
            }
            using (var bodyInfo = new Bullet.RigidBodyConstructionInfo(1, new Bullet.DefaultMotionState(), defaultShape))
            {
                _sensor = new Bullet.RigidBody(bodyInfo);
                _sensor.CollisionFlags |= Bullet.CollisionFlags.NoContactResponse;
                /* FIXME: Ugly */
                if (Parent is IPlayer)
                {
                    _body.ActivationState = Bullet.ActivationState.DisableDeactivation;
                }
                BulletPhysics.Add(_sensor, Bullet.CollisionFilterGroups.SensorTrigger, Bullet.CollisionFilterGroups.StaticFilter);
            }
            
            _motionState.OnUpdated += UpdateSensor;
            BulletPhysics.OnCollision += OnCollision;
            BulletPhysics.OnSeparation += OnSeparation;
        }

        private void OnCollision(Bullet.CollisionObject Object0, Bullet.CollisionObject Object1)
        {
             if (!ReferenceEquals(Object0, _sensor) && !ReferenceEquals(Object1, _sensor)) return;
            var other = ReferenceEquals(Object0, _sensor) ? Object1 : Object0;
            if (!ReferenceEquals(other, _body))
            {
                _sensorContacts++;
            }
        }
        
        private void OnSeparation(Bullet.CollisionObject Object0, Bullet.CollisionObject Object1)
        {
            if (!ReferenceEquals(Object0, _sensor) && !ReferenceEquals(Object1, _sensor)) return;
            var other = ReferenceEquals(Object0, _sensor) ? Object1 : Object0;
            if (!ReferenceEquals(other, _body))
            {
                _sensorContacts--;
            }
        }

        public void SetHitbox(Box Dimensions)
        {
            SetShape(_body, GetShapeForBox(Dimensions));
            var radius = Dimensions.Size.Xz.Length * .5f;
            SetShape(_sensor, new Bullet.BoxShape(radius, .5f, radius));
        }

        private static Bullet.CollisionShape GetShapeForBox(Box Dimensions)
        {
            var bodyShape = default(Bullet.CompoundShape);
            if (Dimensions.Size.Xz.LengthFast > Dimensions.Size.Y)
            {
                bodyShape = BulletPhysics.ShapeFrom(Dimensions);
            }
            else
            {
                bodyShape = new Bullet.CompoundShape();
                var capsule = new Bullet.CapsuleShape(Dimensions.Size.Xz.Length * .5f, Dimensions.Size.Y * .33f);
                bodyShape.AddChildShape(
                    Bullet.Math.Matrix.Translation(Bullet.Math.Vector3.UnitY * Dimensions.Size.Y * -.5f),
                    capsule
                );
            }
            return bodyShape;
        }

        private static void SetShape(Bullet.CollisionObject Body, Bullet.CollisionShape Shape)
        {
            var previous = Body.CollisionShape;
            try
            {
                Body.CollisionShape = Shape;
            }
            finally
            {
                previous.Dispose();
            }
        }

        public Vector3 GravityDirection
        {
            get => _gravityDirection;
            set
            {
                _gravityDirection = value;
                _body.Gravity = Gravity;
            }
        }

        public bool UsePhysics
        {
            get => _body.Gravity != Bullet.Math.Vector3.Zero;
            set => _body.Gravity = (value ? Gravity : Bullet.Math.Vector3.Zero);
        }

        private Bullet.Math.Vector3 Gravity => (BulletPhysics.Gravity * _gravityDirection).Compatible();
        public Vector3 RigidbodyPosition => _body.WorldTransform.Origin.Compatible();

        public override void Draw()
        {
            if (GameSettings.DebugPhysics)
            {
                BulletPhysics.DrawObject(_body.WorldTransform, _body.CollisionShape, Colors.Red);
                BulletPhysics.DrawObject(_sensor.WorldTransform, _sensor.CollisionShape, Colors.GreenYellow);
            }
        }

        public override void Update()
        {
            var deltaTime = Timestep;
            _speedMultiplier = Mathf.Lerp(_speedMultiplier, NormalSpeedModifier * (Parent.IsAttacking ? Parent.AttackingSpeedModifier : 1), deltaTime * 2f);
            HandleFallDamage(deltaTime);
            HandleIsMoving();
            Parent.IsGrounded = _sensorContacts > 0;
            _body.LinearVelocity = new Bullet.Math.Vector3(_accumulatedMovement.X, Math.Min(2, _body.LinearVelocity.Y), _accumulatedMovement.Z);
            _accumulatedMovement = Vector3.Zero;
        }

        private void UpdateSensor()
        {
            _sensor.WorldTransform = _motionState.WorldTransform;
        }

        public void MoveTowards(Vector3 Position)
        {
            _accumulatedMovement += Position;
        }

        private void HandleIsMoving()
        {
            if (_body.LinearVelocity.Compatible().Xz.LengthSquared > 1f)
            {
                OnMove?.Invoke();
            }
        }
        
        private void HandleFallDamage(float DeltaTime)
        {
            if (!Parent.IsGrounded)
            {
                if (!Parent.IsUnderwater)
                    FallTime += DeltaTime;
            }
            else if(FallTime > 0)
            {
                if (FallTime > 2.0f && HasFallDamage)
                {
                    if (!Parent.SearchComponent<DamageComponent>()?.Immune ?? true)
                    {
                        var fallTime = FallTime;
                        Executer.ExecuteOnMainThread(delegate
                        {
                            Parent.Damage(fallTime * 7.5f, null, out _, true);
                            Parent.KnockForSeconds(3f);
                        });
                    }
                }
                else if (FallTime > 1f)
                {
                    SoundPlayer.PlaySound(SoundType.HitGround, Parent.Position);
                }
                FallTime = 0;
            }
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
            MoveTowards(MoveFormula(Parent.Orientation * Scalar));
        }

        public void ResetVelocity()
        {
            _body.LinearVelocity = Bullet.Math.Vector3.Zero;
        }

        public bool Translate(Vector3 Delta)
        {
            return ProcessCommand(new MoveCommand(Delta));
        }

        public bool DeltaTranslate(Vector3 Delta, bool OnlyY = false)
        {
            return ProcessCommand(new MoveCommand(Delta * Time.DeltaTime)
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
            var moved = previous != _body.WorldTransform.Origin;
            if(moved && Command.Delta.Xz != Vector2.Zero)
                OnMove?.Invoke();
            return moved;
        }
        public bool CollidesWithOffset(Vector3 Offset)
        {
            return false; //BulletPhysics.Collides(Offset.Compatible(), _sensor);
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
            return BulletPhysics.Raycast(RigidbodyPosition.Compatible(), End.Compatible());
        }
        
        private float Timestep => Time.IndependentDeltaTime * (UseTimescale ? Time.TimeScale : 1);

        public override void Dispose()
        {
            _motionState.Dispose();
            BulletPhysics.Remove(_body);
            BulletPhysics.Remove(_sensor);
            BulletPhysics.OnCollision -= OnCollision;
            BulletPhysics.OnSeparation -= OnSeparation;
        }
    }
}