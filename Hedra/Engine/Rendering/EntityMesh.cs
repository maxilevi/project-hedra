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
		public Vector3 Size{get; set;}
		public bool Enabled{get; set;}
		public bool DontCull {get; set;}
		public bool Rendered {get; set;}
        private bool _disposed;
        public RenderShape Shape{get; set;}
		public int SceneId {get; set; }
		public ChunkMesh Mesh{get;}
		private EntityMeshBuffer Buffer;
		
		public Vector3 TargetRotation, TargetPosition;
		public float AnimationSpeed = 2;
		
		public EntityMesh(Vector3 Position){
			this.Enabled = true;
			SceneId = Scenes.SceneManager.Game.Id;
			var MeshBuffers = new ChunkMeshBuffer[1];
			MeshBuffers[0] = new EntityMeshBuffer();
			Mesh = new ChunkMesh(Position, MeshBuffers);
			Buffer = Mesh.MeshBuffers[0] as EntityMeshBuffer;
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
			return Buffer.TransformPoint(Point);// + Position;
		}
		
		public bool Outline{
			get{
				return Buffer.Outline;
			}
			set{
				Buffer.Outline = value;
			}
		}
		
		
		public Matrix4 TransformationMatrix{
			get{
				return Buffer.MatrixTrans;
			}
			set{
				Buffer.MatrixTrans = value;
			}
		}
		
		public Vector4 Tint{
			get{
				return Buffer.Tint;
			}
			set{
				Buffer.Tint = value;
			}
		}
		
		public Vector4 BaseTint{
			get{
				return Buffer.BaseTint;
			}
			set{
				Buffer.BaseTint = value;
			}
		}
		
		
		public Vector3 Position{
			get{
				return Buffer.Position;
			}
			set{
				Buffer.Position = value;
			}
		}
		
		public Vector3 LocalPosition{
			get{
				return Buffer.LocalPosition;
			}
			set{
				Buffer.LocalPosition = value;
			}
		}
		
		public Vector3 RotationPoint{
			get{
				return Buffer.Point;
			}
			set{
				Buffer.Point = value;
			}
		}
		
		public Vector3 Rotation{
			get{
				return Buffer.Rotation;
			}
			set{
				float valY = value.Y;
				
				if(float.IsInfinity(valY) || float.IsNaN(valY)) valY = 0;
				
				float valX = value.X;
				
				if(float.IsInfinity(valX) || float.IsNaN(valX)) valX = 0;
				
				float valZ = value.Z;
				
				if(float.IsInfinity(valZ) || float.IsNaN(valZ)) valZ = 0;

				
				Buffer.Rotation = new Vector3(valX, valY, valZ);
			}
		}
		
		public Vector3 BeforeLocalRotation{
			get{
				return Buffer.BeforeLocalRotation;
			}
			set{
				Buffer.BeforeLocalRotation = value;
			}
		}
		
		public Vector3 LocalRotation{
			get{
				return Buffer.LocalRotation;
			}
			set{
				float valY = value.Y;
				
				if(float.IsInfinity(valY) || float.IsNaN(valY)) valY = 0;
				
				float valX = value.X;
				
				if(float.IsInfinity(valX) || float.IsNaN(valX)) valX = 0;
				
				float valZ = value.Z;
				
				if(float.IsInfinity(valZ) || float.IsNaN(valZ)) valZ = 0;

				
				Buffer.LocalRotation = new Vector3(valX, valY, valZ);
			}
		}
		
		public Vector4 OutlineColor{
			get{ return Buffer.OutlineColor; }
			set{ Buffer.OutlineColor = value; }
		}
		
		public Vector3 LocalRotationPoint{
			get{
				return Buffer.LocalRotationPoint;
			}
			set{
				Buffer.LocalRotationPoint = value;
			}
		}
		
		public Vector3 AnimationRotation{
			get{
				return Buffer.AnimationRotation;
			}
			set{
				float valY = value.Y;
				
				if(valY > 40960 || valY < -40960) valY = 0;
				
				float valX = value.X;
				
				if(valX > 40960 || valX < -40960) valX = 0;
				
				float valZ = value.Z;
				
				if(valZ > 40960 || valZ < -40960) valZ = 0;

				
				Buffer.AnimationRotation = new Vector3(valX, valY, valZ);
			}
		}
		
		public Vector3 AnimationRotationPoint{
			get{
				return Buffer.AnimationRotationPoint;
			}
			set{
				Buffer.AnimationRotationPoint = value;
			}
		}
		
		public Vector3 AnimationPosition{
			get{
				return Buffer.AnimationPosition;
			}
			set{
				float valY = value.Y;
				
				if(valY > 4096 || valY < -4096) valY = 0;
				
				float valX = value.X;
				
				if(valX > 4096 || valX < -4096) valX = 0;
				
				float valZ = value.Z;
				
				if(valZ > 4096 || valZ < -4096) valZ = 0;

				
				Buffer.AnimationPosition = new Vector3(valX, valY, valZ);
			}
		}
		
		public bool UseFog{
			get{
				return Buffer.UseFog;
			}
			set{
				Buffer.UseFog = value;
			}
		}
		
		public float Alpha{
			get{
				return Buffer.Alpha;
			}
			set{
				Buffer.Alpha = value;
			}
		}
		
		
		public Vector3 Scale{
			get{
				return Buffer.Scale;
			}
			set{
				Buffer.Scale = value;
			}
		}
		
		private Vector3 BakedPosition{
			get{
				return Buffer.BakedPosition;
			}
			set{
				Buffer.BakedPosition = value;
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
