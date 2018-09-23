﻿/*
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
	public class ObjectMeshBuffer : IMeshBuffer
	{
		public VBO<Vector3> Vertices { get; set; }
		public VBO<Vector4> Colors { get; set; }
		public VBO<uint> Indices { get; set; }
		public VBO<Vector3> Normals { get; set; }
		public VAO<Vector3, Vector4, Vector3> Data { get; set; }
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

        public void Draw()
        {
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
                Renderer.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
                Renderer.StencilFunc(StencilFunction.Always, 1, 0xFF);
                Renderer.StencilMask(0xFF);*/
            }

            Renderer.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
            Renderer.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            if (Outline)
            {
                Renderer.Enable(EnableCap.Blend);
                /*Renderer.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                Renderer.StencilMask(0x00);*/
                Renderer.Disable(EnableCap.DepthTest);
                //GraphicsLayer.Disable(EnableCap.CullFace);
                Shader["Outline"] = this.Outline ? 1 : 0;
                Shader["OutlineColor"] = this.OutlineColor;
                Shader["Time"] = Time.IndependantDeltaTime;
                Renderer.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
                Renderer.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
                //Renderer.StencilMask(0xFF);
                Renderer.Enable(EnableCap.DepthTest);
                //GraphicsLayer.Enable(EnableCap.CullFace);
                //GraphicsLayer.Disable(EnableCap.StencilTest);
            }

            Renderer.Disable(EnableCap.Blend);
			Data.Unbind();
			
			Renderer.Disable(EnableCap.Blend);

			Unbind();
			
			if(Alpha < 1)
				Renderer.Disable(EnableCap.Blend);
		}
		
		public Vector3 TransformPoint(Vector3 Vertex)
        {
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
		
		public Matrix4 RotationMatrix
        {
			get
            { 
				if(_rotMatrixCached) return _rotationMatrix;
                				
				_rotationMatrix = Matrix4.CreateRotationX(Rotation.X * Mathf.Radian)
                    * Matrix4.CreateRotationY(Rotation.Y * Mathf.Radian)
                    * Matrix4.CreateRotationZ(Rotation.Z * Mathf.Radian);
				_rotMatrixCached = true;
				return _rotationMatrix;
				
			}
		}

		public Vector3 Rotation
        {
			get => _rotation;
		    set
            {
				_rotation = value;			
				_rotMatrixCached = false;  
			}
		}

		public Matrix4 TransformationMatrix
        {
            get => _transformationMatrix;
		    set => _transformationMatrix = value;
		}
		public Vector3 LocalRotation
        {
			get => _localRotation;
		    set
            {
				_localRotation = value;
				_localRotationMatrix = Matrix4.CreateRotationX(value.X * Mathf.Radian)
                    * Matrix4.CreateRotationY(value.Y * Mathf.Radian)
                    * Matrix4.CreateRotationZ(value.Z * Mathf.Radian);
			}
		}
		

		public Vector3 AnimationRotation
        {
			get => _animationRotation;
		    set
            {
				_animationRotation = value;
				_animationRotationMatrix = Matrix4.CreateRotationX(value.X * Mathf.Radian)
                    * Matrix4.CreateRotationY(value.Y * Mathf.Radian)
                    * Matrix4.CreateRotationZ(value.Z * Mathf.Radian);
			}
		}
		
		public void Bind()
		{
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

		    Renderer.ActiveTexture(TextureUnit.Texture1);
		    Renderer.BindTexture(TextureTarget.Texture3D, NoiseTexture.Id);
            Shader["noiseTexture"] = 1;           
            Shader["useNoiseTexture"] = UseNoiseTexture ? 1f : 0f;
            
            if (GameSettings.Shadows){
				Shader["ShadowMVP"] = ShadowRenderer.ShadowMvp;
				Renderer.ActiveTexture(TextureUnit.Texture0);
				Renderer.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFbo.TextureID[0]);
				Shader["ShadowTex"] = 0;
                Shader["ShadowDistance"] = ShadowRenderer.ShadowDistance;
			}
			Shader["UseShadows"] = GameSettings.Shadows ? 1 : 0;
		}
		
		public void Unbind()
		{
			Shader.Unbind();
			Renderer.Enable(EnableCap.CullFace);
		}

		public void Dispose()
		{
			Vertices?.Dispose();
			Colors?.Dispose();
			Indices?.Dispose();
			Normals?.Dispose();
			Data?.Dispose();
		}
	}
}
