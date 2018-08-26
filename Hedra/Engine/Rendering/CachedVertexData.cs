using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public class CachedVertexData : IVertexData, IDisposable
    {
        public List<Vector3> Vertices { get; set; }
        public List<Vector4> Colors { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<uint> Indices { get; set; }
        public List<float> Extradata { get; set; }
        private Vector3 _bounds;
        private Vector3 _position;
        private bool _positionInitialized;
        private bool _boundsInitialized;

        public Vector3 Position
        {
            get
            {
                if (_positionInitialized) return _position;
                var sum = Vector3.Zero;
                for (var i = 0; i < Vertices.Count; i++)
                {
                    sum += Vertices[i];
                }
                _position = sum / Vertices.Count;
                _positionInitialized = true;
                return _position;
            }
        }

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
        
        private Vector3 SupportPoint(Vector3 Direction)
        {
            var highest = float.MinValue;
            var support = Vector3.Zero;
            for (var i = Vertices.Count-1; i > -1; i--)
            {
                var dot = Vector3.Dot(Direction, Vertices[i]);
                if (dot < highest) continue;
                highest = dot;
                support = Vertices[i];
            }

            return support;
        }
        
        public VertexData ToVertexData()
        {
            return ToVertexData(this);
        }

        public void Dispose()
        {
            Vertices.Clear();
            Colors.Clear();
            Normals.Clear();
            Indices.Clear();
            Extradata.Clear();
        }

        public static CachedVertexData FromVertexData(VertexData Data)
        {
            return new CachedVertexData
            {
                Vertices = Data.Vertices,
                Colors = Data.Colors,
                Normals = Data.Normals,
                Indices = Data.Indices,
                Extradata = Data.Extradata
            };
        }

        public static VertexData ToVertexData(CachedVertexData Data)
        {
            return new VertexData
            {
                Vertices = Data.Vertices,
                Colors = Data.Colors,
                Normals = Data.Normals,
                Indices = Data.Indices,
                Extradata = Data.Extradata
            }.Clone();
        }
    }
}