/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 08:12 p.m.
 *
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hedra.Engine.Game;
using System.Numerics;
using Hedra.Engine.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering
{
    public class ChunkMesh : Occludable, ICullable, IDisposable
    {
        private ObjectMeshBuffer _buffer;
        private readonly List<InstanceData> _instanceElements;
        private readonly List<InstanceData> _lodedInstanceElements;
        public readonly List<CollisionShape> CollisionBoxes = new List<CollisionShape>();
        public readonly List<VertexData> Elements = new List<VertexData>();

        public bool IsBuilded;
        public bool IsGenerated;
        public bool Enabled { get; set; }
        public bool WasCulled { private get; set; }
        public bool PrematureCulling => false;
        public bool BuildedOnce { get; set; }
        public Vector3 Max { get; private set; }
        public Vector3 Min { get; private set; }
        public Vector3 Position { get; private set; }
        protected override Vector3 OcclusionMin => Min + Position;
        protected override Vector3 OcclusionMax => Max + Position;

        public ChunkMesh(Vector3 Position, ObjectMeshBuffer Buffer)
        {
            this.Position = Position;
            _instanceElements = new List<InstanceData>();
            _lodedInstanceElements = new List<InstanceData>();
            _buffer = Buffer;
        }

        public void SetBounds(Vector3 Min, Vector3 Max)
        {
            this.Max = Max - Min.Y * Vector3.UnitY;
            this.Min = Min - Min.Y * Vector3.UnitY;
            Position = new Vector3(Position.X, Min.Y, Position.Z);
        }

        public void Draw()
        {
            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            DrawMesh(_buffer);
            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        private void DrawMesh(ObjectMeshBuffer MeshBuffer)
        {
            if (IsBuilded && IsGenerated && Enabled && MeshBuffer.Data != null)
            {
                MeshBuffer.Draw();
            }
        }

        public void DoDraw()
        {
            _buffer.DoDraw();
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

        public ObjectMeshBuffer Buffer => _buffer;
        
        public override void Dispose()
        {
            base.Dispose();
            _buffer?.Dispose();
        }
    }
}
