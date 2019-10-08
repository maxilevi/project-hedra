using System.Collections.Generic;
using Hedra.Engine.Core;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Rendering;
using Microsoft.Scripting.Utils;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public abstract class BaseVertexData<T> : LodableObject<T> where T : class
    {
        public abstract List<Vector3> Vertices { get; set; }
        public abstract List<Vector3> Normals { get; set; }
        public abstract List<Vector4> Colors { get; set; }
        public abstract List<uint> Indices { get; set; }
        public abstract List<float> Extradata { get; set; }
        
        public bool HasColors => Colors.Count != 0;
        public bool HasExtradata => Extradata.Count == Vertices.Count;
        
        public static void TrimExcess<T>(List<T> List)
        {
            var excess = (List.Count % 3);
            if (excess == 0) return;
            for (var i = 0; i < (3-excess); ++i)
            {
                List.Add(List[List.Count - 1]);
            }
        }
        
        public void Trim()
        {
            TrimExcess(Vertices);
            TrimExcess(Colors);
            TrimExcess(Normals);
            TrimExcess(Indices);
            TrimExcess(Extradata);
        }
        
        public Vector3 SupportPoint(Vector3 Direction)
        {
            return MeshOperations.SupportPoint(Vertices, Colors, Direction);
        }

        protected Vector3 SupportPoint(Vector3 Direction, Vector4 Color)
        {
            return MeshOperations.SupportPoint(Vertices, Colors, Direction, Color);
        }

        public void Flat(IAllocator Allocator)
        {
            MeshOperations.FlatMesh(Allocator, Indices, Vertices, Normals, Colors, Extradata);
        }
        
        public void UniqueVertices()
        {
            MeshOperations.UniqueVertices(Indices, Vertices, Normals, Colors, Extradata);
        }

        public void Flat()
        {
            MeshOperations.FlatMesh(Indices, Vertices, Normals, Colors, Extradata);
        }
    }
}