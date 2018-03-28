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
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Projectile.
	/// </summary>
	public delegate void OnProjectileHitEvent(Projectile Sender, Entity Hit);
	public delegate void OnProjectileLandEvent(Projectile Sender);
	public delegate void OnProjectileMoveEvent(Projectile Sender);
	
	public class Projectile : IDisposable
	{
		public event OnProjectileHitEvent HitEventHandler;
		public event OnProjectileMoveEvent MoveEventHandler;
		public event OnProjectileMoveEvent LandEventHandler;
		public Vector3 Direction;
		public float Speed = 1;
		public float Lifetime = 10f;
		public ObjectMesh Mesh;
		private Entity Parent;
		private bool Hitted = false;
		private List<ICollidable> Collisions = new List<ICollidable>();
		public bool Collide = true;
		public bool RotateOnX = false;
		
		
		public Projectile(VertexData MeshData, Vector3 Origin, Vector3 Direction, Entity Parent){
			this.Mesh = ObjectMesh.FromVertexData(MeshData);
			this.Direction = Direction;
			this.Mesh.Position = Origin;
			this.Parent = Parent;
			/*Matrix4 Mat4 = Matrix4.LookAt(Vector3.Zero, Direction, Vector3.UnitZ);
			float Angle;
			Vector3 Axis;
			Mat4.ExtractRotation().ToAxisAngle(out Axis, out Angle);
			this.Mesh.Rotation = new Vector3(Axis.X, Axis.Y, Axis.Z) * Angle;*/
			this.Mesh.Rotation = Physics.DirectionToEuler(Direction.Xz.NormalizedFast().ToVector3());
			CoroutineManager.StartCoroutine(Update);
		}
		
		public IEnumerator Update(){
			while(Lifetime > 0 && !Hitted){
				Lifetime -= Time.ScaledFrameTimeSeconds;
				Mesh.Position += Direction * 25 * Speed * (float) Time.deltaTime;
				if(RotateOnX)
					Mesh.Rotation += Vector3.UnitX * (float) Time.deltaTime * 90f;

			    MoveEventHandler?.Invoke(this);

			    #region Collision
				if(Collide){
					bool IsColliding = false;
					Collisions.Clear();
					Collisions.AddRange(World.GlobalColliders);
					
					Chunk UnderChunk = World.GetChunkAt(Mesh.Position);
					Chunk UnderChunkR = World.GetChunkAt(Mesh.Position + new Vector3(Chunk.ChunkWidth,0, 0));
					Chunk UnderChunkL = World.GetChunkAt(Mesh.Position - new Vector3(Chunk.ChunkWidth,0, 0));
					Chunk UnderChunkF = World.GetChunkAt(Mesh.Position + new Vector3(0,0,Chunk.ChunkWidth));
					Chunk UnderChunkB = World.GetChunkAt(Mesh.Position - new Vector3(0,0,Chunk.ChunkWidth));
					
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
						
						if(Physics.Collides(Collisions[i], new Box(Mesh.Position - Vector3.One * 1.5f, Mesh.Position + Vector3.One * 1.5f) + new Box(Direction * 25 * Speed * (float) Time.deltaTime, Direction * 25 * Speed * (float) Time.deltaTime) )){
							IsColliding = true;
							break;
						}
					}
					if(Mesh.Position.Y <= Physics.HeightAtPosition(Mesh.Position))
						IsColliding = true;
					
					if(IsColliding){
						//Sound.SoundManager.PlaySound(Sound.SoundType.ARROW_HIT, Mesh.Position, false, 1f, .8f);
						World.WorldParticles.Color = new Vector4(1,1,1,1);
						World.WorldParticles.ParticleLifetime = 0.75f;
						World.WorldParticles.GravityEffect = .0f; 
						World.WorldParticles.Scale = new Vector3(.75f,.75f,.75f);
						World.WorldParticles.Position = Mesh.Position;
						World.WorldParticles.PositionErrorMargin = Vector3.One * 1.5f;
						for(int i = 0; i < 10; i++){
							World.WorldParticles.Direction = new Vector3(Utils.Rng.NextFloat(), Utils.Rng.NextFloat(), Utils.Rng.NextFloat()) * .15f;
							World.WorldParticles.Emit();
						}
						if(this.LandEventHandler != null)
							this.LandEventHandler.Invoke(this);
						
						this.Dispose();
						yield break;
					}
				}
				#endregion
				
				for(int i = 0; i < World.Entities.Count; i++){
					if( Parent == World.Entities[i])
						continue;
	
					if( (Mesh.Position - World.Entities[i].Position).LengthSquared < World.Entities[i].Physics.HitboxSize * World.Entities[i].Model.Scale.Average() + 8*8){
					    HitEventHandler?.Invoke(this, World.Entities[i]);
					    Hitted = true;
						break;
					}
				}
				
				yield return null;
			}
			this.Dispose();
		}
		
		public Vector3 Rotation{
			get{ return Mesh.Rotation; }
			set{ Mesh.Rotation = value; }
		}
		
		public void Dispose(){
			Mesh.Dispose();
		}
	}
}
