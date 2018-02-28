/*
 * Author: Zaphyk
 * Date: 04/03/2016
 * Time: 05:59 a.m.
 *
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering.Effects;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of EntityMeshBuffer.
	/// </summary>
	public class EntityMeshBuffer : ChunkMeshBuffer
	{
		public static EntityMeshShader Shader = new EntityMeshShader("Shaders/EntityMesh.vert", "Shaders/EntityMesh.frag");
		public bool UseFog = true;
		public bool Outline = false;
		public float Alpha = 1;
		public Vector4 Tint = new Vector4(1,1,1,1);
		public Vector4 BaseTint = new Vector4(0,0,0,0);
		public Vector3 BakedPosition;
		public Vector4 OutlineColor = new Vector4(0.812f, 0.061f, 0.076f, .100f);
		
		public override void Draw(Vector3 Position, bool Shadows){
			if(Constants.HIDE_ENTITIES || Indices == null || Data == null) return;
			
			Bind();

			GL.Uniform4(Shader.TintUniform, Tint+BaseTint);
			
			if(Alpha < 1)
				GL.Enable(EnableCap.Blend);
			//GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.Enable(EnableCap.DepthTest);
			
			Data.Bind();
			GL.EnableVertexAttribArray(0);
			GL.EnableVertexAttribArray(1);
			GL.EnableVertexAttribArray(2);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
			GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
			
			GL.DisableVertexAttribArray(0);
			GL.DisableVertexAttribArray(1);
			GL.DisableVertexAttribArray(2);
			Data.UnBind();
			
			GL.Disable(EnableCap.Blend);

			UnBind();
			
			if(Alpha < 1)
				GL.Disable(EnableCap.Blend);
		}
		
		public Vector3 TransformPoint(Vector3 Vertex){
			Vertex *= Scale;
			
			Vertex += AnimationRotationPoint;
			Vertex =  Vector3.TransformPosition(Vertex, m_RotationAnimationMatrix);
			Vertex -= AnimationRotationPoint;
			
			Vertex += BeforeLocalRotation;
			Vertex += LocalRotationPoint;
			Vertex = Vector3.TransformPosition(Vertex, m_LocalMat4);
			Vertex -= LocalRotationPoint;
			
			Vertex += AnimationPosition;
		
			Vertex += Point;
			Vertex = Vector3.TransformPosition(Vertex, m_mat4);
			Vertex -= Point;
			
			Vertex += m_pos;
			return Vertex;
		}
		
		private bool m_RotMatrixCached;
		private Matrix4 p_m_mat4;
		private Matrix4 m_mat4{
			get{ 
				if(m_RotMatrixCached)
					return p_m_mat4;
				else{
					p_m_mat4 = Matrix4.CreateRotationX(m_Rot.X * Mathf.Radian);
					p_m_mat4 *= Matrix4.CreateRotationY(m_Rot.Y * Mathf.Radian);
					p_m_mat4 *= Matrix4.CreateRotationZ(m_Rot .Z * Mathf.Radian);
					m_RotMatrixCached = true;
					return p_m_mat4;
				}
			}
		}
		private Vector3 m_Rot = Vector3.Zero;
		public Vector3 Rotation{
			get{ return m_Rot; }
			set{
				m_Rot = value;			
				m_RotMatrixCached = false;  
			}
		}
		private Vector3 m_pos = Vector3.Zero;
		public Vector3 Position{
			get{ 
				return m_pos;
			}
			set{
				m_pos = value;	
			}
		}
		
		public Vector3 Scale = Vector3.One;
		public Vector3 Point = Vector3.Zero;
		public Vector3 LocalRotationPoint = Vector3.Zero;
		public Vector3 LocalPosition = Vector3.Zero;
		public Vector3 BeforeLocalRotation = Vector3.Zero;
		public Vector3 AnimationPosition = Vector3.Zero;
		public Vector3 AnimationRotationPoint = Vector3.Zero;
		private Matrix4 m_Mat4Trans = Matrix4.Identity;
		public Matrix4 MatrixTrans{ get{
				return m_Mat4Trans;
			} 
			set{
				m_Mat4Trans = value;
			} 
		}

		private Vector3 m_LocalRot = Vector3.Zero;
		private Matrix4 m_LocalMat4 = Matrix4.Identity;
		public Vector3 LocalRotation{
			get{ return m_LocalRot; }
			set{
				m_LocalRot = value;
				m_LocalMat4 = Matrix4.CreateRotationX(value.X * Mathf.Radian);
				m_LocalMat4 *= Matrix4.CreateRotationY(value.Y * Mathf.Radian);
				m_LocalMat4 *= Matrix4.CreateRotationZ(value.Z * Mathf.Radian);
			}
		}
		
		private Vector3 m_AnimationRotation = Vector3.Zero;
		private Matrix4 m_RotationAnimationMatrix = Matrix4.Identity;
		public Vector3 AnimationRotation{
			get{ return m_AnimationRotation; }
			set{
				m_AnimationRotation = value;
				m_RotationAnimationMatrix = Matrix4.CreateRotationX(value.X * Mathf.Radian);
				m_RotationAnimationMatrix *= Matrix4.CreateRotationY(value.Y * Mathf.Radian);
				m_RotationAnimationMatrix *= Matrix4.CreateRotationZ(value.Z * Mathf.Radian);
			}
		}
		
		public override void Bind(){
			Shader.Bind();
			
			GL.Uniform1(Shader.AlphaUniform, Alpha);
			GL.Uniform3(Shader.ScaleUniform, Scale);
			GL.Uniform1(Shader.UseFogUniform, (UseFog) ? 1 : 0);
			GL.Uniform3(Shader.TransPos, m_pos);
			Matrix4 mat4 = m_mat4;
			GL.UniformMatrix4(Shader.TransMatrixUniformLocation, false, ref mat4);
			GL.UniformMatrix4(Shader.MatrixUniformLocation, false, ref m_Mat4Trans);
			GL.Uniform3(Shader.PointLocation, Point);
			GL.UniformMatrix4(Shader.LocalRotationLocation, false, ref m_LocalMat4);
			GL.Uniform3(Shader.LocalRotationPointLocation, LocalRotationPoint);
			GL.Uniform3(Shader.LocalPositionLocation, LocalPosition);
			GL.Uniform3(Shader.BeforeLocalRotationLocation, BeforeLocalRotation);
			GL.Uniform3(Shader.AnimationPositionLocation, AnimationPosition);
			GL.UniformMatrix4(Shader.AnimationRotationLocation, false, ref m_RotationAnimationMatrix);
			GL.Uniform3(Shader.AnimationRotationPointLocation, AnimationRotationPoint);
			GL.Uniform4(Shader.TintUniform, Tint+BaseTint);
			GL.Uniform1(Shader.OutlineUniform, (Outline) ? 1 : 0);
			GL.Uniform2(Shader.ResolutionUniform, new Vector2(Constants.WIDTH, Constants.HEIGHT));
			GL.Uniform3(Shader.BakedPositionUniform, Vector3.Zero);
			GL.Uniform3(Shader.PlayerPositionUniform, Scenes.SceneManager.Game.LPlayer.Position);
			
			if(GameSettings.Shadows){
				GL.UniformMatrix4(Shader.ShadowMVPUniform, false, ref ShadowRenderer.ShadowMVP);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFBO.TextureID[0]);
				GL.Uniform1(Shader.ShadowTexUniform, 0);
                GL.Uniform1(Shader.ShadowDistanceUniform, ShadowRenderer.ShadowDistance);
			}
			GL.Uniform1(Shader.UseShadowsUniform, GameSettings.Shadows ? 1 : 0);
	           	
			if(Outline){
			//	GL.Enable(EnableCap.StencilTest);
			//	GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
			}
		}
		
		public override void UnBind(){
			Shader.UnBind();
			GL.Enable(EnableCap.CullFace);
			//GL.Disable(EnableCap.StencilTest);
		}
	}
}
