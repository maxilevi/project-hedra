/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/11/2016
 * Time: 01:25 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of InstanceBatch.
	/// </summary>
	public class InstanceBatch : IRenderable
	{
		public const int MaxInstances = 64;
		private VertexData Original;
		public int Count;
		public List<Matrix4> TransMatrix = new List<Matrix4>();
		public List<Vector4> Colors = new List<Vector4>();
		public bool AffectWind {get; set;}
		public bool Builded {get; set;}
		
		
		
		private static InstanceShader Shader;
		private VBO<uint> IndicesVBO;
		private VBO<Vector3> VerticesVBO;
		private VBO<Vector3> NormalsVBO;
		private uint VAOId, BufferID;
		private Vector3 HighestPoint;
		private Vector3 LowestPoint;
		private bool Initialized;
		
		public InstanceBatch(VertexData Original){
			this.Original = Original;
		}
		//So it can be Thread-Safe
		public void Initialize(){
			if(Shader == null){
				Shader = new InstanceShader("Shaders/Instance.vert","Shaders/Instance.frag");
				
			}
			LowestPoint = Original.SupportPoint(-Vector3.UnitY);
			HighestPoint = Original.SupportPoint(Vector3.UnitY);
			IndicesVBO = new VBO<uint>(Original.Indices.ToArray(), Original.Indices.Count * sizeof(uint),
			                           VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw);
			
			VerticesVBO = new VBO<Vector3>(Original.Vertices.ToArray(), Original.Vertices.Count * Vector3.SizeInBytes,
			                               VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
			
			NormalsVBO = new VBO<Vector3>(Original.Normals.ToArray(), Original.Normals.Count * Vector3.SizeInBytes,
			                              VertexAttribPointerType.Float, BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
			
			Original.Dispose();
			this.AffectWind = true;
			this.CreateVAO();
			this.Initialized = true;
		}
		
		public void Add(Vector3 Position, Vector3 Scale, Vector4 Color, Vector3 Rotation){
			if(Count >= MaxInstances) return;
			
			this.Colors.Add(Color);
			Matrix4 NewMat = Matrix4.CreateScale(Scale);
			NewMat *= Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z);
			NewMat *= Matrix4.CreateTranslation(Position);
			this.TransMatrix.Add( NewMat );
			this.Count++;
			this.Builded = false;
		}
		
		public void Build(){
			if(!this.Initialized)
				this.Initialize();
			this.UpdateVBO();
			this.Builded = true;
		}
			
		public void Draw(){
			if(!Builded || !Initialized) return;
			
			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.DepthTest);
			Shader.Bind();
			
			GL.Uniform1(Shader.WindUniform, (AffectWind) ? 1 : 0);
			GL.Uniform1(Shader.TimeUniform, Time.CurrentFrame);
			GL.Uniform3(Shader.HighestPointUniform, HighestPoint);
			GL.Uniform3(Shader.LowestPointUniform, LowestPoint);
			GL.Uniform3(Shader.PlayerPositionUniform, Scenes.SceneManager.Game.Player.Position);
			
			GL.BindVertexArray(VAOId);
			GL.EnableVertexAttribArray(0);
			GL.EnableVertexAttribArray(1);
			GL.EnableVertexAttribArray(2);
			GL.EnableVertexAttribArray(3);
			GL.EnableVertexAttribArray(4);
			GL.EnableVertexAttribArray(5);
			GL.EnableVertexAttribArray(6);
			
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO.ID);
			GL.DrawElementsInstanced(PrimitiveType.Triangles, IndicesVBO.Count, DrawElementsType.UnsignedInt, IntPtr.Zero, Count);
			
			GL.DisableVertexAttribArray(0);
			GL.DisableVertexAttribArray(1);
			GL.DisableVertexAttribArray(2);
			GL.DisableVertexAttribArray(3);
			GL.DisableVertexAttribArray(4);
			GL.DisableVertexAttribArray(5);
			GL.DisableVertexAttribArray(6);
			GL.BindVertexArray(0);
			
			Shader.UnBind();
			GL.Disable(EnableCap.Blend);
		}
		
		private void UpdateVBO(){
			if(Count == 0) return;
			
			Vector4[] Vec4s = new Vector4[Count * 5];
			for(int i = 0; i < Count; i++){
				Vec4s[i * 5 + 0] = Colors[i];
				Vec4s[i * 5 + 1] = TransMatrix[i].Column0;
				Vec4s[i * 5 + 2] = TransMatrix[i].Column1;
				Vec4s[i * 5 + 3] = TransMatrix[i].Column2;
				Vec4s[i * 5 + 4] = TransMatrix[i].Column3;
			}
			GL.BindBuffer(BufferTarget	.ArrayBuffer, BufferID);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(MaxInstances * SizeInBytes), IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr) (SizeInBytes * Count), Vec4s);
		
		}
		
		public int SizeInBytes{
			get{ return sizeof(float) * 16 + Vector4.SizeInBytes; }
		}
		
		private void CreateVAO(){
			GL.GenVertexArrays(1, out VAOId);
			GL.BindVertexArray(VAOId);
			
			GL.BindBuffer(VerticesVBO.BufferTarget, VerticesVBO.ID);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			
			
			GL.BindBuffer(NormalsVBO.BufferTarget, NormalsVBO.ID);
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			
			GL.GenBuffers(1, out BufferID);
			GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);
			
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(MaxInstances * SizeInBytes), IntPtr.Zero, BufferUsageHint.StaticDraw);

			//Columns of the TransMatrix
			GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes*5, IntPtr.Zero);
			
			GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes*5, (IntPtr) (Vector4.SizeInBytes));
			GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes*5, (IntPtr) (Vector4.SizeInBytes*2));
			GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes*5, (IntPtr) (Vector4.SizeInBytes*3));
			GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes*5, (IntPtr) (Vector4.SizeInBytes*4));
			
			GL.VertexAttribDivisor(2,1);
			GL.VertexAttribDivisor(3,1);
			GL.VertexAttribDivisor(4,1);
			GL.VertexAttribDivisor(5,1);
			GL.VertexAttribDivisor(6,1);
				
			GL.BindVertexArray(0);
		}
		
		public void Dispose(){
			ThreadManager.ExecuteOnMainThread( () => GL.DeleteVertexArrays(1, ref VAOId) );
			ThreadManager.ExecuteOnMainThread( () => GL.DeleteBuffers(1, ref BufferID) );
		}
	}
}
