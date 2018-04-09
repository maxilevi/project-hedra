/*
 * Author: Zaphyk
 * Date: 04/03/2016
 * Time: 05:59 a.m.
 *
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of EntityMeshBuffer.
	/// </summary>
	public class ObjectMeshBuffer : ChunkMeshBuffer
	{
		public static ObjectMeshShader Shader { get; } = new ObjectMeshShader("Shaders/ObjectMesh.vert", "Shaders/ObjectMesh.frag");
		public bool ApplyFog { get; set; } = true;
		public float Alpha { get; set; } = 1;
		public Vector4 Tint { get; set; } = new Vector4(1,1,1,1);
		public Vector4 BaseTint { get; set; } = new Vector4(0,0,0,0);
	    public Vector3 Position { get; set; } = Vector3.Zero;
	    public Vector3 Scale { get; set; } = Vector3.One;
	    public Vector3 Point { get; set; }
	    public Vector3 LocalRotationPoint { get; set; }
	    public Vector3 LocalPosition { get; set; }
	    public Vector3 BeforeLocalRotation { get; set; }
	    public Vector3 AnimationPosition { get; set; }
	    public Vector3 AnimationRotationPoint { get; set; }

	    private bool _rotMatrixCached;
        private Vector3 _rotation = Vector3.Zero;
	    private Matrix4 _rotationMatrix;
	    private Vector3 _localRotation;
	    private Vector3 _animationRotation;
        private Matrix4 _localRotationMatrix = Matrix4.Identity;
        private Matrix4 _transformationMatrix = Matrix4.Identity;
	    private Matrix4 _animationRotationMatrix = Matrix4.Identity;

        public override void Draw(Vector3 Position, bool Shadows){
			if(Indices == null || Data == null) return;

		    this.Bind();
		    GL.Disable(EnableCap.Blend);
			
			if(Alpha < 0.9) GL.Enable(EnableCap.Blend);
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
			Vertex =  Vector3.TransformPosition(Vertex, _animationRotationMatrix);
			Vertex -= AnimationRotationPoint;
			
			Vertex += BeforeLocalRotation;
			Vertex += LocalRotationPoint;
			Vertex = Vector3.TransformPosition(Vertex, _localRotationMatrix);
			Vertex -= LocalRotationPoint;

		    Vertex = Vector3.TransformPosition(Vertex, TransformationMatrix);

            Vertex += AnimationPosition;
		
			Vertex += Point;
			Vertex = Vector3.TransformPosition(Vertex, RotationMatrix);
			Vertex -= Point;
			
			Vertex += Position;

            return Vertex;
		}
		
		public Matrix4 RotationMatrix{
			get{ 
				if(_rotMatrixCached) return _rotationMatrix;
                				
				_rotationMatrix = Matrix4.CreateRotationX(Rotation.X * Mathf.Radian);
				_rotationMatrix *= Matrix4.CreateRotationY(Rotation.Y * Mathf.Radian);
				_rotationMatrix *= Matrix4.CreateRotationZ(Rotation.Z * Mathf.Radian);
				_rotMatrixCached = true;
				return _rotationMatrix;
				
			}
		}

		public Vector3 Rotation{
			get{ return _rotation; }
			set{
				_rotation = value;			
				_rotMatrixCached = false;  
			}
		}

		public Matrix4 TransformationMatrix{
            get {
				return _transformationMatrix;
			} 
			set{
				_transformationMatrix = value;
			} 
		}
		public Vector3 LocalRotation{
			get{ return _localRotation; }
			set{
				_localRotation = value;
				_localRotationMatrix = Matrix4.CreateRotationX(value.X * Mathf.Radian);
				_localRotationMatrix *= Matrix4.CreateRotationY(value.Y * Mathf.Radian);
				_localRotationMatrix *= Matrix4.CreateRotationZ(value.Z * Mathf.Radian);
			}
		}
		

		public Vector3 AnimationRotation{
			get{ return _animationRotation; }
			set{
				_animationRotation = value;
				_animationRotationMatrix = Matrix4.CreateRotationX(value.X * Mathf.Radian);
				_animationRotationMatrix *= Matrix4.CreateRotationY(value.Y * Mathf.Radian);
				_animationRotationMatrix *= Matrix4.CreateRotationZ(value.Z * Mathf.Radian);
			}
		}
		
		public override void Bind(){
			Shader.Bind();

		    Matrix4 rotationMatrix = RotationMatrix;

            GL.Uniform1(Shader.AlphaUniform, Alpha);
			GL.Uniform3(Shader.ScaleUniform, Scale);
			GL.Uniform1(Shader.ApplyFogUniform, ApplyFog ? 1 : 0);
			GL.Uniform3(Shader.TransPos, Position);
			GL.UniformMatrix4(Shader.TransMatrixUniformLocation, false, ref rotationMatrix);
			GL.UniformMatrix4(Shader.MatrixUniformLocation, false, ref _transformationMatrix);
			GL.Uniform3(Shader.PointLocation, Point);
			GL.UniformMatrix4(Shader.LocalRotationLocation, false, ref _localRotationMatrix);
			GL.Uniform3(Shader.LocalRotationPointLocation, LocalRotationPoint);
			GL.Uniform3(Shader.LocalPositionLocation, LocalPosition);
			GL.Uniform3(Shader.BeforeLocalRotationLocation, BeforeLocalRotation);
			GL.Uniform3(Shader.AnimationPositionLocation, AnimationPosition);
			GL.UniformMatrix4(Shader.AnimationRotationLocation, false, ref _animationRotationMatrix);
			GL.Uniform3(Shader.AnimationRotationPointLocation, AnimationRotationPoint);
			GL.Uniform4(Shader.TintUniform, Tint+BaseTint);
			GL.Uniform2(Shader.ResolutionUniform, new Vector2(GameSettings.Width, GameSettings.Height));
			GL.Uniform3(Shader.BakedPositionUniform, Vector3.Zero);
			GL.Uniform3(Shader.PlayerPositionUniform, GameManager.Player.Position);
			
			if(GameSettings.Shadows){
				GL.UniformMatrix4(Shader.ShadowMvpUniform, false, ref ShadowRenderer.ShadowMVP);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFBO.TextureID[0]);
				GL.Uniform1(Shader.ShadowTexUniform, 0);
                GL.Uniform1(Shader.ShadowDistanceUniform, ShadowRenderer.ShadowDistance);
			}
			GL.Uniform1(Shader.UseShadowsUniform, GameSettings.Shadows ? 1 : 0);
		}
		
		public override void UnBind(){
			Shader.UnBind();
			GL.Enable(EnableCap.CullFace);
		}
	}
}
