/*
 * Author: Zaphyk
 * Date: 12/02/2016
 * Time: 05:45 a.m.
 *
 */
using System;
using Hedra.Engine.Player;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.Particles
{
	public class ParticleSystem : IRenderable, IUpdatable, IDisposable
	{
		public const int MaxParticleCount = 15000;
	    public int MaxParticles { get; set; } = MaxParticleCount; 
		public List<Particle3D> Particles = new List<Particle3D>();
		public static Shader Shader = Shader.Build("Shaders/Particle.vert","Shaders/Particle.frag");
		public uint VAOID { get; private set; }
        public uint BufferID { get; private set; }
        public Vector3 Position { get; set; }
		public Vector3 Direction { get; set; }
		public Vector4 Color { get; set; }
        public float ParticleLifetime;
		public float GravityEffect;
		public Vector3 PositionErrorMargin;
		public Vector3 ScaleErrorMargin;
		public Vector3 Scale { get; set; }
        public bool RandomRotation { get; set; } = true;
		public ParticleShape Shape { get; set; } = ParticleShape.Square;
		public float ConeAngle;
		public bool Grayscale;
		public bool VariateUniformly;
        public bool HasMultipleOutputs { get; set; }
        public bool Enabled { get; set; } = true;
		
		
		public ParticleSystem(){
			this.Position = Vector3.Zero;
			ParticleCreator.Load();
			Executer.ExecuteOnMainThread(delegate
			{
				this.CreateVAO();

				DrawManager.ParticleRenderer.Add(this);
				UpdateManager.Add(this);
			});
		}
		
		public ParticleSystem(Vector3 Position){
			this.Position = Position;
			ParticleCreator.Load();
			Executer.ExecuteOnMainThread( delegate{ this.CreateVAO();
			
			DrawManager.ParticleRenderer.Add(this);
			UpdateManager.Add(this);
			});
		}
		
		public void Emit(){
			if( (this.Position - LocalPlayer.Instance.Position).LengthSquared > 512*512) return;
			
			if(Particles.Count == MaxParticles || !Enabled || (GameSettings.Paused && Particle3D.UseTimeScale)) return;
			
			float LocalPositionX = PositionErrorMargin.X * Utils.Rng.NextFloat() * 2f - PositionErrorMargin.X;
			float LocalPositionY = PositionErrorMargin.Y * Utils.Rng.NextFloat() * 2f - PositionErrorMargin.Y;
			float LocalPositionZ = PositionErrorMargin.Z * Utils.Rng.NextFloat() * 2f - PositionErrorMargin.Z;
			Vector3 ParticlePosition = new Vector3(LocalPositionX, LocalPositionY, LocalPositionZ);
			
			float ScaleMargin = Utils.Rng.NextFloat();
			float LocalScaleX = ScaleErrorMargin.X * ScaleMargin * 2f - ScaleErrorMargin.X;
			float LocalScaleY = ScaleErrorMargin.Y * ScaleMargin * 2f - ScaleErrorMargin.Y;
			float LocalScaleZ = ScaleErrorMargin.Z * ScaleMargin * 2f - ScaleErrorMargin.Z;
			
			Vector3 ParticleScale = new Vector3(LocalScaleX, LocalScaleY, LocalScaleZ);
			Vector4 NewColor = Vector4.One;
			if(Grayscale){
				float Shade = Color.Xyz.Average() + Utils.Rng.NextFloat() * .2f -.1f;
				NewColor = new Vector4(Shade, Shade, Shade, Color.W);
			}else{
				if(VariateUniformly){
					float Shade = Utils.Rng.NextFloat() * .2f -.1f;
					NewColor = new Vector4(Color.X + Shade, Color.Y + Shade, Color.Z + Shade, Color.W);
				}else{
					NewColor = Utils.VariateColor(Color, 50);
				}
			}
			
			if(Shape == ParticleShape.Cone){
				Particles.Add(new Particle3D(Position, ParticleCreator.UnitWithinCone(Direction, ConeAngle)* 25, Mathf.RandomVector3(Utils.Rng) * 360,
                             NewColor,
                             Scale + ParticleScale, GravityEffect, ParticleLifetime));
			}else if(Shape == ParticleShape.Sphere){
				
				if(ParticlePosition.X * ParticlePosition.X + ParticlePosition.Y * ParticlePosition.Y + ParticlePosition.Z * ParticlePosition.Z <=
				   (PositionErrorMargin.X * PositionErrorMargin.X + PositionErrorMargin.Y * PositionErrorMargin.Y + PositionErrorMargin.Z * PositionErrorMargin.Z) / 4.0)
				Particles.Add(new Particle3D(this.Position + ParticlePosition, Direction * 25, Mathf.RandomVector3(Utils.Rng) * 360,
                             NewColor,
                             Scale + ParticleScale, GravityEffect, ParticleLifetime));
			}else{
				Particles.Add(new Particle3D(this.Position + ParticlePosition, Direction * 25, Mathf.RandomVector3(Utils.Rng) * 360,
                             NewColor,
                             Scale + ParticleScale, GravityEffect, ParticleLifetime));
			}
		}
		
		
		public void Update(){
			if(!HasMultipleOutputs && (this.Position - LocalPlayer.Instance.Position).LengthSquared > 512*512) return;
			
			for(int i = 0; i < Particles.Count; i++){
				if(this.RandomRotation)
					Particles[i].Rotation += Mathf.RandomVector3(Utils.Rng) * 150 * (float) Time.DeltaTime;
				if(!Particles[i].Update()){
					Particles.RemoveAt(i);
				}
			}
			UpdateVBO();
		}
		
		public void Draw(){
			if(!HasMultipleOutputs && (this.Position - LocalPlayer.Instance.Position).LengthSquared > 512*512) return;
			
			if(Particles.Count > 0){
				Renderer.Enable(EnableCap.Blend);
				Renderer.Enable(EnableCap.DepthTest);
				//GraphicsLayer.Disable(EnableCap.CullFace);
				Shader.Bind();
				Shader["PlayerPosition"] = GameManager.Player.Position;
				
				GL.BindVertexArray(VAOID);

				Renderer.EnableVertexAttribArray(0);
				Renderer.EnableVertexAttribArray(1);
				Renderer.EnableVertexAttribArray(2);
				Renderer.EnableVertexAttribArray(3);
				Renderer.EnableVertexAttribArray(4);
				Renderer.EnableVertexAttribArray(5);
				Renderer.EnableVertexAttribArray(6);
				
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ParticleCreator.IndicesVBO.ID);
				GL.DrawElementsInstanced(PrimitiveType.Triangles, ParticleCreator.IndicesVBO.Count, DrawElementsType.UnsignedShort, IntPtr.Zero, Particles.Count);
				
				Renderer.DisableVertexAttribArray(0);
				Renderer.DisableVertexAttribArray(1);
				Renderer.DisableVertexAttribArray(2);
				Renderer.DisableVertexAttribArray(3);
				Renderer.DisableVertexAttribArray(4);
				Renderer.DisableVertexAttribArray(5);
				Renderer.DisableVertexAttribArray(6);
				GL.BindVertexArray(0);
				
				Shader.Unbind();
				//GraphicsLayer.Enable(EnableCap.CullFace);
				Renderer.Disable(EnableCap.Blend);
			}
		}
		
		private void CreateVAO()
		{
		    uint vaoid;
			GL.GenVertexArrays(1, out vaoid);
		    VAOID = vaoid;
			GL.BindVertexArray(VAOID);
			
			GL.BindBuffer(ParticleCreator.VerticesVBO.BufferTarget, ParticleCreator.VerticesVBO.ID);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			
			GL.BindBuffer(ParticleCreator.NormalsVBO.BufferTarget, ParticleCreator.NormalsVBO.ID);
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

		    uint bufferid;
			GL.GenBuffers(1, out bufferid);
		    BufferID = bufferid;
			GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(MaxParticles * Particle3D.SizeInBytes), IntPtr.Zero, BufferUsageHint.DynamicDraw);

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
		
		private void UpdateVBO(){
			if(Particles.Count > 0){
				Vector4[] Vec4s = new Vector4[Particles.Count * 5];
				
				for(int i = Particles.Count-1; i > -1; i--){
					Matrix4 TransMatrix = ConstructTransformationMatrix(Particles[i].Position, Particles[i].Rotation, Particles[i].Scale);
					Vec4s[i * 5 + 0] = Particles[i].Color;
					Vec4s[i * 5 + 1] = TransMatrix.Column0;
					Vec4s[i * 5 + 2] = TransMatrix.Column1;
					Vec4s[i * 5 + 3] = TransMatrix.Column2;
					Vec4s[i * 5 + 4] = TransMatrix.Column3;
				}

				GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);
				GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(MaxParticles * Particle3D.SizeInBytes), IntPtr.Zero, BufferUsageHint.DynamicDraw);
				GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr) (Particle3D.SizeInBytes * Particles.Count), Vec4s);
			}
		}
		
		private Matrix4 ConstructTransformationMatrix(Vector3 Position, Vector3 Rotation, Vector3 Scale){
			Vector3 Axis = Rotation / Rotation.Y;
			Matrix4 RotationMatrix = Matrix4.CreateFromAxisAngle(Axis, Rotation.Y * Mathf.Radian);
			
			Matrix4 TransMatrix = Matrix4.CreateScale(Scale);
			TransMatrix = Matrix4.Mult(TransMatrix, RotationMatrix);
			TransMatrix = Matrix4.Mult(TransMatrix,  Matrix4.CreateTranslation(Position));
			return TransMatrix;
		}
		
		public void Dispose(){
			//ThreadManager.ExecuteOnMainThread(() => GL.DeleteBuffers(1, ref BufferID));
			
			DrawManager.ParticleRenderer.Remove(this);
			UpdateManager.Remove(this);
		}
	}
	
	public enum ParticleShape{
		Square,
		Sphere,
		Cone
	}
}
