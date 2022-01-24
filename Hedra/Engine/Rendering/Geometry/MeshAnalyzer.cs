using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Framework;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering.Geometry
{
    public static class MeshAnalyzer
    {
        /*
         * Reference: https://docs.blender.org/manual/en/latest/modeling/modifiers/deform/smooth.html
         */
        public static void ApplySmoothing(IList<uint> Indices, IList<Vector3> Vertices, IList<Vector4> Colors,
            IList<Vector3> Normals, HashSet<uint> IgnoreList, int Steps = 1)
        {
            var map = IndexUniqueVertices(Indices, Vertices, Colors, Normals);
            for (var w = 0; w < Steps; w++)
            {
                var vertexRemap = new Dictionary<Vector3, VertexRemap>();

                void AddWeight(Vector3 Vertex, int Neighbour)
                {
                    var remap = vertexRemap[Vertex];
                    vertexRemap[Vertex] = new VertexRemap
                    {
                        Position = remap.Position + Vertices[Neighbour],
                        Normal = remap.Normal + Normals[Neighbour],
                        Count = remap.Count + 1
                    };
                }

                for (var i = 0; i < Vertices.Count; ++i)
                {
                    var vertex = Vertices[i];
                    var triangles = map[i].ToArray();
                    if (!vertexRemap.ContainsKey(vertex))
                        vertexRemap[vertex] = new VertexRemap();

                    for (var j = 0; j < triangles.Length; ++j)
                    {
                        var neighbours = triangles[j].GetConnected((uint)i);
                        for (var k = 0; k < neighbours.Length; ++k) AddWeight(vertex, (int)neighbours[k]);
                    }
                }

                for (var i = 0; i < Vertices.Count; ++i)
                {
                    var remap = vertexRemap[Vertices[i]];
                    Normals[i] = remap.Normal / remap.Count;
                    if (IgnoreList?.Contains((uint)i) ?? false) continue;
                    Vertices[i] = remap.Position / remap.Count;
                }
            }
        }

        private static IndexedTriangle[][] IndexUniqueVertices(IList<uint> Indices, IList<Vector3> Vertices,
            IList<Vector4> Colors, IList<Vector3> Normals)
        {
            var map = new HashSet<IndexedTriangle>[Vertices.Count];
            for (var i = 0; i < Indices.Count; i += 3)
            {
                var triangle = new IndexedTriangle
                {
                    P1 = Indices[i],
                    P2 = Indices[i + 1],
                    P3 = Indices[i + 2]
                };
                if (map[triangle.P1] == null)
                    map[triangle.P1] = new HashSet<IndexedTriangle>();
                if (map[triangle.P2] == null)
                    map[triangle.P2] = new HashSet<IndexedTriangle>();
                if (map[triangle.P3] == null)
                    map[triangle.P3] = new HashSet<IndexedTriangle>();

                map[triangle.P1].Add(triangle);
                map[triangle.P2].Add(triangle);
                map[triangle.P3].Add(triangle);
            }

            var finalMap = new IndexedTriangle[Vertices.Count][];
            for (var i = 0; i < Vertices.Count; ++i) finalMap[i] = map[i].ToArray();
            return finalMap;
        }

        /* public static VertexData[] GetConnectedComponents2(VertexData Mesh)
         {
             var list = new List<VertexData>();
             var vertices = Mesh.Vertices;
             if (vertices.Count == 0) return list.ToArray();
 
             var graph = new Dictionary<Vector3, List<Vector3>>();
             var indices = Mesh.Indices;
             Vector3 GetVertex(int I)
             {
                 return new Vertex(
                     vertices[(int) indices[I]],
                     vertices[(int) indices[I]],
                     vertices[(int) indices[I]]
                 );
             }
             
             for (var i = 0; i < indices.Count; i += 3)
             {
                 var v0 = GetVertex(i);
                 var v1 = GetVertex(i+1);
                 var v2 = GetVertex(i+2);
 
                 if(!graph.ContainsKey(v0))
                     graph.Add(v0, new List<Vector3>());
                 
                 if(!graph.ContainsKey(v1))
                     graph.Add(v1, new List<Vector3>());
                 
                 if(!graph.ContainsKey(v2))
                     graph.Add(v2, new List<Vector3>());
                 
                 graph[v0].Add(v1);
                 graph[v0].Add(v2);
                 
                 graph[v1].Add(v0);
                 graph[v1].Add(v2);
                 
                 graph[v2].Add(v0);
                 graph[v2].Add(v1);
             }
 
         }*/

        public static VertexData[] GetConnectedComponents(VertexData Mesh)
        {
            var list = new List<VertexData>();
            if (Mesh.Vertices.Count == 0) return list.ToArray();
            var map = IndexVertices(Mesh);
            var remaining = map.Keys.ToArray();
            var visited = new HashSet<Vector3>();

            void BreadthFirstSearch(Vector3 Start, HashSet<Triangle> Component)
            {
                var queue = new Queue<Vector3>();
                queue.Enqueue(Start);
                while (queue.Count > 0)
                {
                    var vertex = queue.Dequeue();
                    if (visited.Contains(vertex)) continue;
                    visited.Add(vertex);
                    var triangles = map[vertex];
                    for (var i = 0; i < triangles.Length; ++i)
                    {
                        queue.Enqueue(triangles[i].P1.Position);
                        queue.Enqueue(triangles[i].P2.Position);
                        queue.Enqueue(triangles[i].P3.Position);
                        Component.Add(triangles[i]);
                    }
                }
            }

            using (var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 16, Allocator.Megabyte * 2))
            {
                for (var i = 0; i < remaining.Length; ++i)
                {
                    if (visited.Contains(remaining[i])) continue;
                    var component = new HashSet<Triangle>();
                    BreadthFirstSearch(remaining[i], component);

                    var mesh = BuildComponent(component.ToArray(), Mesh);
                    mesh.Optimize(allocator);
                    list.Add(mesh);
                }
            }

            return list.ToArray();
        }

        private static VertexData BuildComponent(Triangle[] Triangles, VertexData Mesh)
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

        private static Dictionary<Vector3, Triangle[]> IndexVertices(VertexData Mesh)
        {
            return IndexVertices(Mesh.Indices, Mesh.Vertices, Mesh.Colors, Mesh.Normals);
        }

        public static Dictionary<Vector3, Triangle[]> IndexVertices(IList<uint> Indices, IList<Vector3> Vertices,
            IList<Vector4> Colors, IList<Vector3> Normals)
        {
            var map = new Dictionary<Vector3, HashSet<Triangle>>();
            var finalMap = new Dictionary<Vector3, Triangle[]>();

            Vertex MakeVertex(int Index)
            {
                return new Vertex(Vertices[Index], Colors[Index], Normals[Index]);
            }

            for (var i = 0; i < Indices.Count; i += 3)
            {
                var p1 = MakeVertex((int)Indices[i]);
                var p2 = MakeVertex((int)Indices[i + 1]);
                var p3 = MakeVertex((int)Indices[i + 2]);
                var triangle = new Triangle
                {
                    P1 = p1,
                    P2 = p2,
                    P3 = p3
                };
                if (!map.ContainsKey(p1.Position))
                    map.Add(p1.Position, new HashSet<Triangle>());
                if (!map.ContainsKey(p2.Position))
                    map.Add(p2.Position, new HashSet<Triangle>());
                if (!map.ContainsKey(p3.Position))
                    map.Add(p3.Position, new HashSet<Triangle>());


                map[p1.Position].Add(triangle);
                map[p2.Position].Add(triangle);
                map[p3.Position].Add(triangle);
            }

            foreach (var pair in map) finalMap.Add(pair.Key, pair.Value.ToArray());
            return finalMap;
        }

        private struct VertexRemap
        {
            public Vector3 Position;
            public Vector3 Normal;
            public int Count;
            
            public override bool Equals(object? obj)
            {
                return obj is VertexRemap t && Position.Equals(t.Position) && Normal.Equals(t.Normal) && Count.Equals(t.Count);
            }
        }

        private struct IndexedTriangle
        {
            public uint P1;
            public uint P2;
            public uint P3;

            public uint[] GetConnected(uint P)
            {
                if (P1.Equals(P))
                    return new[] { P2, P3 };
                if (P2.Equals(P))
                    return new[] { P1, P3 };
                if (P3.Equals(P))
                    return new[] { P2, P3 };
                return null;
            }
            
            public override bool Equals(object? obj)
            {
                return obj is IndexedTriangle t && P1.Equals(t.P1) && P2.Equals(t.P2) && P3.Equals(t.P3);
            }
        }

        public struct Triangle
        {
            public Vertex P1;
            public Vertex P2;
            public Vertex P3;

            public Vertex[] GetConnected(Vector3 P)
            {
                if (P1.Position.Equals(P))
                    return new[] { P2, P3 };
                if (P2.Position.Equals(P))
                    return new[] { P1, P3 };
                if (P3.Position.Equals(P))
                    return new[] { P2, P3 };
                return null;
            }

            public Vertex this[int Index] => Index == 0 ? P1 :
                Index == 1 ? P2 :
                Index == 2 ? P3 : throw new ArgumentOutOfRangeException();
                
            public override bool Equals(object? obj)
            {
                return obj is Triangle t && P1.Equals(t.P1) && P2.Equals(t.P2) && P3.Equals(t.P3);
            }
        }

        public struct Vertex
        {
            public readonly Vector3 Position;
            public readonly Vector4 Color;
            public readonly Vector3 Normal;

            public Vertex(Vector3 p, Vector4 c, Vector3 n)
            {
                Position = p;
                Color = c;
                Normal = n;
            }

            private static int Combine(int h1, int h2)
            {
                return ((h1 << 5) + h1) ^ h2;
            }

            public override int GetHashCode()
            {
                return Combine(Combine(Position.GetHashCode(), Color.GetHashCode()), Normal.GetHashCode());
            }
            
            public override bool Equals(object? obj)
            {
                return obj is Vertex t && Position.Equals(t.Position) && Color.Equals(t.Color) && Normal.Equals(t.Normal);
            }
        }
    }
}