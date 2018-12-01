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
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    public delegate bool OnHitGroundEvent(IEntity Parent, float Falltime);

    public class PhysicsComponent : EntityComponent, IPhysicsComponent
    {
        private const float NormalSpeed = 2.25f;
        private const float MaxSlopeHeight = 1.0f;
        private const float MaxSlide = 3.0f;
        private const float AttackingSpeed = 0.75f;
        private readonly Entity _parent;
        private readonly object _lock = new object();
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

        public PhysicsComponent(Entity Parent) : base(Parent)
        {
            _parent = Parent;
            _physicsBox = new Box();
            UsePhysics = true;
            UseTimescale = true;
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

        private Chunk _underChunk, _underChunkR, _underChunkL, _underChunkF, _underChunkB;
        private readonly List<ICollidable> _collisions = new List<ICollidable>();
        private float _height;
        private float _speed;
        private bool _isOverTerrain;
        private float _deltaTime;
        private Box _physicsBox;

        public Vector3 TargetPosition
        {
            get => Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1);
            set => Parent.BlockPosition = new Vector3(value.X, value.Y / Chunk.BlockSize, value.Z);
        }

        public override void Update()
        {
            if (!UsePhysics)
                return;

            _deltaTime = this.Timestep;

            if (CollidesWithStructures)
            {
                _underChunk = World.GetChunkAt(Parent.Position);
                _underChunkR = World.GetChunkAt(Parent.Position + new Vector3(Chunk.Width, 0, 0));
                _underChunkL = World.GetChunkAt(Parent.Position - new Vector3(Chunk.Width, 0, 0));
                _underChunkF = World.GetChunkAt(Parent.Position + new Vector3(0, 0, Chunk.Width));
                _underChunkB = World.GetChunkAt(Parent.Position - new Vector3(0, 0, Chunk.Width));

                lock (_lock)
                {
                    _collisions.Clear();
                    _collisions.AddRange(World.GlobalColliders);
                    try
                    {
                        if (_underChunk != null && _underChunk.Initialized)
                            _collisions.AddRange(_underChunk.CollisionShapes);

                        var player = LocalPlayer.Instance;

                        if (player?.NearCollisions != null)
                            _collisions.AddRange(player.NearCollisions);

                        if (_underChunkL != null && _underChunkL.Initialized)
                            _collisions.AddRange(_underChunkL.CollisionShapes);
                        if (_underChunkR != null && _underChunkR.Initialized)
                            _collisions.AddRange(_underChunkR.CollisionShapes);
                        if (_underChunkF != null && _underChunkF.Initialized)
                            _collisions.AddRange(_underChunkF.CollisionShapes);
                        if (_underChunkB != null && _underChunkB.Initialized)
                            _collisions.AddRange(_underChunkB.CollisionShapes);

                    }
                    catch (Exception e)
                    {
                        Log.WriteLine("Catched a sync error." + Environment.NewLine + e);
                    }
                }
            }

            var modifier = 1;//40f * (1f / (float) Time.Frametime);
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
                        SoundManager.PlaySound(SoundType.HitGround, _parent.Position);
                    }
                    Falltime = 0;
                }
            }
            Parent.Model.Position = TargetPosition;//Mathf.Lerp(Parent.Model.Position, this.TargetPosition, _deltaTime * 8f);
            _speed = Mathf.Lerp(_speed, Parent.IsAttacking ? AttackingSpeed : NormalSpeed, _deltaTime * 2f);
        }

        public Vector3 MoveFormula(Vector3 Direction, bool ApplyReductions = true)
        {
            var movementSpeed = (Parent.IsUnderwater && !Parent.IsGrounded ? 1.25f : 1.0f) * Parent.Speed;
            return Direction * 5f * 1.75f * movementSpeed * (ApplyReductions ? _speed : NormalSpeed);
        }

        public void Move(float Scalar = 1)
        {
            DeltaTranslate(MoveFormula(Parent.Orientation * Scalar));
        }

        public void ResetVelocity()
        {
            Velocity = Vector3.Zero;
        }

        public void ExecuteTranslate(MoveCommand Command)
        {
            ProcessCommand(Command);
        }

        public void Translate(Vector3 Delta)
        {
            this.ExecuteTranslate(new MoveCommand(Delta));
        }

        public void DeltaTranslate(Vector3 Delta)
        {
            this.ExecuteTranslate(new MoveCommand(Delta * Time.DeltaTime));
        }

        public void DeltaTranslate(MoveCommand Command)
        {
            Command.Delta *= Time.DeltaTime;
            this.ExecuteTranslate(Command);
        }

        private float Timestep => Time.IndependantDeltaTime * (UseTimescale ? Time.TimeScale : 1);

        private void ProcessCommand(MoveCommand Command)
        {
            if (Command.Delta == Vector3.Zero) return;
            var canMove = HandleVoxelCollision(Command);
            HandleDrifting(Command);
            HandleEntityCollision(Command);
            var originalDelta = Command.Delta;
            var normalizedDelta = originalDelta.NormalizedFast();
            var remainingDelta = originalDelta.Length;
            var wontCollide = true;
            while (remainingDelta > 0)
            {
                var commandDelta = Math.Min(.25f, remainingDelta);
                Command.Delta = normalizedDelta * commandDelta;
                wontCollide &= HandleStructureCollision(Command);
                remainingDelta -= commandDelta;
            }
            Command.Delta = originalDelta;
            if (!wontCollide) OnColliderHit();
            canMove &= wontCollide;
            HandleTerrainCollision(Command);
            if (canMove)
            {
                var moveDelta = Command.Delta * new Vector3(1, 1f / Chunk.BlockSize, 1);
                Parent.BlockPosition += moveDelta;
            }
            else
            {
                if (Command.OnlyY) MakeGrounded();
            }
        }

        private void MakeGrounded()
        {
            Parent.IsGrounded = true;
            Velocity = Vector3.Zero;
            _isOverTerrain = false;
        }

        private bool HandleStructureCollision(MoveCommand Command)
        {
            _physicsBox.Min = Vector3.Zero + TargetPosition + 2 * Vector3.UnitY;
            _physicsBox.Max = Parent.Model.BaseBroadphaseBox.Size + TargetPosition;
            _physicsBox.Min += Command.Delta;
            _physicsBox.Max += Command.Delta;
            var shape = _physicsBox.AsShape();
            lock (_lock)
            {
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
                        return false;
                    }
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
            var slopeHeight = .25f;
            if (!CollidesWithOffset(Shape, Box, Vector3.UnitY * 1f))
            {
                /* If the object collided with Y but not Y+1 then it probably is a slope */
                var accum = .05f;
                while (CollidesWithOffset(Shape, Box, Vector3.UnitY * accum) && accum < 1f)
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

        private void HandleDrifting(MoveCommand Command)
        {
            var terrainNormal = Physics.NormalAtPosition(Parent.Position);
            var dot = Vector3.Dot(terrainNormal, Vector3.UnitY);
            if (IsDrifting)
            {
                Parent.BlockPosition += terrainNormal.Xz.ToVector3() * Time.DeltaTime * 8;
                this.ResetFall();
                this.ResetVelocity();
                if (!Command.OnlyY) return;
            }

            if (dot < .35 && Parent.IsGrounded) IsDrifting = true;
            else if (dot > .45 || !Parent.IsGrounded) IsDrifting = false;
        }

        private void HandleTerrainCollision(MoveCommand Command)
        {
            var heightAtPosition = Physics.HeightAtPosition((int)Parent.BlockPosition.X, (int)Parent.BlockPosition.Z);
            if (Parent.BlockPosition.Y * Chunk.BlockSize <= heightAtPosition)
            {
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

        private void HandleEntityCollision(MoveCommand Command)
        {
            if (!this.CollidesWithEntities || Command.IsRecursive) return;
            var entities = World.Entities;
            var chunkSpace = World.ToChunkSpace(Parent.Position);
            for (var i = entities.Count - 1; i > -1; i--)
            {
                if (entities[i] == Parent)
                    continue;

                /* Is a entity is farther than 2 chunks away, just skip it.*/
                if ((World.ToChunkSpace(entities[i].Position) - chunkSpace).LengthSquared > Chunk.Width * Chunk.Width)
                    continue;

                if (!entities[i].Physics.CollidesWithEntities) continue;
                var radii = Parent.Model.Dimensions.Size.LengthFast + entities[i].Model.Dimensions.Size.LengthFast;
                if (!((entities[i].Position - Parent.Position).LengthSquared < radii * radii)) continue;
                if (!Physics.Collides(entities[i].Model.BroadphaseBox, this.Parent.Model.BroadphaseBox) ||
                    !Physics.Collides(entities[i].Model.BroadphaseCollider, Parent.Model.BroadphaseCollider))
                    continue;

                if (!PushAround || !entities[i].Physics.CanBePushed) return;
                if (entities[i].Model.BroadphaseBox.Size.LengthSquared >
                    this.Parent.Model.BroadphaseBox.Size.LengthSquared * 4f)
                {
                    if (Vector3.Dot(Command.Delta.NormalizedFast(), (entities[i].Position - this.Parent.Position).NormalizedFast()) > .75f) return;
                    else continue;
                }
                var increment = -(Parent.Position.Xz - entities[i].Position.Xz).ToVector3().NormalizedFast();
                var command = new MoveCommand(increment * 8f)
                {
                    IsRecursive = true
                };
                entities[i].Physics.DeltaTranslate(command);
            }
        }

        public void ResetFall()
        {
            Falltime = 0.01f;
        }

        public override void Dispose()
        {
            lock (_lock)
                this._collisions.Clear();
            _underChunk = null;
            _underChunkR = null;
            _underChunkL = null;
            _underChunkF = null;
            _underChunkB = null;
        }
    }
}