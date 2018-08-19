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
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;

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

		public Vector3 Propulsion { get; set; }
		public float Lifetime { get; set; } = 10f;
		public ObjectMesh Mesh { get; }
	    public bool Collide { get; set; } = true;
        public bool Disposed { get; private set; }

        private readonly IEntity _parent;
	    private readonly List<ICollidable> _collisions;
	    private readonly Box _collisionBox;
        private Vector3 _accumulatedVelocity;
        private bool _collided;


        public Projectile(IEntity Parent, Vector3 Origin, VertexData MeshData)
        {
		    _parent = Parent;
            _collisions = new List<ICollidable>();
            _collisionBox = Physics.BuildBroadphaseBox(MeshData);
            Mesh = ObjectMesh.FromVertexData(MeshData);
			Propulsion = Propulsion;
            Mesh.Position = Origin;
            UpdateManager.Add(this);
        }
		
		public virtual void Update()
        {
            if(Disposed) return;

            if (_accumulatedVelocity == Vector3.Zero)
            {
                _accumulatedVelocity = Propulsion + Vector3.UnitY * 10f;
            }

            Lifetime -= Time.DeltaTime;
            Propulsion *= (float)Math.Pow(.75f, Time.DeltaTime);
            _accumulatedVelocity += (Propulsion * 60f - Vector3.UnitY * 20f) * (float) Time.DeltaTime;
            _accumulatedVelocity *= (float) Math.Pow(.8f, (float)Time.DeltaTime);
			Mesh.Position += _accumulatedVelocity * 2f * (float)Time.DeltaTime;
            Mesh.Rotation = Physics.DirectionToEuler(_accumulatedVelocity.NormalizedFast());
            if (Collide)
			{
			    ProcessCollision();
			}
				
			for(var i = 0; i < World.Entities.Count; i++)
            {
				if (_parent == World.Entities[i] || !Physics.Collides(_collisionBox.Cache.Translate(Mesh.Position), World.Entities[i].Model.BroadphaseBox)) continue;

				HitEventHandler?.Invoke(this, World.Entities[i]);
				_collided = true;
                this.Dispose();
                break;
            }

            if (Lifetime < 0)
            {
                this.Dispose();
            }
            MoveEventHandler?.Invoke(this);
        }

	    private void ProcessCollision()
	    {
            var isColliding = false;
            _collisions.Clear();
            _collisions.AddRange(World.GlobalColliders);

            var underChunk = World.GetChunkAt(Mesh.Position);
            var underChunkR = World.GetChunkAt(Mesh.Position + new Vector3(Chunk.Width, 0, 0));
            var underChunkL = World.GetChunkAt(Mesh.Position - new Vector3(Chunk.Width, 0, 0));
            var underChunkF = World.GetChunkAt(Mesh.Position + new Vector3(0, 0, Chunk.Width));
            var underChunkB = World.GetChunkAt(Mesh.Position - new Vector3(0, 0, Chunk.Width));

            if (underChunk != null)
                _collisions.AddRange(underChunk.CollisionShapes.ToArray());
            if (underChunkL != null)
                _collisions.AddRange(underChunkL.CollisionShapes.ToArray());
            if (underChunkR != null)
                _collisions.AddRange(underChunkR.CollisionShapes.ToArray());
            if (underChunkF != null)
                _collisions.AddRange(underChunkF.CollisionShapes.ToArray());
            if (underChunkB != null)
                _collisions.AddRange(underChunkB.CollisionShapes.ToArray());

            for (var i = 0; i < _collisions.Count; i++)
            {

                if (Physics.Collides(_collisions[i], _collisionBox.Cache.Translate(Mesh.Position)))
                {
                    isColliding = true;
                    break;
                }
            }
            if (Mesh.Position.Y <= Physics.HeightAtPosition(Mesh.Position))
                isColliding = true;

            if (isColliding)
            {
                //Sound.SoundManager.PlaySound(Sound.SoundType.ARROW_HIT, Mesh.Position, false, 1f, .8f);
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

		public virtual Vector3 Rotation
        {
			get => Mesh.Rotation;
		    set => Mesh.Rotation = value;
		}

	    public virtual Vector3 Position
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
