/*
 * Author: Zaphyk
 * Date: 04/03/2016
 * Time: 05:59 a.m.
 *
 */
using System;
using System.Windows.Forms.VisualStyles;
using Hedra.Engine.Generation;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of EntityMeshBuffer.
	/// </summary>
	internal class ObjectMeshBuffer : ChunkMeshBuffer
	{
		public static Shader Shader { get; }
		public bool ApplyFog { get; set; } = true;
		public float Alpha { get; set; } = 1;
	    public bool UseNoiseTexture { get; set; }
	    public bool Dither { get; set; }
	    public bool Outline { get; set; }
        public bool Pause { get; set; }
        public Vector4 OutlineColor { get; set; }
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

	    private static readonly Texture3D NoiseTexture;
	    private bool _rotMatrixCached;
        private Vector3 _rotation = Vector3.Zero;
	    private Matrix4 _rotationMatrix;
	    private Vector3 _localRotation;
	    private Vector3 _animationRotation;
        private Matrix4 _localRotationMatrix = Matrix4.Identity;
        private Matrix4 _transformationMatrix = Matrix4.Identity;
	    private Matrix4 _animationRotationMatrix = Matrix4.Identity;

	    static ObjectMeshBuffer()
	    {
	        Shader = Shader.Build("Shaders/ObjectMesh.vert", "Shaders/ObjectMesh.frag");
	        var noiseValues = new float[16, 16, 16];
	        for (var x = 0; x < noiseValues.GetLength(0); x++)
	        {
	            for (var y = 0; y < noiseValues.GetLength(1); y++)
	            {
	                for (var z = 0; z < noiseValues.GetLength(2); z++)
	                {
	                    noiseValues[x, y, z] = (float)OpenSimplexNoise.Evaluate(x * 0.6f, y * 0.6f, z * 0.6f) * .5f + .5f;
	                }
	            }
	        }
	        NoiseTexture = new Texture3D(noiseValues);
        }

        public override void Draw(Vector3 Position, bool Shadows){
			if(Indices == null || Data == null) return;

		    this.Bind();
		    Renderer.Disable(EnableCap.Blend);
			
			if(Alpha < 0.9) Renderer.Enable(EnableCap.Blend);
			Renderer.Enable(EnableCap.DepthTest);
			
			Data.Bind();

            Shader["Outline"] = 0;
            if (Outline)
            {
                /*GraphicsLayer.Enable(EnableCap.StencilTest);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
                GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                GL.StencilMask(0xFF);*/
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
            GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            if (Outline)
            {
                Renderer.Enable(EnableCap.Blend);
                /*GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                GL.StencilMask(0x00);*/
                Renderer.Disable(EnableCap.DepthTest);
                //GraphicsLayer.Disable(EnableCap.CullFace);
                Shader["Outline"] = this.Outline ? 1 : 0;
                Shader["OutlineColor"] = this.OutlineColor;
                Shader["Time"] = Time.IndependantDeltaTime;
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
                GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
                //GL.StencilMask(0xFF);
                Renderer.Enable(EnableCap.DepthTest);
                //GraphicsLayer.Enable(EnableCap.CullFace);
                //GraphicsLayer.Disable(EnableCap.StencilTest);
            }

            Renderer.Disable(EnableCap.Blend);
			Data.Unbind();
			
			Renderer.Disable(EnableCap.Blend);

			UnBind();
			
			if(Alpha < 1)
				Renderer.Disable(EnableCap.Blend);
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

            Shader["Alpha"] = Alpha;
			Shader["Scale"] = Scale;
		    Shader["Dither"] = Dither ? 1 : 0;
			Shader["UseFog"] = ApplyFog ? 1 : 0;
			Shader["TransPos"] = Position;
			Shader["TransMatrix"] = RotationMatrix;
			Shader["Matrix"] = _transformationMatrix;
			Shader["Point"] = Point;
			Shader["LocalRotation"] = _localRotationMatrix;
			Shader["LocalRotationPoint"] = LocalRotationPoint;
			Shader["LocalPosition"] = LocalPosition;
			Shader["BeforeLocalRotation"] = BeforeLocalRotation;
			Shader["AnimationPosition"] = AnimationPosition;
			Shader["AnimationRotation"] = _animationRotationMatrix;
			Shader["AnimationRotationPoint"] = AnimationRotationPoint;
			Shader["Tint"] = Tint;
		    Shader["BaseTint"] = BaseTint;
			Shader["BakedPosition"] = Vector3.Zero;
			Shader["PlayerPosition"] = GameManager.Player.Position;

		    GL.ActiveTexture(TextureUnit.Texture1);
		    GL.BindTexture(TextureTarget.Texture3D, NoiseTexture.Id);
            Shader["noiseTexture"] = 1;           
            Shader["useNoiseTexture"] = UseNoiseTexture ? 1f : 0f;
            
            if (GameSettings.Shadows){
				Shader["ShadowMVP"] = ShadowRenderer.ShadowMvp;
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFbo.TextureID[0]);
				Shader["ShadowTex"] = 0;
                Shader["ShadowDistance"] = ShadowRenderer.ShadowDistance;
			}
			Shader["UseShadows"] = GameSettings.Shadows ? 1 : 0;
		}
		
		public override void UnBind(){
			Shader.Unbind();
			Renderer.Enable(EnableCap.CullFace);
		}
	}
}
