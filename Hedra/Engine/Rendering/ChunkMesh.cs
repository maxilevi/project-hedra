/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 08:12 p.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        private readonly object _collisionLock = new object();
        private readonly List<InstanceData> _instanceElements;
        private readonly List<InstanceData> _lodedInstanceElements;
        private readonly HashSet<Vector2> _offsets = new HashSet<Vector2>();
        private readonly object _offsetsLock = new object();
        private readonly List<CollisionShape> CollisionBoxes = new List<CollisionShape>();
        public readonly List<VertexData> Elements = new List<VertexData>();

        public bool IsBuilded;
        public bool IsGenerated;

        public ChunkMesh(Vector3 Position, ObjectMeshBuffer Buffer)
        {
            this.Position = Position;
            _instanceElements = new List<InstanceData>();
            _lodedInstanceElements = new List<InstanceData>();
            this.Buffer = Buffer;
        }

        public bool BuildedOnce { get; set; }
        protected override Vector3 OcclusionMin => Min + Position;
        protected override Vector3 OcclusionMax => Max + Position;

        public Vector2[] Offsets
        {
            get
            {
                lock (_offsetsLock)
                {
                    return _offsets.ToArray();
                }
            }
        }

        public CollisionShape[] CollisionShapes
        {
            get
            {
                lock (_collisionLock)
                {
                    return CollisionBoxes.ToArray();
                }
            }
        }

        public InstanceData[] InstanceElements => _instanceElements.ToArray();

        public InstanceData[] LodAffectedInstanceElements => _lodedInstanceElements.ToArray();

        public ObjectMeshBuffer Buffer { get; }

        public bool Enabled { get; set; }
        public bool WasCulled { private get; set; }
        public bool PrematureCulling => false;
        public Vector3 Max { get; private set; }
        public Vector3 Min { get; private set; }
        public Vector3 Position { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            Buffer?.Dispose();
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
            DrawMesh(Buffer);
            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        private void DrawMesh(ObjectMeshBuffer MeshBuffer)
        {
            if (IsBuilded && IsGenerated && Enabled && MeshBuffer.Data != null) MeshBuffer.Draw();
        }

        public void DoDraw()
        {
            Buffer.DoDraw();
        }

        public void AddInstance(InstanceData Data, bool AffectedByLod = false)
        {
            if (!AffectedByLod)
                _instanceElements.Add(Data);
            else
                _lodedInstanceElements.Add(Data);
        }

        public void RemoveInstance(InstanceData Data)
        {
            _instanceElements.Remove(Data);
            _lodedInstanceElements.Remove(Data);
        }

        public void Add(CollisionShape Shape)
        {
            lock (_offsets)
            {
                lock (_collisionLock)
                {
                    for (var i = 0; i < Shape.Vertices.Length; ++i) _offsets.Add(World.ToChunkSpace(Shape.Vertices[i]));
                    CollisionBoxes.Add(Shape);
                }
            }
        }
    }
}