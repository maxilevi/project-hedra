using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hedra.Engine.Generation;
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
            IList<Vector3> Normals, HashSet<uint> IgnoreList, int Steps = 1, bool SmoothNormals = true)
        {
            ApplySmoothing(new ManagedSmoothingBuffer<uint>(Indices), new ManagedSmoothingBuffer<Vector3>(Vertices),
                new ManagedSmoothingBuffer<Vector3>(Normals), IgnoreList, Steps, SmoothNormals);
        }

        public static void ApplySmoothing(NativeVertexData Mesh, HashSet<uint> IgnoreList, int Steps = 1,
            bool SmoothNormals = true)
        {
            // The core only allocates managed scratch arrays. Resolve all native
            // pointers once; no arena allocation/growth can invalidate them here.
            ApplySmoothing(new NativeSmoothingBuffer<uint>(Mesh.Indices), new NativeSmoothingBuffer<Vector3>(Mesh.Vertices),
                new NativeSmoothingBuffer<Vector3>(Mesh.Normals), IgnoreList, Steps, SmoothNormals);
        }

        private interface ISmoothingBuffer<T> where T : unmanaged
        {
            int Count { get; }
            T this[int Index] { get; set; }
        }

        private readonly struct ManagedSmoothingBuffer<T> : ISmoothingBuffer<T> where T : unmanaged
        {
            private readonly IList<T> _list;
            public ManagedSmoothingBuffer(IList<T> List) { _list = List; }
            public int Count => _list.Count;
            public T this[int Index] { get => _list[Index]; set => _list[Index] = value; }
        }

        private readonly unsafe struct NativeSmoothingBuffer<T> : ISmoothingBuffer<T> where T : unmanaged
        {
            private readonly T* _pointer;
            public NativeSmoothingBuffer(NativeList<T> List) { _pointer = (T*)List.Pointer; Count = List.Count; }
            public int Count { get; }
            public T this[int Index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _pointer[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _pointer[Index] = value;
            }
        }

        private static void ApplySmoothing<TIndices, TVertices, TNormals>(TIndices Indices, TVertices Vertices,
            TNormals Normals, HashSet<uint> IgnoreList, int Steps, bool SmoothNormals)
            where TIndices : struct, ISmoothingBuffer<uint>
            where TVertices : struct, ISmoothingBuffer<Vector3>
            where TNormals : struct, ISmoothingBuffer<Vector3>
        {
            var vertexCount = Vertices.Count;
            var triangles = new List<IndexedTriangle>(Indices.Count / 3);
            var unique = new HashSet<IndexedTriangle>();
            var counts = new int[vertexCount];
            for (var i = 0; i < Indices.Count; i += 3)
            {
                var triangle = new IndexedTriangle { P1 = Indices[i], P2 = Indices[i + 1], P3 = Indices[i + 2] };
                if (!unique.Add(triangle)) continue;
                triangles.Add(triangle);
                counts[triangle.P1] += 2;
                if (triangle.P2 != triangle.P1) counts[triangle.P2] += 2;
                if (triangle.P3 != triangle.P1 && triangle.P3 != triangle.P2) counts[triangle.P3] += 2;
            }
            var offsets = new int[vertexCount + 1];
            for (var i = 0; i < vertexCount; i++) offsets[i + 1] = checked(offsets[i] + counts[i]);
            var neighbours = new uint[offsets[vertexCount]];
            var write = (int[])offsets.Clone();
            void Add(uint Vertex, uint First, uint Second)
            {
                neighbours[write[Vertex]++] = First;
                neighbours[write[Vertex]++] = Second;
            }
            foreach (var triangle in triangles)
            {
                Add(triangle.P1, triangle.P2, triangle.P3);
                if (triangle.P2 != triangle.P1) Add(triangle.P2, triangle.P1, triangle.P3);
                // Preserve GetConnected's existing third-corner weighting, including self.
                if (triangle.P3 != triangle.P1 && triangle.P3 != triangle.P2) Add(triangle.P3, triangle.P2, triangle.P3);
            }
            var remaps = new VertexRemap[vertexCount];
            var groups = new int[vertexCount];
            var positions = new Dictionary<Vector3, int>(vertexCount);
            for (var w = 0; w < Steps; w++)
            {
                positions.Clear();
                Array.Clear(remaps, 0, remaps.Length);
                var groupCount = 0;
                for (var i = 0; i < vertexCount; ++i)
                {
                    var vertex = Vertices[i];
                    ref var group = ref CollectionsMarshal.GetValueRefOrAddDefault(positions, vertex, out var exists);
                    if (!exists) group = groupCount++;
                    groups[i] = group;
                    ref var remap = ref remaps[group];
                    for (var j = offsets[i]; j < offsets[i + 1]; j++)
                    {
                        var neighbour = (int)neighbours[j];
                        remap.Position += Vertices[neighbour];
                        if (SmoothNormals) remap.Normal += Normals[neighbour];
                        remap.Count++;
                    }
                }

                for (var i = 0; i < vertexCount; ++i)
                {
                    var remap = remaps[groups[i]];
                    if (SmoothNormals) Normals[i] = remap.Normal / remap.Count;
                    if (IgnoreList?.Contains((uint)i) ?? false) continue;
                    Vertices[i] = remap.Position / remap.Count;
                }
            }
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

        public static VertexData[] GetConnectedComponents(VertexData Mesh, bool Optimize = true, IAllocator Scratch = null)
        {
            using var timing = GenerationMetrics.Measure("mesh_components");
            var list = new List<VertexData>();
            if (Mesh.Vertices.Count == 0) return list.ToArray();
            var positions = new Dictionary<Vector3, int>();
            var adjacency = new List<List<int>>();
            var triangles = new List<Triangle>(Mesh.Indices.Count / 3);
            var triangleVertices = new List<IndexedTriangle>(Mesh.Indices.Count / 3);
            var uniqueTriangles = new HashSet<Triangle>();
            int Index(Vector3 Position)
            {
                if (positions.TryGetValue(Position, out var index)) return index;
                index = positions.Count;
                positions.Add(Position, index);
                adjacency.Add(new List<int>());
                return index;
            }

            using (GenerationMetrics.Measure("mesh_components_index"))
            for (var i = 0; i < Mesh.Indices.Count; i += 3)
            {
                Vertex VertexAt(uint Id) => new(Mesh.Vertices[(int)Id], Mesh.Colors[(int)Id], Mesh.Normals[(int)Id]);
                var triangle = new Triangle
                {
                    P1 = VertexAt(Mesh.Indices[i]),
                    P2 = VertexAt(Mesh.Indices[i + 1]),
                    P3 = VertexAt(Mesh.Indices[i + 2])
                };
                var p1 = Index(triangle.P1.Position);
                var p2 = Index(triangle.P2.Position);
                var p3 = Index(triangle.P3.Position);
                if (!uniqueTriangles.Add(triangle)) continue;
                var id = triangles.Count;
                triangles.Add(triangle);
                triangleVertices.Add(new IndexedTriangle { P1 = (uint)p1, P2 = (uint)p2, P3 = (uint)p3 });
                adjacency[p1].Add(id);
                if (p2 != p1) adjacency[p2].Add(id);
                if (p3 != p1 && p3 != p2) adjacency[p3].Add(id);
            }
            var visited = new bool[positions.Count];
            var visitedTriangles = new bool[triangles.Count];
            var queue = new Queue<int>();
            var component = new List<Triangle>();
            void BreadthFirstSearch(int Start)
            {
                queue.Enqueue(Start);
                while (queue.Count != 0)
                {
                    var vertex = queue.Dequeue();
                    if (visited[vertex]) continue;
                    visited[vertex] = true;
                    foreach (var id in adjacency[vertex])
                    {
                        if (visitedTriangles[id]) continue;
                        visitedTriangles[id] = true;
                        var points = triangleVertices[id];
                        queue.Enqueue((int)points.P1);
                        queue.Enqueue((int)points.P2);
                        queue.Enqueue((int)points.P3);
                        component.Add(triangles[id]);
                    }
                }
            }

            using (var ownedAllocator = Scratch == null ? new ResizableHeapAllocator(Allocator.Megabyte * 16, Allocator.Megabyte * 2) : null)
            {
                var allocator = Scratch ?? ownedAllocator;
                for (var i = 0; i < visited.Length; ++i)
                {
                    if (visited[i]) continue;
                    component.Clear();
                    using (GenerationMetrics.Measure("mesh_components_traverse")) BreadthFirstSearch(i);
                    VertexData mesh;
                    using (GenerationMetrics.Measure("mesh_components_build")) mesh = BuildComponent(component.ToArray(), Mesh);
                    if (Optimize)
                        using (GenerationMetrics.Measure("mesh_components_optimize")) mesh.Optimize(allocator);
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

        private struct IndexedTriangle : IEquatable<IndexedTriangle>
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
                return obj is IndexedTriangle t && Equals(t);
            }
            public bool Equals(IndexedTriangle Other) => P1 == Other.P1 && P2 == Other.P2 && P3 == Other.P3;
            public override int GetHashCode() => HashCode.Combine(P1, P2, P3);
        }

        public struct Triangle : IEquatable<Triangle>
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
                return obj is Triangle t && Equals(t);
            }
            public bool Equals(Triangle Other) => P1.Equals(Other.P1) && P2.Equals(Other.P2) && P3.Equals(Other.P3);
            public override int GetHashCode() => HashCode.Combine(P1, P2, P3);
        }

        public struct Vertex : IEquatable<Vertex>
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

            public override int GetHashCode()
            {
                return HashCode.Combine(Position, Color, Normal);
            }
            
            public override bool Equals(object? obj)
            {
                return obj is Vertex t && Equals(t);
            }
            public bool Equals(Vertex Other) => Position.Equals(Other.Position) && Color.Equals(Other.Color) && Normal.Equals(Other.Normal);
        }
    }
}
