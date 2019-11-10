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
            var triangles = indexed.Values.SelectMany(T => T).ToArray();
            for (var i = 0; i < triangles.Length; ++i)
            {
                var tri = triangles[i];
                var midpoint = FromTriangle(tri, Transformation);
                for (var j = 0; j < 3; ++j)
                {
                    var connected = tri.GetConnected(tri[j].Position);
                    var vertex = FromVertex(tri[j], Transformation);
                    graph.AddEdge(midpoint, vertex);
                    for (var k = 0; k < connected.Length; ++k)
                    {
                        graph.AddEdge(vertex, FromVertex(connected[k], Transformation));
                    }
                }
            }
            return graph;
        }

        private static Waypoint FromVertex(MeshAnalyzer.Vertex Vertex, Matrix4x4 Transformation)
        {
            return new Waypoint
            {
                Position = Vector3.Transform(Vertex.Position, Transformation),
                Size = 4
            };
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