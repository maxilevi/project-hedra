/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/07/2016
 * Time: 11:37 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Rendering;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// This is a mess
	/// </summary>
	
	public delegate void OnParticleProjectileHitEvent(ParticleProjectile Sender, Entity Hit);
	
	public class ParticleProjectile : IUpdatable, IDisposable
	{
		public Vector3 Direction;
		public float Speed;
		
		public ParticleSystem Particles;
		public float Lifetime = 6f;
		public event OnParticleProjectileHitEvent HitEventHandler;
		public Vector3 Position;
		private Entity Parent;
		public bool Trail = false;
		private List<ICollidable> Collisions = new List<ICollidable>();
		public bool Collide = true;
		public bool UseLight {get; set;}
		private PointLight Light;
		
		public ParticleProjectile(Vector3 Scale, Vector3 Position, float Speed, Vector3 Direction, Entity Parent){
			this.Parent = Parent;
			this.Direction = Direction;
			this.Speed = Speed;
			this.Position = Position;
			
			Particles = new ParticleSystem(Vector3.Zero);
			
			UpdateManager.Add(this);
		}
		
		private Vector3 Velocity = Vector3.Zero;
		private bool Exploded = false;
		public void Update(){
			if(UseLight && Light == null){
				Light = ShaderManager.GetAvailableLight();
				if(Light != null)
					Light.Color = new Vector3(1,0.2f,0.2f);
			}
			
			Lifetime -= Time.ScaledFrameTimeSeconds;
			if(Collide){
				bool IsColliding = false;
				Collisions.Clear();
				Collisions.AddRange(World.GlobalColliders);
				
				Chunk UnderChunk = World.GetChunkAt(Position);
				Chunk UnderChunkR = World.GetChunkAt(Position + new Vector3(Chunk.Width,0, 0));
				Chunk UnderChunkL = World.GetChunkAt(Position - new Vector3(Chunk.Width,0, 0));
				Chunk UnderChunkF = World.GetChunkAt(Position + new Vector3(0,0,Chunk.Width));
				Chunk UnderChunkB = World.GetChunkAt(Position - new Vector3(0,0,Chunk.Width));
				
				if(UnderChunk != null)
					Collisions.AddRange(UnderChunk.CollisionShapes.ToArray());
				if(UnderChunkL != null)
					Collisions.AddRange(UnderChunkL.CollisionShapes.ToArray());
				if(UnderChunkR != null)
					Collisions.AddRange(UnderChunkR.CollisionShapes.ToArray());
				if(UnderChunkF != null)
					Collisions.AddRange(UnderChunkF.CollisionShapes.ToArray());
				if(UnderChunkB != null)
					Collisions.AddRange(UnderChunkB.CollisionShapes.ToArray());
				
				
				for(int i = 0; i < Collisions.Count; i++){
					
					if(Physics.Collides(Collisions[i], new Box(Position - Vector3.One, Position + Vector3.One) + new Box(Direction * 25 * Speed * (float) Time.deltaTime, Direction * 25 * Speed * (float) Time.deltaTime) )){
						IsColliding = true;
						break;
					}
				}
				if(Position.Y+.25f <= Physics.HeightAtPosition(Position))
					IsColliding = true;
				
				if(IsColliding){
                    //Sound.SoundManager.PlaySound(Sound.SoundType.ARROW_HIT, Mesh.Position, false, 1f, .8f);
				    World.Particles.Color = Particle3D.FireColor;
				    World.Particles.ParticleLifetime = 4;
				    World.Particles.GravityEffect = 0f;
				    World.Particles.Scale = Vector3.One * .5f;
				    World.Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				    World.Particles.Position = Position;
				    World.Particles.PositionErrorMargin = new Vector3(1f,1f,1f);
				    World.Particles.ParticleLifetime = 0.5f;
					for(int i = 0; i < 50; i++){
						Vector3 Dir = (Mathf.RandomVector3(Utils.Rng) - Vector3.One * 0.5f);
						World.Particles.Direction = Dir;
						World.Particles.Emit();
					}
					this.Dispose();
					Exploded = true;
					return;
				}
			}
			
			Velocity = Direction;
			this.Position += Velocity * 25 * (float) Time.deltaTime;
			if(Light != null){
				Light.Position = this.Position;
				ShaderManager.UpdateLight(Light);
			}

			Particles.Position = this.Position;
			Particles.Color = Particle3D.FireColor;
			Particles.ParticleLifetime = 1;
			Particles.GravityEffect = 0f;
			Particles.PositionErrorMargin = new Vector3(1f,1f,1f);
			Particles.Scale = Vector3.One * .5f;
			Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
			
			if(Trail)
				Particles.Emit();
			
			if(Lifetime <= 0){
				this.Dispose();
			}
			
			//Collision
			if(Exploded)
				return;

			for(var i = 0; i < World.Entities.Count; i++){	   
			    if (Parent == World.Entities[i] || !((this.Position - World.Entities[i].Position).LengthFast < 12 +
			        (World.Entities[i].BaseBox.Max - World.Entities[i].BaseBox.Min).LengthFast)) continue;

			    HitEventHandler?.Invoke(this, World.Entities[i]);
			    Exploded = true;
			    return;
			}
		}
		
		public void Dispose(){
			if(Light != null){
				Light.Position = Vector3.Zero;
				Light.Locked = false;
				ShaderManager.UpdateLight(Light);
				Light = null;
			}
			UpdateManager.Remove(this);
			if(Particles != null)
			Particles.Dispose();
		}
		
	}
}
