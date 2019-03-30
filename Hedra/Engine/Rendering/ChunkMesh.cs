/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 08:12 p.m.
 *
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Game;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering
{
    public class ChunkMesh : Occludable, ICullable, IDisposable
    {
        private IMeshBuffer _buffer;
        private readonly List<InstanceData> _instanceElements;
        private readonly List<InstanceData> _lodedInstanceElements;
        public List<ICollidable> CollisionBoxes = new List<ICollidable>();
        public List<VertexData> Elements = new List<VertexData>();
        public VertexData ModelData { get; set; }

        public bool IsBuilded;
        public bool IsGenerated;
        public bool Enabled { get; set; }
        public bool WasCulled { private get; set; }
        public bool PrematureCulling => false;
        public bool BuildedOnce { get; set; }
        public Vector3 Max { get; private set; }
        public Vector3 Min { get; private set; }
        public Vector3 Position { get; }
        protected override Vector3 OcclusionMin => Min + Position;
        protected override Vector3 OcclusionMax => Max + Position;

        public ChunkMesh(Vector3 Position, IMeshBuffer Buffer)
        {
            this.Position = Position;
            _instanceElements = new List<InstanceData>();
            _lodedInstanceElements = new List<InstanceData>();
            _buffer = Buffer;
        }

        public void SetBounds(Vector3 Min, Vector3 Max)
        {
            this.Max = Max;
            this.Min = Min;
        }

        public void BuildFrom(VertexData Data, bool ExtraData)
        {
            try
            {
                if (Data?.Colors == null)
                    return;

                Vector4[] colorBuffer;
                if (ExtraData)
                {
                    colorBuffer = new Vector4[Data.Colors.Count];
                    for (var i = 0; i < colorBuffer.Length; i++)
                    {
                        colorBuffer[i] = new Vector4(Data.Colors[i].Xyz, Data.Extradata[i]);
                    }
                }
                else
                {
                    colorBuffer = Data.Colors.ToArray();
                }

                var colorBufferSize = (colorBuffer.Length * Vector4.SizeInBytes);
                var vertexBufferSize = (Data.Vertices.Count * Vector3.SizeInBytes);
                var indexBufferSize = (Data.Indices.Count * sizeof(int));
                var normalBufferSize = (Data.Normals.Count * Vector3.SizeInBytes);

                if (_buffer.Vertices == null)
                    _buffer.Vertices = new VBO<Vector3>(Data.Vertices.ToArray(), vertexBufferSize,
                        VertexAttribPointerType.Float);
                else
                    _buffer.Vertices.Update(Data.Vertices.ToArray(), vertexBufferSize);

                if (_buffer.Indices == null)
                    _buffer.Indices = new VBO<uint>(Data.Indices.ToArray(), indexBufferSize,
                        VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
                else
                    _buffer.Indices.Update(Data.Indices.ToArray(), indexBufferSize);


                if (_buffer.Colors == null)
                    _buffer.Colors = new VBO<Vector4>(colorBuffer, colorBufferSize, VertexAttribPointerType.Float);
                else
                    _buffer.Colors.Update(colorBuffer, colorBufferSize);

                if (_buffer.Normals == null)
                    _buffer.Normals = new VBO<Vector3>(Data.Normals.ToArray(), normalBufferSize,
                        VertexAttribPointerType.Float);
                else
                    _buffer.Normals.Update(Data.Normals.ToArray(), normalBufferSize);

                if (_buffer.Data == null)
                    _buffer.Data =
                        new VAO<Vector3, Vector4, Vector3>(_buffer.Vertices, _buffer.Colors, _buffer.Normals);

                IsBuilded = true;
                Enabled = true;
                BuildedOnce = true;
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
            }
        }

        public void Draw()
        {
            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            DrawMesh(_buffer);
            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        private void DrawMesh(IMeshBuffer MeshBuffer)
        {
            if (IsBuilded && IsGenerated && Enabled && MeshBuffer.Data != null)
            {
                MeshBuffer.Draw();
            }
        }

        public void AddInstance(InstanceData Data, bool AffectedByLod = false)
        {
            if(!AffectedByLod)
                _instanceElements.Add(Data);
            else
                _lodedInstanceElements.Add(Data);
        }
        
        public void RemoveInstance(InstanceData Data)
        {
            _instanceElements.Remove(Data);
            _lodedInstanceElements.Remove(Data);
        }

        public InstanceData[] InstanceElements => _instanceElements.ToArray();

        public InstanceData[] LodAffectedInstanceElements => _lodedInstanceElements.ToArray();
        
        public override void Dispose()
        {
            base.Dispose();
            _buffer?.Dispose();
        }
    }
}
