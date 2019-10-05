using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Rendering.Geometry
{
    public static class MeshAnalyzer
    {

        private struct VertexRemap
        {
            public Vector3 Position;
            public Vector3 Normal;
            public int Count;
        }
        
        /*
         * Reference: https://docs.blender.org/manual/en/latest/modeling/modifiers/deform/smooth.html
         */
        public static void ApplySmoothing(VertexData Mesh, HashSet<uint> IgnoreList)
        {
            var map = IndexVertices(Mesh);
            var vertexRemap = new Dictionary<Vector3, VertexRemap>();

            void AddWeight(Vector3 Vertex, Vertex Neighbour)
            {
                var remap = vertexRemap[Vertex];
                vertexRemap[Vertex] = new VertexRemap
                {
                    Position = remap.Position + Neighbour.Position,
                    Normal = remap.Normal + Neighbour.Normal,
                    Count = remap.Count + 1,
                };
            }
            
            for (var i = 0; i < Mesh.Vertices.Count; ++i)
            {
                var vertex = Mesh.Vertices[i];
                var triangles = map[vertex].ToArray();
                if (!vertexRemap.ContainsKey(vertex))
                    vertexRemap[vertex] = new VertexRemap();

                for (var j = 0; j < triangles.Length; ++j)
                {
                    var neighbours = triangles[j].GetConnected(vertex);
                    for (var k = 0; k < neighbours.Length; ++k)
                    {
                        AddWeight(vertex, neighbours[k]);
                    }
                }
            }

            for (var i = 0; i < Mesh.Vertices.Count; ++i)
            {
                var remap = vertexRemap[Mesh.Vertices[i]];
                Mesh.Normals[i] = remap.Normal / remap.Count;
                if(IgnoreList.Contains((uint)i)) continue;
                Mesh.Vertices[i] = remap.Position / remap.Count;
            }
        }
        
        public static VertexData[] GetConnectedComponents(VertexData Mesh)
        {
            var list = new List<VertexData>();
            if (Mesh.Vertices.Count == 0) return list.ToArray();
            var map = IndexVertices(Mesh);
            var remaining = map.Keys.ToArray();
            var visited = new HashSet<Vector3>();
            
            void DepthFirstSearch(Vector3 Vertex, HashSet<Triangle> Component)
            {
                if (visited.Contains(Vertex)) return;
                visited.Add(Vertex);
                var triangles = map[Vertex].ToArray();
                for (var i = 0; i < triangles.Length; ++i)
                {
                    if (!Component.Contains(triangles[i]))
                        Component.Add(triangles[i]);
                }
                var connected = triangles.SelectMany(T => T.GetConnected(Vertex)).ToArray();
                for (var i = 0; i < connected.Length; ++i)
                {
                    DepthFirstSearch(connected[i].Position, Component);
                }
            }

            for (var i = 0; i < remaining.Length; ++i)
            {
                if(visited.Contains(remaining[i])) continue;
                var component = new HashSet<Triangle>();
                DepthFirstSearch(remaining[i], component);

                var mesh = BuildComponent(component.ToArray());
                mesh.Optimize();
                list.Add(mesh);
            }
            return list.ToArray();
        }

        private static VertexData BuildComponent(Triangle[] Triangles)
        {
            var vertices = new List<Vector3>();
            var colors = new List<Vector4>();
            var normals = new List<Vector3>();
            var indices = new List<uint>();
            for (var k = 0; k < Triangles.Length; ++k)
            {
                vertices.Add(Triangles[k].P1.Position);
                vertices.Add(Triangles[k].P2.Position);
                vertices.Add(Triangles[k].P3.Position);
                    
                normals.Add(Triangles[k].P1.Normal);
                normals.Add(Triangles[k].P2.Normal);
                normals.Add(Triangles[k].P3.Normal);
                    
                colors.Add(Triangles[k].P1.Color);
                colors.Add(Triangles[k].P2.Color);
                colors.Add(Triangles[k].P3.Color);
                    
                indices.Add((uint)indices.Count);
                indices.Add((uint)indices.Count);
                indices.Add((uint)indices.Count);
            }
            return new VertexData
            {
                Vertices = vertices,
                Normals = normals,
                Colors = colors,
                Indices = indices
            };
        }

        private static Dictionary<Vector3, HashSet<Triangle>> IndexVertices(VertexData Mesh)
        {
            var map = new Dictionary<Vector3, HashSet<Triangle>>();
            Vertex MakeVertex(int Index)
            {
                return new Vertex
                {
                    Position = Mesh.Vertices[Index],
                    Color = Mesh.Colors[Index],
                    Normal = Mesh.Normals[Index]
                };
            }
            for (var i = 0; i < Mesh.Indices.Count; i+=3)
            {
                var p1 = MakeVertex((int)Mesh.Indices[i]);
                var p2 = MakeVertex((int)Mesh.Indices[i+1]);
                var p3 = MakeVertex((int)Mesh.Indices[i+2]);
                var triangle = new Triangle
                {
                    P1 = p1,
                    P2 = p2,
                    P3 = p3
                };
                if(!map.ContainsKey(p1.Position))
                    map.Add(p1.Position, new HashSet<Triangle>());
                if(!map.ContainsKey(p2.Position))
                    map.Add(p2.Position, new HashSet<Triangle>());
                if(!map.ContainsKey(p3.Position))
                    map.Add(p3.Position, new HashSet<Triangle>());

                map[p1.Position].Add(triangle);
                map[p2.Position].Add(triangle);
                map[p3.Position].Add(triangle);
            }
            return map;
        }
        
        private struct Triangle
        {
            public Vertex P1;
            public Vertex P2;
            public Vertex P3;

            public Vertex[] GetConnected(Vector3 P)
            {
                if (P1.Position.Equals(P))
                    return new[] {P2, P3};
                if (P2.Position.Equals(P))
                    return new[] {P1, P3};
                if (P3.Position.Equals(P))
                    return new[] {P2, P3};
                return null;
            }
        }
        
        private struct Vertex
        {
            public Vector3 Position;
            public Vector4 Color;
            public Vector3 Normal;

            public override int GetHashCode()
            {
                return (Position.GetHashCode() * 17 + Color.GetHashCode()) ^ Normal.GetHashCode();
            }
        }
    }
}