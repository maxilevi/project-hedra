/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 02/05/2016
 * Time: 09:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Linq;
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
using System.Numerics;
using Hedra.Numerics;
using Bullet = BulletSharp;
using CollisionShape = BulletSharp.CollisionShape;
using Vector3 = System.Numerics.Vector3;

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
        public bool IsStuck => _isStuck;
        public Vector3 Impulse { get; private set; }
        
        private readonly PhysicsComponentMotionState _motionState;
        private readonly RigidBody _body;
        private readonly RigidBody _sensor;
        private readonly AllHitsRayResultCallback _rayResult;
        private readonly PhysicsObjectInformation _mainInformation;
        private readonly PhysicsObjectInformation _sensorInformation;
        private Vector3 _gravityDirection;
        private float _speedMultiplier;
        private Vector3 _accumulatedMovement;
        private int _sensorContacts;
        private Vector3 _gravity;
        private bool _moved;
        private bool _isStuck;
        private bool _usePhysics;
        private Vector3 _movedDistance;

        public PhysicsComponent(IEntity Parent) : base(Parent)
        {
            _usePhysics = true;
            _gravityDirection = -Vector3.UnitY;
            _gravity = Gravity;
            using (var bodyInfo = new RigidBodyConstructionInfo(1, _motionState = new PhysicsComponentMotionState(), new BoxShape(Vector3.One.Compatible())))
            {
                _body = new RigidBody(bodyInfo);
                /* FIXME: Ugly */
                if (Parent is IPlayer)
                {
                    _body.CollisionFlags |= CollisionFlags.CharacterObject;
                    _body.CcdMotionThreshold = 1e-16f;
                    _body.CcdSweptSphereRadius = 4f;
                }
                _body.ActivationState = ActivationState.DisableDeactivation;
                _body.Friction = 1;
                BulletPhysics.Add(_body, _mainInformation = new PhysicsObjectInformation
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
                //if (Parent is IPlayer)
                {
                    _sensor.ActivationState = ActivationState.DisableDeactivation;
                }
                BulletPhysics.Add(_sensor, _sensorInformation = new PhysicsObjectInformation
                {
                    Group = CollisionFilterGroups.SensorTrigger,
                    Mask = (CollisionFilterGroups.AllFilter & ~CollisionFilterGroups.SensorTrigger),
                    Entity = Parent,
                    Name = $"'{Parent.Name}' sensor"
                });
                _sensor.Gravity = BulletSharp.Math.Vector3.Zero;
            }

            var from = BulletSharp.Math.Vector3.Zero;
            var to = BulletSharp.Math.Vector3.Zero;
            _rayResult = new AllHitsRayResultCallback(from, to);
            _motionState.OnUpdated += UpdateSensor;
            BulletPhysics.OnRigidbodyReAdded += OnRigidbodyReAdded;
            BulletPhysics.OnCollision += OnCollision;
            BulletPhysics.OnSeparation += OnSeparation;
        }

        private void OnRigidbodyReAdded(RigidBody Body)
        {
            if(ReferenceEquals(Body, _body))
                UpdateGravity();
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
            var radius = Dimensions.Size.Xz().Length() * .5f;
            if (radius < 0.25f) throw new ArgumentOutOfRangeException();
            SetShape(_sensor, new BoxShape(radius * .5f, .5f, radius * .5f));
        }

        private CollisionShape GetShapeForBox(Box Dimensions)
        {
            var bodyShape = default(CompoundShape);
            if (Dimensions.Size.Xz().LengthFast() > Dimensions.Size.Y)
            {
                var radius = Dimensions.Size.Xz().Length() * .5f * .5f;
                bodyShape = new CompoundShape();
                if (radius < 0.25f) throw new ArgumentOutOfRangeException();
                var capsule = new SphereShape(radius);
                bodyShape.AddChildShape(
                    Matrix.Translation(BulletSharp.Math.Vector3.UnitY * -radius),
                    capsule
                );
            }
            else
            {
                var radius = Dimensions.Size.Xz().Length() * .25f;
                bodyShape = new CompoundShape();
                if (radius < 0.25f) throw new ArgumentOutOfRangeException();
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
                UpdateGravity();
            }
        }

        public bool ContactResponse
        {
            get => (_body.CollisionFlags & CollisionFlags.NoContactResponse) == 0;
            set
            {
                if (value)
                    _body.CollisionFlags &= ~CollisionFlags.NoContactResponse;
                else
                    _body.CollisionFlags |= CollisionFlags.NoContactResponse;
            }
        }

        public bool CollidesWithEntities
        {
            get => (_mainInformation.Mask & CollisionFilterGroups.CharacterFilter) == CollisionFilterGroups.CharacterFilter;
            set
            {
                if (value)
                {
                    _mainInformation.Mask |= CollisionFilterGroups.CharacterFilter;
                    _sensorInformation.Mask |= CollisionFilterGroups.CharacterFilter;
                }
                else
                {
                    _mainInformation.Mask &= ~CollisionFilterGroups.CharacterFilter;
                    _sensorInformation.Mask &= ~CollisionFilterGroups.CharacterFilter;
                }

                BulletPhysics.ApplyMaskChanges(_body);
                BulletPhysics.ApplyMaskChanges(_sensor);
            }
    }

        public bool UsePhysics
        {
            get => _usePhysics;
            set
            {
                _usePhysics = value;
                _gravity = Gravity;
                UpdateGravity();
            }
        }

        private Vector3 Gravity => (BulletPhysics.Gravity * _gravityDirection) * (_usePhysics ? 1 : 0);
        public Vector3 RigidbodyPosition => !_body.IsDisposed ? _body.WorldTransform.Origin.Compatible() : Vector3.Zero;

        public override void Draw()
        {
            if (GameSettings.DebugPhysics && Parent is LocalPlayer)
            {
                //BulletPhysics.DrawSimulated();
            }
        }

        public override void Update()
        {
            var deltaTime = Timestep;
            _speedMultiplier = Mathf.Lerp(_speedMultiplier, NormalSpeedModifier * (Parent.IsAttacking ? Parent.AttackingSpeedModifier : 1), deltaTime * 2f);
            if(_mainInformation.IsInSimulation)
                HandleFallDamage(deltaTime);
            HandleIsMoving();
            HandleIsStuck();
            Parent.IsGrounded = _sensorContacts > 0;
            UpdateGravity();
            _body.LinearVelocity = ContactResponse 
                ? new BulletSharp.Math.Vector3(_accumulatedMovement.X, Math.Min(0, _body.LinearVelocity.Y), _accumulatedMovement.Z) + Impulse.Compatible() * (UseTimescale ? Time.TimeScale : 1f)
                : BulletSharp.Math.Vector3.Zero;
            //_body.Activate();
            Impulse *= (float) Math.Pow(0.25f, deltaTime * 5f);
            _accumulatedMovement = Vector3.Zero;
        }

        private void UpdateGravity()
        {
            _body.Gravity = Parent.IsGrounded ? BulletSharp.Math.Vector3.Zero : _gravity.Compatible();
        }

        private void HandleIsStuck()
        {
            if (!(Parent is LocalPlayer))
            {
                _isStuck = (_moved && _body.LinearVelocity.Compatible().Xz().LengthSquared() < 4f) && !Parent.IsKnocked;
                _moved = false;
            }
        }

        private void UpdateSensor()
        {
            _sensor.WorldTransform = _motionState.WorldTransform;
            _sensor.Activate();
        }

        public void MoveTowards(Vector3 Position)
        {
            if(_sensor.IsDisposed) return;
            _accumulatedMovement += Position;
            _sensor.Activate();
            _movedDistance = Position;
            _moved = _movedDistance.Length() > float.Epsilon;
        }

        private void HandleIsMoving()
        {
            if (_body.LinearVelocity.Compatible().Xz().LengthSquared() > 1f)
            {
                OnMove?.Invoke();
            }
        }
        
        private void HandleFallDamage(float DeltaTime)
        {
            if (!ContactResponse) return;
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
                            Parent.Damage(fallTime * 12.5f, null, out _, true);
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
            if(moved && Delta.Xz() != Vector2.Zero)
                OnMove?.Invoke();
            return moved;
        }
        public bool CollidesWithOffset(Vector3 Offset)
        {
            bool DoRaycast(Vector3 From)
            {
                BulletPhysics.ResetCallback(_rayResult);
                /*
                 * We don't include the terrain in the raycast hit
                 * But we include the sensors so we get more accuracy on where the other characters are
                 */
                _rayResult.CollisionFilterMask = (int)(CollisionFilterGroups.StaticFilter | CollisionFilterGroups.CharacterFilter | CollisionFilterGroups.SensorTrigger);
                var from = From.Compatible() - BulletSharp.Math.Vector3.UnitY * 4;
                var to = from + BulletSharp.Math.Vector3.UnitY * 4;
                _rayResult.RayFromWorld = from;
                _rayResult.RayToWorld = to;
                BulletPhysics.Raycast(ref from, ref to, _rayResult);
                return _rayResult.CollisionObjects.Count(C => !ReferenceEquals(C, _sensor) && !ReferenceEquals(C, _body)) > 0;
            }
            
            _body.CollisionShape.GetAabb(Matrix.Identity, out var aabbMin, out var aabbMax);
            var position = Offset + RigidbodyPosition;
            var aabbMinXz = aabbMin.Compatible().Xz().ToVector3();
            var aabbMaxXz = aabbMax.Compatible().Xz().ToVector3();
            return DoRaycast(aabbMinXz * .5f + position)
                   || DoRaycast(new Vector3(aabbMinXz.X, 0, aabbMaxXz.Z) * .5f + position)
                   || DoRaycast(new Vector3(aabbMaxXz.X, 0, aabbMinXz.Z) * .5f + position)
                   || DoRaycast(aabbMaxXz * .5f + position)
                   || DoRaycast(position)
                   || DoRaycast(aabbMinXz + position) 
                   || DoRaycast(new Vector3(aabbMinXz.X, 0, aabbMaxXz.Z) + position) 
                   || DoRaycast(new Vector3(aabbMaxXz.X, 0, aabbMinXz.Z) + position) 
                   || DoRaycast(aabbMaxXz + position);
        }

        public void ResetFall()
        {
            FallTime = 0.01f;
        }

        public void ApplyImpulse(Vector3 Impulse)
        {
            this.Impulse += Impulse;
        }

        public bool CollidesWithStructures { get; set; }

        public bool UpdateColliderList { get; set; }

        public Vector3 LinearVelocity => _body.LinearVelocity.Compatible();

        public bool StaticRaycast(Vector3 End)
        {
            return StaticRaycast(End, out _);
        }
        
        public bool StaticRaycast(Vector3 End, out Vector3 Hit)
        {
            Hit = End;
            var result = BulletPhysics.Raycast(RigidbodyPosition.Compatible() + Vector3.UnitY.Compatible() * Parent.Model.Height * .5f, End.Compatible(), CollisionFilterGroups.StaticFilter | BulletPhysics.TerrainFilter);
            if(result.HasHit)
                Hit = result.HitPointWorld.Compatible();
            return result.HasHit;
        }

        public bool DisableCollisionIfNoContactResponse
        {
            get => _mainInformation.DisableCollisionIfNoContactResponse;

            set
            {
                _mainInformation.DisableCollisionIfNoContactResponse = value;
                _sensorInformation.DisableCollisionIfNoContactResponse = value;
            }
        }
        
        private float Timestep => Time.IndependentDeltaTime * (UseTimescale ? Time.TimeScale : 1);

        public override void Dispose()
        {
            _rayResult.Dispose();
            BulletPhysics.RemoveAndDispose(_body);
            BulletPhysics.RemoveAndDispose(_sensor);
            BulletPhysics.OnRigidbodyReAdded -= OnRigidbodyReAdded;
            BulletPhysics.OnCollision -= OnCollision;
            BulletPhysics.OnSeparation -= OnSeparation;
        }
    }
}