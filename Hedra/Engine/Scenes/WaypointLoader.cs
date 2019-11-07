using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Geometry;

namespace Hedra.Engine.Scenes
{
    public static class WaypointLoader
    {
        public static WaypointGraph Load(string Path, Vector3 Scale, Matrix4x4 Transformation)
        {
            var model = AssetManager.PLYLoader(Path, Scale, Vector3.Zero, Vector3.Zero, false);
            model.Colors = Enumerable.Repeat(Vector4.One, model.Vertices.Count).ToList();
            var groups = model.Ungroup();
            if(groups.Length != 1) throw new ArgumentOutOfRangeException("Pathfinding meshes need to be a convex graph");
            var indexed = MeshAnalyzer.IndexVertices(model.Indices, model.Vertices, model.Colors, model.Normals);
            var graph = new WaypointGraph();
            var visited = new HashSet<MeshAnalyzer.Triangle>();
            var queue = new Queue<MeshAnalyzer.Triangle>();
            queue.Enqueue(indexed.Values.First().First());
            while (queue.Count > 0)
            {
                var w = queue.Dequeue();
                if(visited.Contains(w)) continue;
                visited.Add(w);
                var from = FromTriangle(w, Transformation);
                graph.AddVertex(from);
                for (var i = 0; i < 3; ++i)
                {
                    var adjacents = indexed[w[i].Position];
                    for (var j = 0; j < adjacents.Length; ++j)
                    {
                        var x = adjacents[j];
                        if(visited.Contains(x)) continue;
                        var to = FromTriangle(x, Transformation);
                        graph.AddEdge(from, to);
                        queue.Enqueue(x);
                    }
                }
            }
            return graph;
        }

        private static Waypoint FromTriangle(MeshAnalyzer.Triangle Triangle, Matrix4x4 Transformation)
        {
            return new Waypoint
            {
                Position = Vector3.Transform((Triangle.P1.Position + Triangle.P2.Position + Triangle.P3.Position) / 3f, Transformation),
                Size = 4
            };
        }
    }
}