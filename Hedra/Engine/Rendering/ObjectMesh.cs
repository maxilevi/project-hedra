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
	public class ObjectMesh : IRenderable, ICullable, IDisposable
	{
	    public Vector3 TargetRotation { get; set; }
	    public Vector3 TargetPosition { get; set; }
	    public float AnimationSpeed { get; set; } = 2;
        public Vector3 Size { get; set;}
		public bool Enabled { get; set;}
		public bool DontCull { get; set;}
		public bool Rendered { get; set;}
        public RenderShape Shape { get; set;}
		public ChunkMesh Mesh { get; }
		private readonly ObjectMeshBuffer _buffer;
		
		public ObjectMesh(Vector3 Position){
			this.Enabled = true;

		    var meshBuffers = new ChunkMeshBuffer[]
		    {
		        new ObjectMeshBuffer()
		    };
			this.Mesh = new ChunkMesh(Position, meshBuffers);
			this._buffer = Mesh.MeshBuffers[0] as ObjectMeshBuffer;
			this.Position = Position;
			this.Rotation = Vector3.Zero;
			Mesh.Enabled = true;
			Enabled = true;
			DrawManager.Add(this);
		}

		
		public void Draw(){
			if(Enabled) Mesh.Draw(0);					
			this.AnimationPosition = Mathf.Lerp(this.AnimationPosition, this.TargetPosition,
                Time.unScaledDeltaTime * 6 * AnimationSpeed);
			this.AnimationRotation = Mathf.Lerp(this.AnimationRotation, this.TargetRotation,
                Time.unScaledDeltaTime * 6 * AnimationSpeed);
		}

	    public bool ApplyNoiseTexture
	    {
	        get { return _buffer.UseNoiseTexture; }
	        set { _buffer.UseNoiseTexture = value; }
	    }

        public Vector3 TransformPoint(Vector3 Point){
			return _buffer.TransformPoint(Point);
		}

		public Matrix4 TransformationMatrix{
			get{ return _buffer.TransformationMatrix; }
			set{ _buffer.TransformationMatrix = value; }
		}
		
		public Vector4 Tint{
			get{ return _buffer.Tint; }
			set{ _buffer.Tint = value; }
		}
		
		public Vector4 BaseTint{
			get{ return _buffer.BaseTint; }
			set{ _buffer.BaseTint = value; }
		}
		
		
		public Vector3 Position{
			get{ return _buffer.Position; }
			set{ _buffer.Position = value; }
		}
		
		public Vector3 LocalPosition{
			get{ return _buffer.LocalPosition; }
			set{ _buffer.LocalPosition = value; }
		}
		
		public Vector3 RotationPoint{
			get{ return _buffer.Point; }
			set{ _buffer.Point = value; }
		}
		
		public Vector3 Rotation{
			get{ return _buffer.Rotation; }
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
		
		public bool ApplyFog{
			get{
				return _buffer.ApplyFog;
			}
			set{
				_buffer.ApplyFog = value;
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

		public static ObjectMesh FromVertexData(VertexData Data){
			var mesh = new ObjectMesh(Vector3.Zero);
			ThreadManager.ExecuteOnMainThread( delegate
            {		                                  	
			    mesh.Mesh.BuildFrom(mesh.Mesh.MeshBuffers[0], Data, false);
			    mesh.Mesh.IsGenerated = true;
			    mesh.Mesh.IsBuilded = true;
			    mesh.Mesh.Enabled = true;
			});
			
			return mesh;
			
		}
		
		public static ObjectMesh FromVertexData(VertexData Data, Vector3 Position){
			var mesh = new ObjectMesh(Position);
			ThreadManager.ExecuteOnMainThread( delegate
            {			                                  	
			    mesh.Mesh.BuildFrom(mesh.Mesh.MeshBuffers[0], Data, false);
			    mesh.Mesh.IsGenerated = true;
			    mesh.Mesh.IsBuilded = true;
			    mesh.Mesh.Enabled = true;                
            });
			
			return mesh;
			
		}
		
		public static ObjectMesh FromVertexData(VertexData Data, Vector4 Color1, Vector4 Color2){
			Data.Color(AssetManager.ColorCode1, Color1);
			Data.Color(AssetManager.ColorCode2, Color2);
			
			return FromVertexData(Data, Vector3.Zero);
		}
		
		public static ObjectMesh FromVertexData(VertexData Data, Vector3 Position, Vector4 Color1, Vector4 Color2){
			Data.Color(AssetManager.ColorCode1, Color1);
			Data.Color(AssetManager.ColorCode2, Color2);
			
			return FromVertexData(Data, Position);
		}
		public void Dispose(){
			Mesh.Dispose();
			DrawManager.Remove(this);
        }
	}
}
