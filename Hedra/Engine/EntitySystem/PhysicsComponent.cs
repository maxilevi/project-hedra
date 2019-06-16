/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 02/05/2016
 * Time: 09:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Generation;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.Engine.EntitySystem
{
    public delegate bool OnHitGroundEvent(IEntity Parent, float Falltime);
    public delegate void OnMoveEvent();

    public class PhysicsComponent : EntityComponent, IPhysicsComponent
    {
        private const float NormalSpeedModifier = 2.25f;
        private const float MaxSlopeHeight = 0.25f;
        private const float MaxSlide = 3.0f;
        public event OnMoveEvent OnMove;
        public event OnHitGroundEvent OnHitGround;
        public bool UsePhysics { get; set; }
        public float Falltime { get; private set; }
        public bool CanBePushed { get; set; } = true;
        public Vector3 GravityDirection { get; set; } = new Vector3(0, -1f, 0);
        public float VelocityCap { get; set; } = float.MaxValue;
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public bool HasFallDamage { get; set; } = true;
        public bool UseTimescale { get; set; }
        public bool InFrontOfWall { get; private set; }
        public bool IsDrifting { get; private set; }
        public bool IsOverAShape => !_isOverTerrain;

        private Chunk _underChunk;
        private Chunk _underChunkR;
        private Chunk _underChunkL;
        private Chunk _underChunkF;
        private Chunk _underChunkB;
        private float _height;
        private float _speedMultiplier;
        private bool _isOverTerrain;
        private float _deltaTime;
        private Vector3 _lastPosition;
        private readonly Box _physicsBox;
        private readonly List<ICollidable> _collisions;
        private readonly Timer _updateCollidersTimer;
        
        public PhysicsComponent(IEntity Parent) : base(Parent)
        {
            _physicsBox = new Box();
            _updateCollidersTimer = new Timer(.5f);
            UsePhysics = true;
            UseTimescale = true;
            _collisions = new List<ICollidable>();
        }

        public Vector3 TargetPosition
        {
            get => Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1);
            set => Parent.BlockPosition = new Vector3(value.X, value.Y / Chunk.BlockSize, value.Z);
        }
        
        public void UpdateColliders()
        {
            //if(!_updateCollidersTimer.Tick()) return;
            _underChunk = World.GetChunkAt(Parent.Physics.TargetPosition);
            _underChunkR = World.GetChunkAt(Parent.Physics.TargetPosition + new Vector3(Chunk.Width, 0, 0));
            _underChunkL = World.GetChunkAt(Parent.Physics.TargetPosition - new Vector3(Chunk.Width, 0, 0));
            _underChunkF = World.GetChunkAt(Parent.Physics.TargetPosition + new Vector3(0, 0, Chunk.Width));
            _underChunkB = World.GetChunkAt(Parent.Position - new Vector3(0, 0, Chunk.Width));

            _collisions.Clear();
            _collisions.AddRange(World.GlobalColliders);
            if (_underChunk != null && _underChunk.Initialized)
                _collisions.AddRange(_underChunk.CollisionShapes);
            if (_underChunkL != null && _underChunkL.Initialized)
                _collisions.AddRange(_underChunkL.CollisionShapes);
            if (_underChunkR != null && _underChunkR.Initialized)
                _collisions.AddRange(_underChunkR.CollisionShapes);
            if (_underChunkF != null && _underChunkF.Initialized)
                _collisions.AddRange(_underChunkF.CollisionShapes);
            if (_underChunkB != null && _underChunkB.Initialized)
                _collisions.AddRange(_underChunkB.CollisionShapes);

            var nearCollisions = GameManager.Player.NearCollisions;
            var currentOffset = World.ToChunkSpace(Parent.Physics.TargetPosition);
            for (var i = 0; i < nearCollisions.Length; i++)
            {
                if(nearCollisions[i].Contains(currentOffset))
                    _collisions.Add(nearCollisions[i]);
            }
        }

        public override void Update()
        {
            _deltaTime = this.Timestep;
            _speedMultiplier = Mathf.Lerp(_speedMultiplier, NormalSpeedModifier * (Parent.IsAttacking ? Parent.AttackingSpeedModifier : 1), _deltaTime * 2f);
            if (!UsePhysics) return;
            if (Parent.IsGrounded && Parent.BlockPosition == _lastPosition 
                                  && TargetPosition.Y >= Physics.HeightAtPosition(Parent.BlockPosition)) return;
            if(!GameSettings.Paused) _lastPosition = Parent.BlockPosition;
            if ((CollidesWithStructures || UpdateColliderList) && !GameSettings.Paused)
            {
                UpdateColliders();
            }

            var modifier = 1;
            Velocity += -Physics.Gravity * GravityDirection * _deltaTime * Chunk.BlockSize;
            Velocity = Mathf.Clamp(Velocity, -VelocityCap, VelocityCap);

            var command = new MoveCommand(Velocity * _deltaTime, true);
            this.ProcessCommand(command);

            if (!Parent.IsGrounded)
            {
                if (!Parent.IsUnderwater)
                    Falltime += _deltaTime;
            }
            else
            {
                if (Falltime > 0)
                {
                    if (Falltime > 2.0f && HasFallDamage && (OnHitGround?.Invoke(this.Parent, Falltime) ?? true))
                    {
                        if (!Parent.SearchComponent<DamageComponent>()?.Immune ?? true)
                        {
                            var fallTime = Falltime;
                            Executer.ExecuteOnMainThread(delegate
                            {
                                Parent.Damage(fallTime * 7.5f, null, out _, true);
                                Parent.KnockForSeconds(3f);
                            });
                        }
                    }
                    else if (Falltime > 1f)
                    {
                        SoundPlayer.PlaySound(SoundType.HitGround, Parent.Position);
                    }

                    Falltime = 0;
                }
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
            DeltaTranslate(MoveFormula(Parent.Orientation * Scalar));
        }

        public void ResetVelocity()
        {
            Velocity = Vector3.Zero;
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

        public bool DeltaTranslate(MoveCommand Command)
        {
            Command.Delta *= Time.DeltaTime;
            return this.ExecuteTranslate(Command);
        }

        private float Timestep => Time.IndependentDeltaTime * (UseTimescale ? Time.TimeScale : 1);

        private bool ProcessCommand(MoveCommand Command)
        {
            if (Command.Delta == Vector3.Zero) return true;
            if (Command.OnlyY && Math.Abs(Command.Delta.Y) > .25f) Parent.IsGrounded = false;
            var canMove = HandleVoxelCollision(Command);
            HandleEntityCollision(Command);
            var originalDelta = Command.Delta;
            var normalizedDelta = originalDelta.NormalizedFast();
            var remainingDelta = originalDelta.Length;
            var wontCollide = true;
            while (remainingDelta > 0 && CollidesWithStructures)
            {
                var commandDelta = Math.Min(.25f, remainingDelta);
                Command.Delta = normalizedDelta * commandDelta;
                wontCollide &= HandleStructureCollision(Command);
                remainingDelta -= commandDelta;
            }
            Command.Delta = originalDelta;
            if (!wontCollide && Command.OnlyY) OnColliderHit();
            canMove &= wontCollide;
            HandleTerrainCollision(Command);
            if (canMove)
            {
                var moveDelta = Command.Delta * new Vector3(1, 1f / Chunk.BlockSize, 1);
                Parent.BlockPosition += moveDelta;
                if(moveDelta.LengthFast > 0.005f)
                    OnMove?.Invoke();
            }
            else
            {
                if (Command.OnlyY && Command.Delta.Y < 0) MakeGrounded();
            }

            return canMove;
        }

        private void MakeGrounded()
        {
            if(!Parent.IsGrounded) OnColliderHit();
            Parent.IsGrounded = true;
            Velocity = Vector3.Zero;
            _isOverTerrain = false;
        }

        private bool HandleStructureCollision(MoveCommand Command)
        {
            _physicsBox.Min = -Parent.Model.BaseBroadphaseBox.Size.Xz.ToVector3() * .5f + TargetPosition;
            _physicsBox.Max = Parent.Model.BaseBroadphaseBox.Size.Xz.ToVector3() * .5f + TargetPosition + Parent.Model.BaseBroadphaseBox.Size.Y * .5f * Vector3.UnitY;
            _physicsBox.Min += Command.Delta;
            _physicsBox.Max += Command.Delta;
            var shape = Parent.Model.BroadphaseBox.Cache.Translate(Command.Delta).AsShape();
            for (var i = _collisions.Count - 1; i > -1; i--)
            {
                if (!Physics.Collides(_collisions[i], shape)) continue;
                if (!Command.OnlyY)
                {
                    var collided = true;
                    collided &= HandleSlopes(Command, _collisions[i], shape);
                    if(collided) HandleSliding(Command, _collisions[i], shape);
                    collided &= HandleSlidingCorners(Command, _collisions[i], shape);
                    if (collided) return false;
                }
                else
                {
                    if (Command.Delta.Y < 0)
                    {
                        Parent.IsGrounded = true;
                    }
                    else
                    {
                        if (!CollidesWithOffset(_collisions[i], shape, Vector3.UnitY * -.5f))
                            Parent.BlockPosition -= .05f * Vector3.UnitY;
                        Parent.IsGrounded = false;
                    }
                    return false;
                }
            }         
            return true;
        }

        private bool HandleSlidingCorners(MoveCommand Command, ICollidable Shape, CollisionShape Box)
        {
            if (!CollidesWithOffset(Shape, Box, Command.Delta.NormalizedFast() * MaxSlide))
            {
                return false;
            }
            return true;
        }

        private bool HandleSlopes(MoveCommand Command, ICollidable Shape, CollisionShape Box)
        {
            if (!CollidesWithOffset(Shape, Box, Vector3.UnitY * MaxSlopeHeight))
            {
                /* If the object collided with Y but not Y+1 then it probably is a slope */
                var accum = .05f;
                while (CollidesWithOffset(Shape, Box, Vector3.UnitY * accum) && accum < 1f && !CollidesWithOffset(Shape, Box, Vector3.UnitY * (2f + accum)))
                {
                    accum += 0.05f;
                }
                Parent.BlockPosition += accum * Vector3.UnitY;
                MakeGrounded();
                return false;
            }
            return true;
        }

        private void HandleSliding(MoveCommand Command, ICollidable Shape, CollisionShape Box)
        {
            var offset = Vector3.Zero;
            if (!CollidesWithOffset(Shape, Box, offset = (Command.Delta.Xz.PerpendicularLeft.ToVector3() * .5f + Command.Delta * .5f)))
            {
                Parent.BlockPosition += offset;
            }
            else if (!CollidesWithOffset(Shape, Box, offset = Command.Delta.Xz.PerpendicularLeft.ToVector3()))
            {
                Parent.BlockPosition += offset;
            }
            else if (!CollidesWithOffset(Shape, Box, offset = (Command.Delta.Xz.PerpendicularRight.ToVector3() * .5f + Command.Delta * .5f)))
            {
                Parent.BlockPosition += offset;
            }
            else if (!CollidesWithOffset(Shape, Box, offset = Command.Delta.Xz.PerpendicularRight.ToVector3()))
            {
                Parent.BlockPosition += offset;
            }
        }

        private bool CollidesWithOffset(ICollidable Shape, CollisionShape Box, Vector3 Offset)
        {
            try
            {
                Box.Transform(Offset);
                return Physics.Collides(Shape, Box);
            }
            finally
            {
                Box.Transform(-Offset);
            }
        }

        private bool HandleVoxelCollision(MoveCommand Command)
        {
            var modifierX = Command.Delta.X < 0 ? -1f : 1f;
            var modifierZ = Command.Delta.Z < 0 ? -1f : 1f;
            if (Command.OnlyY) return true;
            var nextBlock =
                World.GetBlockAt(new Vector3(1f * modifierX, 0, 1f * modifierZ) + Command.Delta +
                                 this.Parent.BlockPosition);
            bool IsSolid(Block B) => B.Type != BlockType.Air && B.Type != BlockType.Water;
            var calcPosition = new Vector3(1f * modifierX, 2.5f, 1f * modifierZ) + Command.Delta +
                               this.Parent.BlockPosition;
            var nextBlockY = World.GetBlockAt(calcPosition);
            var tNormal = Physics.NormalAtPosition(calcPosition);
            if (!IsDrifting)
            {
                InFrontOfWall = IsSolid(nextBlockY) ||
                                (Vector3.Dot(tNormal, Vector3.UnitY) < .35f && IsSolid(nextBlock));
                if (InFrontOfWall) return false;
            }
            return true;
        }

        private void HandleTerrainCollision(MoveCommand Command)
        {
            var heightAtPosition = Physics.HeightAtPosition((int)Parent.BlockPosition.X, (int)Parent.BlockPosition.Z);
            if (Parent.BlockPosition.Y * Chunk.BlockSize <= heightAtPosition)
            {
                if(!Parent.IsGrounded) 
                    OnColliderHit();
                
                Parent.BlockPosition = new Vector3(Parent.BlockPosition.X, heightAtPosition / Chunk.BlockSize,
                    Parent.BlockPosition.Z);
                Parent.IsGrounded = true;
                Velocity = Vector3.Zero;
                _isOverTerrain = true;
            }

            if (Command.Delta.Y < 0 ||  Parent.IsUnderwater)
            {
                var human = Parent as Humanoid;
                if (Parent.IsUnderwater || (human?.IsJumping ?? false))
                {
                     Parent.IsGrounded = false;
                    _isOverTerrain = false;
                }
            }
        }

        private void OnColliderHit()
        {
            if (!(Parent is IHumanoid human) || !human.IsTravelling) return;
            human.IsTravelling = false;
            Parent.KnockForSeconds(3f);
            Executer.ExecuteOnMainThread(delegate
            {
                Parent.Damage(Parent.MaxHealth * .15f, Parent, out _);
            });
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
        
        private void HandleEntityCollision(MoveCommand Command)
        {
            if (!this.CollidesWithEntities || Command.IsRecursive) return;
            EntityRaycast(World.Entities.ToArray(), Vector3.Zero, E =>
            {
                if (!PushAround || !E.Physics.CanBePushed) return false;
                if (E.Model.BroadphaseBox.Size.LengthSquared >
                    this.Parent.Model.BroadphaseBox.Size.LengthSquared * 4f)
                {
                    if (Vector3.Dot(Command.Delta.NormalizedFast(), (E.Position - this.Parent.Position).NormalizedFast()) > .75f) return false;
                    else return false;
                }
                var increment = -(Parent.Position.Xz - E.Position.Xz).ToVector3().NormalizedFast();
                var command = new MoveCommand(increment * 8f)
                {
                    IsRecursive = true
                };
                E.Physics.DeltaTranslate(command);
                return false;
            });
        }
        
        public void ResetFall()
        {
            Falltime = 0.01f;
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
            var shape = new CollisionShape(new []
            {
                TargetPosition,
                End
            });
            return _collisions.Any(S => Physics.Collides(S, shape));
        }
        
        public bool CollidesWithOffset(Vector3 Offset)
        {
            var shape = Parent.Model.BroadphaseBox.Cache.Translate(Offset).AsShape();
            return _collisions.Any(S => Physics.Collides(S, shape));
        }
        
        public override void Dispose()
        {
            _collisions.Clear();
            _underChunk = null;
            _underChunkR = null;
            _underChunkL = null;
            _underChunkF = null;
            _underChunkB = null;
        }
    }
}