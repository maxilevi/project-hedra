/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 02/05/2016
 * Time: 09:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using BulletSharp;
using BulletSharp.Math;
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
using CollisionShape = BulletSharp.CollisionShape;
using Vector3 = OpenTK.Vector3;

namespace Hedra.Engine.Bullet
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
        private readonly RigidBody _body;
        private readonly RigidBody _sensor;
        private readonly ClosestRayResultCallback _rayResult;
        private Vector3 _gravityDirection;
        private float _speedMultiplier;
        private Vector3 _accumulatedMovement;
        private Vector3 _impulse;
        private int _sensorContacts;
        private Vector3 _gravity;

        public PhysicsComponent(IEntity Parent) : base(Parent)
        {
            _gravityDirection = -Vector3.UnitY;
            _gravity = Gravity;
            _motionState = new PhysicsComponentMotionState();
            using (var bodyInfo = new RigidBodyConstructionInfo(1, _motionState, new BoxShape(Vector3.One.Compatible())))
            {
                _body = new RigidBody(bodyInfo);
                /* FIXME: Ugly */
                if (Parent is IPlayer)
                {
                    _body.CollisionFlags |= CollisionFlags.CharacterObject;
                    _body.ActivationState = ActivationState.DisableDeactivation;
                }
                _body.Friction = 1;
                BulletPhysics.Add(_body, new PhysicsObjectInformation
                {
                    Group = CollisionFilterGroups.CharacterFilter,
                    Mask = CollisionFilterGroups.AllFilter,
                    Entity = Parent,
                    Name = Parent.Name
                });
            }
            using (var bodyInfo = new RigidBodyConstructionInfo(1, new DefaultMotionState(), new BoxShape(Vector3.One.Compatible())))
            {
                _sensor = new RigidBody(bodyInfo);
                _sensor.CollisionFlags |= CollisionFlags.NoContactResponse;
                /* FIXME: Ugly */
                if (Parent is IPlayer)
                {
                    _sensor.ActivationState = ActivationState.DisableDeactivation;
                }
                BulletPhysics.Add(_sensor, new PhysicsObjectInformation
                {
                    Group = CollisionFilterGroups.SensorTrigger,
                    Mask = (CollisionFilterGroups.AllFilter & ~CollisionFilterGroups.SensorTrigger),
                    Entity = Parent,
                    IsSensor = true,
                    Name = $"'{Parent.Name}' sensor"
                });
                _sensor.Gravity = BulletSharp.Math.Vector3.Zero;
            }

            var from = BulletSharp.Math.Vector3.Zero;
            var to = BulletSharp.Math.Vector3.Zero;
            _rayResult = new ClosestRayResultCallback(ref from, ref to);
            _motionState.OnUpdated += UpdateSensor;
            BulletPhysics.OnCollision += OnCollision;
            BulletPhysics.OnSeparation += OnSeparation;
        }

        private void OnCollision(CollisionObject Object0, CollisionObject Object1)
        {
            if (!ReferenceEquals(Object0, _sensor) && !ReferenceEquals(Object1, _sensor)) return;
            var other = ReferenceEquals(Object0, _sensor) ? Object1 : Object0;
            if (!ReferenceEquals(other, _body))
            {
                _sensorContacts++;
            }
        }
        
        private void OnSeparation(CollisionObject Object0, CollisionObject Object1)
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
            SetShape(_sensor, new BoxShape(radius * .5f, .5f, radius * .5f));
        }

        private CollisionShape GetShapeForBox(Box Dimensions)
        {
            var bodyShape = default(CompoundShape);
            if (Dimensions.Size.Xz.LengthFast > Dimensions.Size.Y)
            {
                var radius = Dimensions.Size.Xz.Length * .5f * .5f;
                bodyShape = new CompoundShape();
                var capsule = new SphereShape(radius);
                bodyShape.AddChildShape(
                    Matrix.Translation(BulletSharp.Math.Vector3.UnitY * -radius),
                    capsule
                );
            }
            else
            {
                var radius = Dimensions.Size.Xz.Length * .25f;
                bodyShape = new CompoundShape();
                var capsule = new CapsuleShape(radius, Dimensions.Size.Y * .5f);
                bodyShape.AddChildShape(
                    Matrix.Translation(BulletSharp.Math.Vector3.UnitY * (-capsule.HalfHeight - radius)),
                    capsule
                );
            }
            return bodyShape;
        }

        private static void SetShape(CollisionObject Body, CollisionShape Shape)
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
                _gravity = Gravity;
            }
        }

        public bool UsePhysics
        {
            get => _gravity != Vector3.Zero;
            set => _gravity = (value ? Gravity : Vector3.Zero);
        }

        private Vector3 Gravity => (BulletPhysics.Gravity * _gravityDirection);
        public Vector3 RigidbodyPosition => _body.WorldTransform.Origin.Compatible();//Time.Paused ? _body.WorldTransform.Origin.Compatible() : _motionState.Position;

        public override void Draw()
        {
            if (GameSettings.DebugPhysics)
            {
                //BulletPhysics.DrawObject(_body.WorldTransform, _body.CollisionShape, Colors.Red);
                //BulletPhysics.DrawObject(_sensor.WorldTransform, _sensor.CollisionShape, Colors.GreenYellow);
            }
        }

        public override void Update()
        {
            var deltaTime = Timestep;
            _speedMultiplier = Mathf.Lerp(_speedMultiplier, NormalSpeedModifier * (Parent.IsAttacking ? Parent.AttackingSpeedModifier : 1), deltaTime * 2f);
            HandleFallDamage(deltaTime);
            HandleIsMoving();
            Parent.IsGrounded = _sensorContacts > 0;
            _body.Gravity = Parent.IsGrounded ? BulletSharp.Math.Vector3.Zero : _gravity.Compatible();
            _body.LinearVelocity = new BulletSharp.Math.Vector3(_accumulatedMovement.X, Math.Min(0, _body.LinearVelocity.Y), _accumulatedMovement.Z) + _impulse.Compatible();
            _body.Activate();
            _impulse *= (float) Math.Pow(0.25f, Time.DeltaTime * 5f);
            _accumulatedMovement = Vector3.Zero;
        }

        private void UpdateSensor()
        {
            _sensor.WorldTransform = _motionState.WorldTransform;
            _sensor.Activate();
        }

        public void MoveTowards(Vector3 Position)
        {
            _accumulatedMovement += Position;
            _sensor.Activate();
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
            _body.LinearVelocity = BulletSharp.Math.Vector3.Zero;
        }

        public bool Translate(Vector3 Delta)
        {
            return ProcessCommand(Delta);
        }

        public bool DeltaTranslate(Vector3 Delta)
        {
            return ProcessCommand(Delta * Time.DeltaTime);
        }
        
        private bool ProcessCommand(Vector3 Delta)
        {
            if (Delta == Vector3.Zero) return true;
            _body.Activate(true);
            var previous = _body.WorldTransform.Origin;
            _body.Translate(Delta.Compatible());
            var moved = previous != _body.WorldTransform.Origin;
            if(moved && Delta.Xz != Vector2.Zero)
                OnMove?.Invoke();
            return moved;
        }
        public bool CollidesWithOffset(Vector3 Offset)
        {
            BulletPhysics.ResetCallback(_rayResult);
            _rayResult.CollisionFilterMask = (int)CollisionFilterGroups.StaticFilter;
            var from = (RigidbodyPosition + Offset).Compatible() - BulletSharp.Math.Vector3.UnitY * 4;
            var to = from + BulletSharp.Math.Vector3.UnitY * 4;
            _rayResult.RayFromWorld = from;
            _rayResult.RayToWorld = to;
            BulletPhysics.Raycast(ref from, ref to, _rayResult);
            return _rayResult.HasHit;
        }

        public void ResetFall()
        {
            FallTime = 0.01f;
        }

        public void ApplyImpulse(Vector3 Impulse)
        {
            _impulse += Impulse;
        }

        public bool CollidesWithStructures { get; set; }

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
            BulletPhysics.RemoveAndDispose(_body);
            BulletPhysics.RemoveAndDispose(_sensor);
            BulletPhysics.OnCollision -= OnCollision;
            BulletPhysics.OnSeparation -= OnSeparation;
        }
    }
}