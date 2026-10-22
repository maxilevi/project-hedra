using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using Hedra.Rendering;

namespace Hedra.Engine.Generation
{
    // Store the original BFS triangle order, not an approximation or a simplified mesh.
    // Colors and transforms are applied to the full model before assembling these parts.
    public sealed class BakedMeshTopology
    {
        public byte[] Fingerprint { get; }
        public int[][] Parts { get; }
        private int[] _vertexParts;
        private int[][] _sourceTriangles;
        private int[] _positionVertices;
        private int[][] _attributeVertices;

        public BakedMeshTopology(byte[] Fingerprint, int[][] Parts)
        {
            this.Fingerprint = Fingerprint;
            this.Parts = Parts;
        }

        public static byte[] Hash(VertexData Model)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
            {
                writer.Write(Model.Vertices.Count);
                writer.Write(Model.Indices.Count);
                foreach (var v in Model.Vertices) { writer.Write(v.X); writer.Write(v.Y); writer.Write(v.Z); }
                foreach (var v in Model.Normals) { writer.Write(v.X); writer.Write(v.Y); writer.Write(v.Z); }
                foreach (var c in Model.Colors) { writer.Write(c.X); writer.Write(c.Y); writer.Write(c.Z); writer.Write(c.W); }
                foreach (var i in Model.Indices) writer.Write(i);
            }
            return SHA256.HashData(stream.GetBuffer().AsSpan(0, (int)stream.Length));
        }

        public static BakedMeshTopology Compile(VertexData Source)
        {
            var vertices = new Dictionary<(long, long, long, long, long), int>();
            for (var i = 0; i < Source.Vertices.Count; i++)
                vertices.TryAdd(VertexBits(VertexAt(Source, i)), i);
            var parts = MeshAnalyzer.GetConnectedComponents(Source, false);
            var topology = new BakedMeshTopology(Hash(Source), parts.Select(part => part.Indices.Select(index =>
                vertices[VertexBits(VertexAt(part, (int)index))]).ToArray()).ToArray());
            topology.Prepare(Source);
            return topology;
        }

        public bool Matches(VertexData Source)
        {
            if (!Fingerprint.SequenceEqual(Hash(Source)) ||
                !Parts.All(part => part.Length % 3 == 0 && part.All(index => index >= 0 && index < Source.Vertices.Count))) return false;
            Prepare(Source);
            return true;
        }

        private void Prepare(VertexData Source)
        {
            var positions = new Dictionary<Vector3, int>();
            for (var p = 0; p < Parts.Length; p++)
                foreach (var index in Parts[p]) positions[Source.Vertices[index]] = p;
            _vertexParts = new int[Source.Vertices.Count];
            for (var i = 0; i < _vertexParts.Length; i++)
                _vertexParts[i] = positions.TryGetValue(Source.Vertices[i], out var part) ? part : -1;
            var triangles = Enumerable.Range(0, Parts.Length).Select(_ => new List<int>()).ToArray();
            for (var i = 0; i < Source.Indices.Count; i += 3)
                triangles[_vertexParts[(int)Source.Indices[i]]].Add(i);
            _sourceTriangles = triangles.Select(list => list.ToArray()).ToArray();
            var positionVertices = new Dictionary<Vector3, int>();
            var attributeVertices = Enumerable.Range(0, Parts.Length).Select(_ => new List<int>()).ToArray();
            var attributes = new HashSet<MeshAnalyzer.Vertex>();
            for (var i = 0; i < Source.Vertices.Count; i++)
            {
                var part = _vertexParts[i];
                if (part < 0) continue;
                positionVertices.TryAdd(Source.Vertices[i], i);
                if (attributes.Add(VertexAt(Source, i))) attributeVertices[part].Add(i);
            }
            _positionVertices = positionVertices.Values.ToArray();
            _attributeVertices = attributeVertices.Select(list => list.ToArray()).ToArray();
        }

        public bool TryAssemble(VertexData Source, VertexData Transformed, out VertexData[] Result)
        {
            using var timing = GenerationMetrics.Measure("baked_mesh_assemble");
            Result = null;
            if (Source.Vertices.Count != Transformed.Vertices.Count) return false;
            if (_vertexParts == null) throw new InvalidOperationException("Baked topology must match its source before use");
            var parents = Enumerable.Range(0, Parts.Length).ToArray();
            var affected = new bool[Parts.Length];
            int Root(int Part)
            {
                while (parents[Part] != Part)
                {
                    parents[Part] = parents[parents[Part]];
                    Part = parents[Part];
                }
                return Part;
            }
            // Repair only the connected pieces changed by float rounding or recoloring.
            var positions = new Dictionary<Vector3, int>(_positionVertices.Length);
            var attributes = new Dictionary<MeshAnalyzer.Vertex, int>();
            using (GenerationMetrics.Measure("baked_mesh_guard"))
            {
                foreach (var i in _positionVertices)
                {
                    var part = _vertexParts[i];
                    var original = Source.Vertices[i];
                    var transformed = Transformed.Vertices[i];
                    if (positions.TryGetValue(transformed, out var previous) && Source.Vertices[previous] != original)
                    {
                        var left = Root(part);
                        var right = Root(_vertexParts[previous]);
                        parents[Math.Max(left, right)] = Math.Min(left, right);
                        affected[part] = affected[_vertexParts[previous]] = true;
                    }
                    else positions[transformed] = i;
                }
                for (var part = 0; part < Parts.Length; part++)
                {
                    attributes.Clear();
                    foreach (var i in _attributeVertices[part])
                    {
                        var vertex = VertexAt(Transformed, i);
                        var originalVertex = VertexAt(Source, i);
                        if (attributes.TryGetValue(vertex, out var previousVertex) && !VertexAt(Source, previousVertex).Equals(originalVertex))
                            affected[part] = affected[_vertexParts[previousVertex]] = true;
                        else attributes[vertex] = i;
                    }
                }
            }

            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 16, Allocator.Megabyte * 2);
            var repairs = new Dictionary<int, List<int>>();
            for (var p = 0; p < Parts.Length; p++)
            {
                if (!affected[p]) continue;
                var root = Root(p);
                if (!repairs.ContainsKey(root)) repairs.Add(root, new List<int>());
            }
            for (var p = 0; p < Parts.Length; p++)
                if (repairs.TryGetValue(Root(p), out var triangles)) triangles.AddRange(_sourceTriangles[p]);
            var output = new List<VertexData>(Parts.Length);
            using var assemblyTiming = GenerationMetrics.Measure("baked_mesh_parts");
            for (var p = 0; p < Parts.Length; p++)
            {
                if (Root(p) != p) continue;
                if (repairs.TryGetValue(p, out var triangles))
                {
                    // Restore source triangle order before replaying BFS for a merged piece.
                    triangles.Sort();
                    var repair = new VertexData();
                    foreach (var triangle in triangles)
                    for (var v = 0; v < 3; v++)
                        AppendVertex(repair, Transformed, (int)Source.Indices[triangle + v]);
                    output.AddRange(MeshAnalyzer.GetConnectedComponents(repair, true, allocator));
                    continue;
                }
                var indices = Parts[p];
                var part = new VertexData
                {
                    Vertices = new List<Vector3>(indices.Length), Normals = new List<Vector3>(indices.Length),
                    Colors = new List<Vector4>(indices.Length), Indices = new List<uint>(indices.Length)
                };
                foreach (var index in indices)
                    AppendVertex(part, Transformed, index);
                part.Optimize(allocator);
                output.Add(part);
            }
            Result = output.ToArray();
            return true;
        }

        private static void AppendVertex(VertexData Part, VertexData Model, int Index)
        {
            Part.Indices.Add((uint)Part.Vertices.Count);
            Part.Vertices.Add(Model.Vertices[Index]);
            Part.Normals.Add(Model.Normals[Index]);
            Part.Colors.Add(Model.Colors[Index]);
        }

        private static MeshAnalyzer.Vertex VertexAt(VertexData Model, int Index) =>
            new(Model.Vertices[Index], Model.Colors[Index], Model.Normals[Index]);

        // Native mesh remapping compares bytes, including the sign bit of zero.
        private static (long, long, long, long, long) VertexBits(MeshAnalyzer.Vertex Vertex)
        {
            long Pair(float Left, float Right) =>
                ((long)(uint)BitConverter.SingleToInt32Bits(Left) << 32) | (uint)BitConverter.SingleToInt32Bits(Right);
            return (Pair(Vertex.Position.X, Vertex.Position.Y), Pair(Vertex.Position.Z, Vertex.Color.X),
                Pair(Vertex.Color.Y, Vertex.Color.Z), Pair(Vertex.Color.W, Vertex.Normal.X), Pair(Vertex.Normal.Y, Vertex.Normal.Z));
        }
    }
}
