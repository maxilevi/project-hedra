/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/01/2017
 * Time: 11:35 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;

namespace Hedra.WorldObjects
{
    /// <summary>
    /// Description of Projectile.
    /// </summary>
    public delegate void OnProjectileHitEvent(Projectile Sender, IEntity Hit);
    public delegate void OnProjectileLandEvent(Projectile Sender, LandType Type);
    public delegate void OnProjectileMoveEvent(Projectile Sender);
    
    public class Projectile : IDisposable, IUpdatable, IWorldObject
    {
        public event OnDisposedEvent OnDispose;
        public event OnProjectileHitEvent HitEventHandler;
        public event OnProjectileMoveEvent MoveEventHandler;
        public event OnProjectileLandEvent LandEventHandler;

        public bool DisposeOnHit { get; set; } = true;
        public Vector3 Propulsion { get; set; }
        public Vector3 Direction { get; set; }
        public float Lifetime { get; set; } = 10f;
        public ObjectMesh Mesh { get; }
        public bool Collide { get; set; } = true;
        protected bool HandleLifecycle { get; set; } = true;
        public bool Disposed { get; private set; }
        public bool UsePhysics { get; set; } = true;
        public float Speed { get; set; } = 1;
        public float Falloff { get; set; } = 1f;
        public IEntity[] IgnoreEntities { get; set; }
        public bool CollideWithWater { get; set; }
        public bool PlaySound { get; set; } = true;
        public bool ShowParticlesOnDestroy { get; set; } = true;
        public bool ManuallyDispose { get; set; } = false;
        public float PropulsionDecay { get; set; } = 1;
        public Vector3 Delta { get; set; }

        private readonly HashSet<IEntity> _collidedList;
        private readonly IEntity _parent;
        private readonly List<ICollidable> _chunkCollisions;
        private readonly List<ICollidable> _structureCollisions;
        private readonly Box _collisionBox;
        private Vector2 _lastChunkCollisionPosition;
        private Vector2 _lastStructureCollisionPosition;
        private Vector3 _accumulatedVelocity;
        private bool _landed;


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

            if (_accumulatedVelocity == Vector3.Zero && UsePhysics || !UsePhysics && Direction == Vector3.Zero)
            {
                Direction = Propulsion.NormalizedFast();
                _accumulatedVelocity = Propulsion + Vector3.UnitY * 7f;
            }

            Lifetime -= Time.DeltaTime;
            Vector3 rotation;
            if (UsePhysics)
            {
                Propulsion *= (float)Math.Pow(.75f, Time.DeltaTime) * PropulsionDecay;
                _accumulatedVelocity += (Propulsion * 60f - Vector3.UnitY * 20f * Falloff) * Time.DeltaTime;
                _accumulatedVelocity *= (float)Math.Pow(.8f, Time.DeltaTime);
                var lastPosition = Mesh.Position;
                if(!_landed)
                    Mesh.Position += _accumulatedVelocity * 2.25f * Time.DeltaTime;
                Delta = Mesh.Position - lastPosition;
                rotation = Physics.DirectionToEuler(_accumulatedVelocity.NormalizedFast());
            }
            else
            {
                Mesh.Position += Direction * 100f * Speed * Time.DeltaTime;
                rotation = Physics.DirectionToEuler(Direction);
            }
            Mesh.LocalRotation = rotation;
            if(_landed) return;
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

                if (Lifetime < 0 && !ManuallyDispose)
                {
                    this.Dispose();
                }
            }

            MoveEventHandler?.Invoke(this);
        }

        private void ProcessCollision()
        {
            if(_landed) return;

            Collision.Update(
                Position,
                _chunkCollisions,
                _structureCollisions,
                ref _lastChunkCollisionPosition,
                ref _lastStructureCollisionPosition
            );
            var type = LandType.Structure;
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
            {
                isColliding = true;
                type = LandType.Ground;
            }

            if (CollideWithWater && Mesh.Position.Y <= Physics.WaterHeight(Mesh.Position))
            {
                isColliding = true;
                type = LandType.Water;
            }

            if (isColliding)
            {
                if(PlaySound) 
                    SoundPlayer.PlaySound(SoundType.HitGround, Mesh.Position);
                if (ShowParticlesOnDestroy)
                {
                    World.Particles.Color = new Vector4(1, 1, 1, 1);
                    World.Particles.ParticleLifetime = 0.75f;
                    World.Particles.GravityEffect = .0f;
                    World.Particles.Scale = new Vector3(.75f, .75f, .75f);
                    World.Particles.Position = Mesh.Position;
                    World.Particles.PositionErrorMargin = Vector3.One * 1.5f;
                    for (var i = 0; i < 10; i++)
                    {
                        World.Particles.Direction =
                            new Vector3(Utils.Rng.NextFloat(), Utils.Rng.NextFloat(), Utils.Rng.NextFloat()) * .15f;
                        World.Particles.Emit();
                    }
                }

                LandEventHandler?.Invoke(this, type);

                _landed = true;
                if(!ManuallyDispose) this.Dispose();
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
            OnDispose?.Invoke();
        }
    }
}
