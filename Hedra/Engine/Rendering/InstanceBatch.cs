﻿/*
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
		private readonly VertexData _original;
		public int Count { get; set; }
		public List<Matrix4> TransMatrix { get; set; }
		public List<Vector4> Colors { get; set; }
		public bool AffectWind {get; set;}
		public bool Builded {get; set;}	
		
		private static Shader _shader;
		private VBO<uint> _indicesVbo;
		private VBO<Vector3> _verticesVbo;
		private VBO<Vector3> _normalsVbo;
	    private Vector3 _highestPoint;
		private Vector3 _lowestPoint;
	    private uint _vaoId;
	    private uint _bufferId;
        private bool _initialized;
		
		public InstanceBatch(VertexData Original){
            TransMatrix = new List<Matrix4>();
            Colors = new List<Vector4>();
            this._original = Original;
		}

		public void Initialize(){
			if(_shader == null){
				_shader = Shader.Build("Shaders/Instance.vert","Shaders/Instance.frag");			
			}
			_lowestPoint = _original.SupportPoint(-Vector3.UnitY);
			_highestPoint = _original.SupportPoint(Vector3.UnitY);
			_indicesVbo = new VBO<uint>(_original.Indices.ToArray(), _original.Indices.Count * sizeof(uint),
			                           VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
			
			_verticesVbo = new VBO<Vector3>(_original.Vertices.ToArray(), _original.Vertices.Count * Vector3.SizeInBytes,
			                               VertexAttribPointerType.Float);
			
			_normalsVbo = new VBO<Vector3>(_original.Normals.ToArray(), _original.Normals.Count * Vector3.SizeInBytes,
			                              VertexAttribPointerType.Float);
			
			_original.Dispose();
			this.AffectWind = true;
			this.CreateVAO();
			this._initialized = true;
		}
		
		public void Add(Vector3 Position, Vector3 Scale, Vector4 Color, Vector3 Rotation){
			if(Count >= MaxInstances) return;
			
			this.Colors.Add(Color);
			var newMat = Matrix4.CreateScale(Scale);
			newMat *= Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z);
			newMat *= Matrix4.CreateTranslation(Position);
			this.TransMatrix.Add( newMat );
			this.Count++;
			this.Builded = false;
		}
		
		public void Build(){
			if(!this._initialized)
				this.Initialize();
			this.UpdateVBO();
			this.Builded = true;
		}
			
		public void Draw(){
			if(!Builded || !_initialized) return;
			
			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.DepthTest);
			_shader.Bind();
			
			_shader["HasWind"] = AffectWind ? 1 : 0;
			_shader["Time"] = Time.CurrentFrame;
			_shader["HighestPoint"] = _highestPoint;
			_shader["LowestPoint"] = _lowestPoint;
			_shader["PlayerPosition"] = GameManager.Player.Position;

            GL.BindVertexArray(_vaoId);
			GL.EnableVertexAttribArray(0);
			GL.EnableVertexAttribArray(1);
			GL.EnableVertexAttribArray(2);
			GL.EnableVertexAttribArray(3);
			GL.EnableVertexAttribArray(4);
			GL.EnableVertexAttribArray(5);
			GL.EnableVertexAttribArray(6);
			
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesVbo.ID);
			GL.DrawElementsInstanced(PrimitiveType.Triangles, _indicesVbo.Count, DrawElementsType.UnsignedInt, IntPtr.Zero, Count);
			
			GL.DisableVertexAttribArray(0);
			GL.DisableVertexAttribArray(1);
			GL.DisableVertexAttribArray(2);
			GL.DisableVertexAttribArray(3);
			GL.DisableVertexAttribArray(4);
			GL.DisableVertexAttribArray(5);
			GL.DisableVertexAttribArray(6);
			GL.BindVertexArray(0);
			
			_shader.UnBind();
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
			GL.BindBuffer(BufferTarget	.ArrayBuffer, _bufferId);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(MaxInstances * SizeInBytes), IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr) (SizeInBytes * Count), Vec4s);
		
		}
		
		public int SizeInBytes{
			get{ return sizeof(float) * 16 + Vector4.SizeInBytes; }
		}
		
		private void CreateVAO(){
			GL.GenVertexArrays(1, out _vaoId);
			GL.BindVertexArray(_vaoId);
			
			GL.BindBuffer(_verticesVbo.BufferTarget, _verticesVbo.ID);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			
			
			GL.BindBuffer(_normalsVbo.BufferTarget, _normalsVbo.ID);
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			
			GL.GenBuffers(1, out _bufferId);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferId);
			
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
			ThreadManager.ExecuteOnMainThread( () => GL.DeleteVertexArrays(1, ref _vaoId) );
			ThreadManager.ExecuteOnMainThread( () => GL.DeleteBuffers(1, ref _bufferId) );
		}
	}
}
