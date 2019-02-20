/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/01/2017
 * Time: 11:35 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Engine.ComplexMath;
using Hedra.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.Player
{
    /// <summary>
    /// Description of Projectile.
    /// </summary>
    public delegate void OnProjectileHitEvent(Projectile Sender, IEntity Hit);
    public delegate void OnProjectileLandEvent(Projectile Sender);
    public delegate void OnProjectileMoveEvent(Projectile Sender);
    
    public class Projectile : IDisposable, IUpdatable
    {
        public event OnProjectileHitEvent HitEventHandler;
        public event OnProjectileMoveEvent MoveEventHandler;
        public event OnProjectileMoveEvent LandEventHandler;

        public bool DisposeOnHit { get; set; } = true;
        public Vector3 Propulsion { get; set; }
        public Vector3 Direction { get; set; }
        public float Lifetime { get; set; } = 10f;
        public ObjectMesh Mesh { get; }
        public bool Collide { get; set; } = true;
        public bool HandleLifecycle { get; set; } = true;
        public bool Disposed { get; private set; }
        public bool UsePhysics { get; set; } = true;
        public float Speed { get; set; } = 1;
        public IEntity[] IgnoreEntities { get; set; }

        private readonly HashSet<IEntity> _collidedList;
        private readonly IEntity _parent;
        private readonly List<ICollidable> _chunkCollisions;
        private readonly List<ICollidable> _structureCollisions;
        private readonly Box _collisionBox;
        private Vector2 _lastChunkCollisionPosition;
        private Vector2 _lastStructureCollisionPosition;
        private Vector3 _accumulatedVelocity;


        public Projectile(IEntity Parent, Vector3 Origin, VertexData MeshData)
        {
            _parent = Parent;
            _collidedList = new HashSet<IEntity>();
            _chunkCollisions = new List<ICollidable>();
            _structureCollisions = new List<ICollidable>();
            _collisionBox = GetCollisionBox(MeshData);
            Mesh = ObjectMesh.FromVertexData(MeshData);
            Propulsion = Propulsion;
            Mesh.Position = Origin;
            //Mesh.LocalRotation = Physics.DirectionToEuler(Parent.Orientation);
            UpdateManager.Add(this);
        }

        protected virtual Box GetCollisionBox(VertexData MeshData)
        {
            return Physics.BuildBroadphaseBox(MeshData);
        }
        
        public virtual void Update()
        {
            if(Disposed) return;

            if (_accumulatedVelocity == Vector3.Zero)
            {
                _accumulatedVelocity = Propulsion + Vector3.UnitY * 7f;
            }

            Lifetime -= Time.DeltaTime;
            if (UsePhysics)
            {
                Propulsion *= (float)Math.Pow(.75f, Time.DeltaTime);
                _accumulatedVelocity += (Propulsion * 60f - Vector3.UnitY * 20f) * Time.DeltaTime;
                _accumulatedVelocity *= (float)Math.Pow(.8f, Time.DeltaTime);
                Mesh.Position += _accumulatedVelocity * 2f * Time.DeltaTime;
            }
            else
            {
                Mesh.Position += Direction * Speed * Time.DeltaTime;
            }
            var dir = Physics.DirectionToEuler(_accumulatedVelocity.NormalizedFast());
            Mesh.LocalRotation = dir;
            if (HandleLifecycle)
            {
                if (Collide)
                {
                    ProcessCollision();
                }

                try
                {
                    _collisionBox.Translate(Mesh.Position);
                    var entities = World.Entities;
                    for (var i = 0; i < entities.Count; i++)
                    {
                        if (_parent == entities[i]
                            || _collidedList.Contains(World.Entities[i])
                            || !Physics.Collides(_collisionBox, entities[i].Model.BroadphaseBox)
                            || IgnoreEntities != null && Array.IndexOf(IgnoreEntities, entities[i]) != -1) continue;

                        HitEventHandler?.Invoke(this, World.Entities[i]);
                        _collidedList.Add(World.Entities[i]);
                        if(DisposeOnHit)
                            Dispose();
                        break;
                    }
                }
                finally
                {
                    _collisionBox.Translate(-Mesh.Position);
                }

                if (Lifetime < 0)
                {
                    this.Dispose();
                }
            }

            MoveEventHandler?.Invoke(this);
        }

        private void ProcessCollision()
        {
            Collision.Update(
                Position,
                _chunkCollisions,
                _structureCollisions,
                ref _lastChunkCollisionPosition,
                ref _lastStructureCollisionPosition
            );
            var isColliding = false;
            try
            {
                _collisionBox.Translate(Mesh.Position);
                for (var i = 0; i < _structureCollisions.Count && !isColliding; i++)
                {
                    if (Physics.Collides(_structureCollisions[i], _collisionBox))
                        isColliding = true;
                }
                for (var i = 0; i < _chunkCollisions.Count && !isColliding; i++)
                {
                    if (Physics.Collides(_chunkCollisions[i], _collisionBox))
                        isColliding = true;
                }
            }
            finally
            {
                _collisionBox.Translate(-Mesh.Position);
            }

            if (Mesh.Position.Y <= Physics.HeightAtPosition(Mesh.Position))
                isColliding = true;          
            if (isColliding)
            {
                SoundPlayer.PlaySound(SoundType.HitGround, Mesh.Position);
                World.Particles.Color = new Vector4(1, 1, 1, 1);
                World.Particles.ParticleLifetime = 0.75f;
                World.Particles.GravityEffect = .0f;
                World.Particles.Scale = new Vector3(.75f, .75f, .75f);
                World.Particles.Position = Mesh.Position;
                World.Particles.PositionErrorMargin = Vector3.One * 1.5f;
                for (int i = 0; i < 10; i++)
                {
                    World.Particles.Direction = new Vector3(Utils.Rng.NextFloat(), Utils.Rng.NextFloat(), Utils.Rng.NextFloat()) * .15f;
                    World.Particles.Emit();
                }
                LandEventHandler?.Invoke(this);

                this.Dispose();
            }
        }

        public Vector3 Rotation
        {
            get => Mesh.LocalRotation;
            set => Mesh.LocalRotation = value;
        }

        public Vector3 Position
        {
            get => Mesh.Position;
            set => Mesh.Position = value;
        }

        public virtual void Dispose()
        {
            UpdateManager.Remove(this);
            Mesh.Dispose();
            Disposed = true;
        }
    }
}
