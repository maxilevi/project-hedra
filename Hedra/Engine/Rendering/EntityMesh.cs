/*
 * Author: Zaphyk
 * Date: 03/03/2016
 * Time: 09:08 p.m.
 *
 */
using System;
using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering
{
	public class EntityMesh : IRenderable, ICullable, IDisposable
	{
	    public Vector3 TargetRotation { get; set; }
	    public Vector3 TargetPosition { get; set; }
	    public float AnimationSpeed { get; set; } = 2;
        public Vector3 Size { get; set;}
		public bool Enabled { get; set;}
		public bool DontCull { get; set;}
		public bool Rendered { get; set;}
        public RenderShape Shape { get; set;}
		public int SceneId { get; set; }
		public ChunkMesh Mesh { get; }

		private readonly EntityMeshBuffer _buffer;
	    private bool _disposed;
		
		public EntityMesh(Vector3 Position){
			this.Enabled = true;
			SceneId = Scenes.SceneManager.Game.Id;
			var meshBuffers = new ChunkMeshBuffer[1];
			meshBuffers[0] = new EntityMeshBuffer();
			Mesh = new ChunkMesh(Position, meshBuffers);
			_buffer = Mesh.MeshBuffers[0] as EntityMeshBuffer;
			this.Position = Position;
			this.Rotation = Vector3.Zero;
			Mesh.Enabled = true;
			Enabled = true;
			DrawManager.Add(this);
		}

		
		public void Draw(){
			if(Enabled)
				Mesh.Draw(0);
			
			
			this.AnimationPosition = Mathf.Lerp(this.AnimationPosition, this.TargetPosition, (float) Time.unScaledDeltaTime * 6 * AnimationSpeed);
			this.AnimationRotation = Mathf.Lerp(this.AnimationRotation, this.TargetRotation, (float) Time.unScaledDeltaTime * 6 * AnimationSpeed);
		}
		
		public Vector3 TransformPoint(Vector3 Point){
			return _buffer.TransformPoint(Point);// + Position;
		}
		
		public bool Outline{
			get{
				return _buffer.Outline;
			}
			set{
				_buffer.Outline = value;
			}
		}
		
		
		public Matrix4 TransformationMatrix{
			get{
				return _buffer.MatrixTrans;
			}
			set{
				_buffer.MatrixTrans = value;
			}
		}
		
		public Vector4 Tint{
			get{
				return _buffer.Tint;
			}
			set{
				_buffer.Tint = value;
			}
		}
		
		public Vector4 BaseTint{
			get{
				return _buffer.BaseTint;
			}
			set{
				_buffer.BaseTint = value;
			}
		}
		
		
		public Vector3 Position{
			get{
				return _buffer.Position;
			}
			set{
				_buffer.Position = value;
			}
		}
		
		public Vector3 LocalPosition{
			get{
				return _buffer.LocalPosition;
			}
			set{
				_buffer.LocalPosition = value;
			}
		}
		
		public Vector3 RotationPoint{
			get{
				return _buffer.Point;
			}
			set{
				_buffer.Point = value;
			}
		}
		
		public Vector3 Rotation{
			get{
				return _buffer.Rotation;
			}
			set{
				float valY = value.Y;
				
				if(float.IsInfinity(valY) || float.IsNaN(valY)) valY = 0;
				
				float valX = value.X;
				
				if(float.IsInfinity(valX) || float.IsNaN(valX)) valX = 0;
				
				float valZ = value.Z;
				
				if(float.IsInfinity(valZ) || float.IsNaN(valZ)) valZ = 0;

				
				_buffer.Rotation = new Vector3(valX, valY, valZ);
			}
		}
		
		public Vector3 BeforeLocalRotation{
			get{
				return _buffer.BeforeLocalRotation;
			}
			set{
				_buffer.BeforeLocalRotation = value;
			}
		}
		
		public Vector3 LocalRotation{
			get{
				return _buffer.LocalRotation;
			}
			set{
				float valY = value.Y;
				
				if(float.IsInfinity(valY) || float.IsNaN(valY)) valY = 0;
				
				float valX = value.X;
				
				if(float.IsInfinity(valX) || float.IsNaN(valX)) valX = 0;
				
				float valZ = value.Z;
				
				if(float.IsInfinity(valZ) || float.IsNaN(valZ)) valZ = 0;

				
				_buffer.LocalRotation = new Vector3(valX, valY, valZ);
			}
		}
		
		public Vector4 OutlineColor{
			get{ return _buffer.OutlineColor; }
			set{ _buffer.OutlineColor = value; }
		}
		
		public Vector3 LocalRotationPoint{
			get{
				return _buffer.LocalRotationPoint;
			}
			set{
				_buffer.LocalRotationPoint = value;
			}
		}
		
		public Vector3 AnimationRotation{
			get{
				return _buffer.AnimationRotation;
			}
			set{
				float valY = value.Y;
				
				if(valY > 40960 || valY < -40960) valY = 0;
				
				float valX = value.X;
				
				if(valX > 40960 || valX < -40960) valX = 0;
				
				float valZ = value.Z;
				
				if(valZ > 40960 || valZ < -40960) valZ = 0;

				
				_buffer.AnimationRotation = new Vector3(valX, valY, valZ);
			}
		}
		
		public Vector3 AnimationRotationPoint{
			get{
				return _buffer.AnimationRotationPoint;
			}
			set{
				_buffer.AnimationRotationPoint = value;
			}
		}
		
		public Vector3 AnimationPosition{
			get{
				return _buffer.AnimationPosition;
			}
			set{
				float valY = value.Y;
				
				if(valY > 4096 || valY < -4096) valY = 0;
				
				float valX = value.X;
				
				if(valX > 4096 || valX < -4096) valX = 0;
				
				float valZ = value.Z;
				
				if(valZ > 4096 || valZ < -4096) valZ = 0;

				
				_buffer.AnimationPosition = new Vector3(valX, valY, valZ);
			}
		}
		
		public bool UseFog{
			get{
				return _buffer.UseFog;
			}
			set{
				_buffer.UseFog = value;
			}
		}
		
		public float Alpha{
			get{
				return _buffer.Alpha;
			}
			set{
				_buffer.Alpha = value;
			}
		}
		
		
		public Vector3 Scale{
			get{
				return _buffer.Scale;
			}
			set{
				_buffer.Scale = value;
			}
		}

		public static EntityMesh FromVertexData(VertexData Data){
			EntityMesh EMesh = new EntityMesh(Vector3.Zero);
			ThreadManager.ExecuteOnMainThread( delegate{
			                                  	
			EMesh.Mesh.BuildFrom(EMesh.Mesh.MeshBuffers[0], Data, false);
			EMesh.Mesh.IsGenerated = true;
			EMesh.Mesh.IsBuilded = true;
			EMesh.Mesh.Enabled = true;
			                                  });
			
			return EMesh;
			
		}
		
		public static EntityMesh FromVertexData(VertexData Data, Vector3 Position){
			EntityMesh EMesh = new EntityMesh(Position);
			ThreadManager.ExecuteOnMainThread( delegate{
			                                  	
			EMesh.Mesh.BuildFrom(EMesh.Mesh.MeshBuffers[0], Data, false);
			EMesh.Mesh.IsGenerated = true;
			EMesh.Mesh.IsBuilded = true;
			EMesh.Mesh.Enabled = true;
			                                  });
			
			return EMesh;
			
		}
		
		public static EntityMesh FromVertexData(VertexData Data, Vector4 Color1, Vector4 Color2){
			Data.Color(AssetManager.ColorCode1, Color1);
			Data.Color(AssetManager.ColorCode2, Color2);
			
			return FromVertexData(Data, Vector3.Zero);
		}
		
		public static EntityMesh FromVertexData(VertexData Data, Vector3 Position, Vector4 Color1, Vector4 Color2){
			Data.Color(AssetManager.ColorCode1, Color1);
			Data.Color(AssetManager.ColorCode2, Color2);
			
			return FromVertexData(Data, Position);
		}
		public void Dispose(){
			_disposed = true;
			Mesh.Dispose();
			DrawManager.Remove(this);
		}
	}
}
