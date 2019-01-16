/*
 * Author: Zaphyk
 * Date: 04/03/2016
 * Time: 05:59 a.m.
 *
 */
using System;
using Hedra.Core;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

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
        public Vector3 RotationPoint { get; set; }
        public Vector3 LocalRotationPoint { get; set; }
        public Vector3 LocalPosition { get; set; }
        public Vector3 BeforeRotation { get; set; }

        private static readonly Texture3D NoiseTexture;
        private bool _rotMatrixCached;
        private bool _disposed;
        private Vector3 _localRotation;
        private Matrix3 _localRotationMatrix;
        private Vector3 _rotation;
        private Matrix3 _rotationMatrix = Matrix3.Identity;

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
            if (Indices == null || Data == null) return;
            if (_disposed) throw new AccessViolationException($"Cannot draw a disposed object");

            this.Bind();
            Renderer.Disable(EnableCap.Blend);
            
            if(Alpha < 0.9) Renderer.Enable(EnableCap.Blend);
            Renderer.Enable(EnableCap.DepthTest);
            
            Data.Bind();

            Renderer.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
            Renderer.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            if (Outline)
            {
                Renderer.Enable(EnableCap.Blend);
                Renderer.Disable(EnableCap.DepthTest);
                Shader["Outline"] = this.Outline ? 1 : 0;
                Shader["OutlineColor"] = this.OutlineColor;
                Shader["Time"] = Time.IndependantDeltaTime;
                Renderer.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
                Renderer.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
                Renderer.Enable(EnableCap.DepthTest);
            }
            Shader["Outline"] = 0;

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

            Vertex += LocalRotationPoint;
            Vertex = Vector3.Transform(Vertex, LocalRotationMatrix);
            Vertex -= LocalRotationPoint;

            Vertex += BeforeRotation;
            Vertex += RotationPoint;
            Vertex = Vector3.Transform(Vertex, _rotationMatrix);
            Vertex -= RotationPoint;

            Vertex = Vector3.TransformPosition(Vertex, TransformationMatrix);
            
            Vertex += Position + LocalPosition;

            return Vertex;
        }

        private Matrix3 LocalRotationMatrix
        {
            get
            { 
                if(_rotMatrixCached) return _localRotationMatrix;
                _localRotationMatrix = Matrix3.CreateRotationY(_localRotation.Y * Mathf.Radian)
                    * Matrix3.CreateRotationX(_localRotation.X * Mathf.Radian)
                    * Matrix3.CreateRotationZ(_localRotation.Z * Mathf.Radian);
                _rotMatrixCached = true;
                return _localRotationMatrix;
                
            }
        }

        public Vector3 LocalRotation
        {
            get => _localRotation;
            set
            {
                if(_localRotation == value) return;
                _localRotation = value;            
                _rotMatrixCached = false;  
            }
        }

        public Matrix4 TransformationMatrix { get; set; } = Matrix4.Identity;

        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _rotationMatrix = Matrix3.CreateRotationX(value.X * Mathf.Radian)
                    * Matrix3.CreateRotationY(value.Y * Mathf.Radian)
                    * Matrix3.CreateRotationZ(value.Z * Mathf.Radian);
            }
        }

        private void Bind()
        {
            Shader.Bind();

            Shader["Alpha"] = Alpha;
            Shader["Scale"] = Scale;
            Shader["Position"] = Position + LocalPosition;
            Shader["LocalRotationMatrix"] = LocalRotationMatrix;
            Shader["TransformationMatrix"] = TransformationMatrix;
            Shader["RotationPoint"] = RotationPoint;
            Shader["RotationMatrix"] = _rotationMatrix;
            Shader["LocalRotationPoint"] = LocalRotationPoint;
            Shader["BeforeRotation"] = BeforeRotation;
            Shader["Tint"] = Tint;
            Shader["BaseTint"] = BaseTint;
            Shader["PlayerPosition"] = GameManager.Player.Position;

            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture3D, NoiseTexture.Id);
            Shader["noiseTexture"] = 1;      
            
            if (GameSettings.Shadows)
            {
                Shader["ShadowMVP"] = ShadowRenderer.ShadowMvp;
                Renderer.ActiveTexture(TextureUnit.Texture0);
                Renderer.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFbo.TextureId[0]);
                Shader["ShadowTex"] = 0;
                Shader["ShadowDistance"] = ShadowRenderer.ShadowDistance;
            }
            Shader["DitherFogTextureShadows"] = new Vector4(Dither ? 1 : 0, ApplyFog ? 1 : 0, UseNoiseTexture ? 1 : 0, GameSettings.Shadows ? 1 : 0);
        }

        private static void Unbind()
        {
            
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture3D, 0);
            Shader.Unbind();
            Renderer.Enable(EnableCap.CullFace);
        }

        public void Dispose()
        {
            _disposed = true;
            Vertices?.Dispose();
            Colors?.Dispose();
            Indices?.Dispose();
            Normals?.Dispose();
            Data?.Dispose();
        }
    }
}
