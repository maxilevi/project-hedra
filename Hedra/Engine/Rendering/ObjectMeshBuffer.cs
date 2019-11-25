/*
 * Author: Zaphyk
 * Date: 04/03/2016
 * Time: 05:59 a.m.
 *
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Game;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of EntityMeshBuffer.
    /// </summary>
    public class ObjectMeshBuffer : IMeshBuffer
    {
        public VBO<Vector3> Vertices { get; private set; }
        public VBO<Vector4> Colors { get; private set; }
        public VBO<uint> Indices { get; private set; }
        public VBO<Vector3> Normals { get; private set; }
        public VAO<Vector3, Vector4, Vector3> Data { get; private set; }
        private static Shader Shader { get; }
        private static UBO<ObjectMeshBufferData> UBO { get; }
        public bool ApplyFog { get; set; } = true;
        public float Alpha { get; set; } = 1;
        public bool UseNoiseTexture { get; set; }
        public bool Dither { get; set; }
        public bool Outline { get; set; }
        public bool Pause { get; set; }
        public bool ApplySSAO { get; set; } = true;
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
        private Matrix4x4 _localRotationMatrix;
        private Vector3 _rotation;
        private Matrix4x4 _rotationMatrix = Matrix4x4.Identity;

        static ObjectMeshBuffer()
        {
            UBO = new UBO<ObjectMeshBufferData>("ObjectAttributes");
            Shader = Shader.Build("Shaders/ObjectMesh.vert", "Shaders/ObjectMesh.frag");
            UBO.RegisterShader(Shader);
            var noiseValues = WorldRenderer.CreateNoiseArray(out var width, out var height, out var depth);
            NoiseTexture = new Texture3D(noiseValues, width, height, depth);
        }

        public ObjectMeshBuffer(VertexData ModelData)
        {
            if(ModelData.IsEmpty) return;
            ModelData.AssertTriangulated();
            Executer.ExecuteOnMainThread(() =>
            {
                Vertices = new VBO<Vector3>(ModelData.Vertices.ToArray(), ModelData.Vertices.Count * HedraSize.Vector3, VertexAttribPointerType.Float);
                Colors = new VBO<Vector4>(ModelData.Colors.ToArray(), ModelData.Colors.Count * HedraSize.Vector4, VertexAttribPointerType.Float);
                Normals = new VBO<Vector3>(ModelData.Normals.ToArray(), ModelData.Normals.Count * HedraSize.Vector3, VertexAttribPointerType.Float);
                Indices = new VBO<uint>(ModelData.Indices.ToArray(), ModelData.Indices.Count * sizeof(uint), VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
                Data = new VAO<Vector3, Vector4, Vector3>(Vertices, Colors, Normals);
            });
        }

        public void Draw()
        {
            Bind();

            DoDraw();

            Unbind();
        }

        public void DoDraw()
        {
            if (Indices == null || Data == null) return;
            if (_disposed) throw new AccessViolationException($"Cannot draw a disposed object");
            
            Renderer.Disable(EnableCap.Blend);
            
            if(Alpha < 0.9)
                Renderer.Enable(EnableCap.Blend);
            else
                Renderer.Disable(EnableCap.Blend);
            Renderer.Enable(EnableCap.DepthTest);
            UpdateUniforms();

            Data.Bind();
            Indices.Bind();
            Renderer.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
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

            Vertex = Vector3.Transform(Vertex, TransformationMatrix);
            
            Vertex += Position + LocalPosition;
            return Vertex;
        }

        private Matrix4x4 LocalRotationMatrix
        {
            get
            { 
                if(_rotMatrixCached) return _localRotationMatrix;
                _localRotationMatrix = Matrix4x4.CreateRotationY(_localRotation.Y * Mathf.Radian)
                    * Matrix4x4.CreateRotationX(_localRotation.X * Mathf.Radian)
                    * Matrix4x4.CreateRotationZ(_localRotation.Z * Mathf.Radian);
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

        public Matrix4x4 TransformationMatrix { get; set; } = Matrix4x4.Identity;

        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _rotationMatrix = Matrix4x4.CreateRotationX(value.X * Mathf.Radian)
                    * Matrix4x4.CreateRotationY(value.Y * Mathf.Radian)
                    * Matrix4x4.CreateRotationZ(value.Z * Mathf.Radian);
            }
        }
        
        public static void DrawBatched(List<ObjectMesh> Meshes)
        {
            BindDrawBatched();
            
            for (var i = 0; i < Meshes.Count; ++i)
            {
                if(Meshes[i].Enabled && Culling.IsInside(Meshes[i]))
                    Meshes[i].Mesh.Buffer.DoDraw();
            }

            var isBound = true;
            for (var i = 0; i < Meshes.Count && isBound; ++i)
            {
                if (Meshes[i].Mesh.Buffer.Data != null)
                {
                    Meshes[i].Mesh.Buffer.Unbind();
                    isBound = false;
                }
            }
        }

        private static void BindDrawBatched()
        {
            Shader.Bind();
            
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
        }


        private void UpdateUniforms()
        {
            UBO.Update(new ObjectMeshBufferData
            {
                Alpha = Alpha,
                Scale = Scale,
                Position = Position + LocalPosition,
                LocalRotationMatrix = LocalRotationMatrix,
                TransformationMatrix = TransformationMatrix,
                RotationPoint = RotationPoint,
                RotationMatrix = _rotationMatrix,
                LocalRotationPoint = LocalRotationPoint,
                BeforeRotation = BeforeRotation,
                Tint = Tint,
                BaseTint = BaseTint,
                PlayerPosition = GameManager.Player.Position,
                IgnoreSSAO = ApplySSAO ? 0 : 1,
                DitherFogTextureShadows = new Vector4(Dither ? 1 : 0, ApplyFog ? 1 : 0, UseNoiseTexture ? 1 : 0, GameSettings.Shadows ? 1 : 0)
            });
            Shader["Outline"] = this.Outline ? 1 : 0;
            Shader["OutlineColor"] = this.OutlineColor;
            Shader["Time"] = Time.IndependentDeltaTime;
        }

        private void Bind()
        {
            BindDrawBatched();
        }

        private void Unbind()
        {
            Data.Unbind();
            Indices.Unbind();
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture3D, 0);
            Renderer.Enable(EnableCap.CullFace);
            Renderer.Disable(EnableCap.Blend);
            Shader.Unbind();
        }

        public void Dispose()
        {
            _disposed = true;
            void DoDispose()
            {
                Vertices?.Dispose();
                Colors?.Dispose();
                Indices?.Dispose();
                Normals?.Dispose();
                Data?.Dispose();
            }

            if (Data == null)
                Executer.ExecuteOnMainThread(DoDispose);
            else
                DoDispose();
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ObjectMeshBufferData
    {
        [FieldOffset(0)]
        public Matrix4x4 RotationMatrix;
        [FieldOffset(64)]
        public Matrix4x4 LocalRotationMatrix;
        [FieldOffset(128)]
        public Matrix4x4 TransformationMatrix;
        [FieldOffset(192)]
        public float Alpha;
        [FieldOffset(208)]
        public Vector3 Scale;
        [FieldOffset(224)]
        public Vector3 Position;
        [FieldOffset(240)]
        public Vector3 RotationPoint;
        [FieldOffset(256)]
        public Vector3 LocalRotationPoint;
        [FieldOffset(272)]
        public Vector3 BeforeRotation;
        [FieldOffset(288)]
        public Vector4 Tint;
        [FieldOffset(304)]
        public Vector4 BaseTint;
        [FieldOffset(320)]
        public Vector3 PlayerPosition;
        [FieldOffset(332)]
        public int IgnoreSSAO;
        [FieldOffset(336)]
        public Vector4 DitherFogTextureShadows;

        public override bool Equals(object obj)
        {
            if(obj is ObjectMeshBufferData other)
                return Alpha.Equals(other.Alpha) && Scale.Equals(other.Scale) && Position.Equals(other.Position) && LocalRotationMatrix.Equals(other.LocalRotationMatrix) &&
                       TransformationMatrix.Equals(other.TransformationMatrix) && RotationPoint.Equals(other.RotationPoint) && RotationMatrix.Equals(other.RotationMatrix) &&
                       LocalRotationPoint.Equals(other.LocalRotationPoint) && BeforeRotation.Equals(other.BeforeRotation) && Tint.Equals(other.Tint) && BaseTint.Equals(other.BaseTint) &&
                       PlayerPosition.Equals(other.PlayerPosition) && IgnoreSSAO.Equals(other.IgnoreSSAO) && DitherFogTextureShadows.Equals(other.DitherFogTextureShadows);
            return false;
        }
    }
}
