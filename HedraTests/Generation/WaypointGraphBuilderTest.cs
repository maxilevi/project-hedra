using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Scenes;
using NUnit.Framework;

namespace HedraTests.Generation
{
    [TestFixture]
    public class WaypointGraphBuilderTest
    {
        [TestCase(0)]
        [TestCase(12345)]
        public void SpatialLookupPreservesVerticesAdjacencyOrderAndPaths(int Seed)
        {
            const float step = 3.5f;
            var expected = new WaypointGraph();
            var actual = new WaypointGraph();
            var builder = new WaypointGraphBuilder(actual, step);
            var rng = new Random(Seed);
            var rotation = Matrix4x4.CreateRotationY(0.83f) * Matrix4x4.CreateTranslation(-5000, 37, 5000);
            // Include duplicates, negative coordinates, rotated points, and inclusive boundaries.
            var positions = new[] { Vector3.Zero, Vector3.One * step, -Vector3.One * step,
                Vector3.Zero, new Vector3(7, 0, 0), new Vector3(3.5f, 0, 0),
                new Vector3(-0.0000001f, 0, 0), new Vector3(3.5f, 0, 0) }
                .Concat(Enumerable.Range(0, 600).Select(_ => Vector3.Transform(new Vector3(
                    rng.Next(-10, 11) * step, rng.Next(-2, 3) * step, rng.Next(-10, 11) * step), rotation)));
            foreach (var position in positions)
            {
                var neighbors = expected.Vertices.Where(v =>
                    Math.Abs(v.Position.X - position.X) <= step &&
                    Math.Abs(v.Position.Y - position.Y) <= step &&
                    Math.Abs(v.Position.Z - position.Z) <= step).ToArray();
                var waypoint = new Waypoint { Position = position };
                expected.AddVertex(waypoint);
                foreach (var neighbor in neighbors) expected.AddEdge(waypoint, neighbor);
                builder.Add(position);
            }

            CollectionAssert.AreEqual(expected.Vertices, actual.Vertices);
            foreach (var vertex in expected.Vertices)
                CollectionAssert.AreEqual(expected.Adjacent(vertex), actual.Adjacent(vertex));
            var vertices = expected.Vertices;
            for (var i = 0; i < vertices.Length; i += 23)
            {
                var source = vertices[i];
                var target = vertices[vertices.Length - i - 1];
                var expectedPath = expected.GetShortestPath(source, target, out var expectedReach);
                var actualPath = actual.GetShortestPath(source, target, out var actualReach);
                Assert.AreEqual(expectedReach, actualReach);
                CollectionAssert.AreEqual(expectedPath, actualPath);
            }
        }
    }
}
