using System;
using System.Numerics;
using Hedra.Engine.Core;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering
{
    public class CachedVertexData : CompressedVertexData, IDisposable, IPositionable, ISearchable
    {
        private Vector3 _bounds;
        private bool _boundsInitialized;
        private VertexData _cache;
        private Vector3 _position;
        private bool _positionInitialized;
        public string Name { get; set; }

        public Vector3 Bounds
        {
            get
            {
                if (_boundsInitialized) return _bounds;
                _bounds.X = SupportPoint(Vector3.UnitX).X - SupportPoint(-Vector3.UnitX).X;
                _bounds.Y = SupportPoint(Vector3.UnitY).Y - SupportPoint(-Vector3.UnitY).Y;
                _bounds.Z = SupportPoint(Vector3.UnitZ).Z - SupportPoint(-Vector3.UnitZ).Z;
                _boundsInitialized = true;
                return _bounds;
            }
        }

        public VertexData VertexData
        {
            get
            {
                if (_cache == null)
                    _cache = new VertexData
                    {
                        Vertices = Vertices,
                        Colors = Colors,
                        Normals = Normals,
                        Indices = Indices,
                        Extradata = Extradata
                    };
                return _cache;
            }
        }

        public void Dispose()
        {
            Vertices.Clear();
            Colors.Clear();
            Normals.Clear();
            Indices.Clear();
            Extradata.Clear();
        }

        public Vector3 Position
        {
            get
            {
                if (_positionInitialized) return _position;
                var sum = Vector3.Zero;
                var verts = Vertices;
                for (var i = 0; i < verts.Count; i++) sum += verts[i];
                _position = sum / verts.Count;
                _positionInitialized = true;
                return _position;
            }
        }

        private Vector3 SupportPoint(Vector3 Direction)
        {
            var highest = float.MinValue;
            var support = Vector3.Zero;
            var verts = Vertices;
            for (var i = verts.Count - 1; i > -1; i--)
            {
                var dot = Vector3.Dot(Direction, verts[i]);
                if (dot < highest) continue;
                highest = dot;
                support = verts[i];
            }

            return support;
        }

        public static CachedVertexData FromVertexData(VertexData Data)
        {
            return new CachedVertexData
            {
                Vertices = Data.Vertices,
                Colors = Data.Colors,
                Normals = Data.Normals,
                Indices = Data.Indices,
                Extradata = Data.Extradata,
                Name = Data.Name
            };
        }
    }
}