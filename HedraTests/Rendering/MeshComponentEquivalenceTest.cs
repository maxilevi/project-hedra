using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using Hedra.Rendering;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    public class MeshComponentEquivalenceTest
    {
        [TestCase(7)]
        [TestCase(43)]
        public void ComponentsPreserveTriangleOrderAndVertexAttributes(int Seed)
        {
            var rng = new Random(Seed);
            var mesh = new VertexData();
            for (var group = 0; group < 4; group++)
            {
                var points = Enumerable.Range(0, 50).Select(i =>
                    new Vector3(rng.Next(0, 12), rng.Next(0, 12), rng.Next(0, 12)) +
                    new Vector3(group * 100, 0, -5000)).ToArray();
                for (var triangle = 0; triangle < 250; triangle++)
                {
                    for (var p = 0; p < 3; p++)
                    {
                        mesh.Indices.Add((uint)mesh.Vertices.Count);
                        mesh.Vertices.Add(points[rng.Next(points.Length)]);
                        mesh.Normals.Add(triangle % 2 == 0 ? Vector3.UnitY : Vector3.UnitX);
                        mesh.Colors.Add(triangle % 3 == 0 ? Vector4.One : Vector4.UnitX);
                    }
                    // Exact duplicates must be removed; seams at equal positions stay distinct.
                    if (triangle % 9 == 0)
                        mesh.Indices.AddRange(mesh.Indices.Skip(mesh.Indices.Count - 3).Take(3).ToArray());
                }
            }

            var expected = Reference(mesh);
            var actual = MeshAnalyzer.GetConnectedComponents(mesh);
            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                CollectionAssert.AreEqual(expected[i].Vertices, actual[i].Vertices);
                CollectionAssert.AreEqual(expected[i].Colors, actual[i].Colors);
                CollectionAssert.AreEqual(expected[i].Normals, actual[i].Normals);
                CollectionAssert.AreEqual(expected[i].Indices, actual[i].Indices);
            }
        }

        private static VertexData[] Reference(VertexData Mesh)
        {
            var map = MeshAnalyzer.IndexVertices(Mesh.Indices, Mesh.Vertices, Mesh.Colors, Mesh.Normals);
            var visited = new HashSet<Vector3>();
            var output = new List<VertexData>();
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 16, Allocator.Megabyte * 2);
            foreach (var start in map.Keys)
            {
                if (visited.Contains(start)) continue;
                var triangles = new HashSet<MeshAnalyzer.Triangle>();
                var queue = new Queue<Vector3>();
                queue.Enqueue(start);
                while (queue.Count != 0)
                {
                    var vertex = queue.Dequeue();
                    if (!visited.Add(vertex)) continue;
                    foreach (var triangle in map[vertex])
                    {
                        queue.Enqueue(triangle.P1.Position);
                        queue.Enqueue(triangle.P2.Position);
                        queue.Enqueue(triangle.P3.Position);
                        triangles.Add(triangle);
                    }
                }
                var component = new VertexData();
                foreach (var triangle in triangles)
                for (var p = 0; p < 3; p++)
                {
                    var vertex = triangle[p];
                    component.Indices.Add((uint)component.Vertices.Count);
                    component.Vertices.Add(vertex.Position);
                    component.Colors.Add(vertex.Color);
                    component.Normals.Add(vertex.Normal);
                }
                component.Optimize(allocator);
                output.Add(component);
            }
            return output.ToArray();
        }
    }
}
