using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using BulletSharp;
using Hedra.Engine.Bullet;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Rendering;

namespace Hedra.Engine.Scenes
{
    public static class WaypointLoader
    {
        public static WaypointGraph Load(string Path, Vector3 Scale, Matrix4x4 Transformation)
        {
            using var timing = GenerationMetrics.Measure("waypoint_load");
            var model = AssetManager.PLYLoader(Path, Scale, Vector3.Zero, Vector3.Zero, false);
            model.Colors = Enumerable.Repeat(Vector4.One, model.Vertices.Count).ToList();
            //var groups = model.Ungroup();
            //if (groups.Length != 1)
            //    throw new ArgumentOutOfRangeException("Pathfinding meshes need to be a convex graph");
            var graph = new WaypointGraph();
            if (BakedGenerationAssets.TryGetWaypoints(Path, model, out var positions))
            {
                var builder = new WaypointGraphBuilder(graph, 3.5f);
                foreach (var position in positions) builder.Add(Vector3.Transform(position, Transformation));
            }
            else Sample(model, graph, 3.5f, Transformation);
            if (Environment.GetEnvironmentVariable("HEDRA_VERIFY_WAYPOINTS") == "1")
            {
                var reference = new WaypointGraph();
                Sample(model, reference, 3.5f, Transformation, true);
                if (!graph.Vertices.SequenceEqual(reference.Vertices) ||
                    graph.Vertices.Any(v => !graph.Adjacent(v).SequenceEqual(reference.Adjacent(v))))
                    throw new InvalidOperationException($"Waypoint graph differs from reference: {Path}");
                Log.WriteLine($"GEN_NAV_VERIFIED path={Path} vertices={graph.Vertices.Length}");
            }
            return graph;
        }

        public static Vector3[] SamplePositions(VertexData Model)
        {
            var positions = new List<Vector3>();
            Sample(Model, null, 3.5f, Matrix4x4.Identity, false, positions.Add);
            return positions.ToArray();
        }

        private static void Sample(VertexData Model, WaypointGraph Graph, float Step, Matrix4x4 Transformation,
            bool Reference = false, Action<Vector3> Sink = null)
        {
            if (Step >= 4.0f) throw new ArgumentOutOfRangeException();
            //Debug.Log($"Sampling waypoints");
            var world = new BulletWorld(10);
            var shape = BulletPhysics.CreateTriangleShape(Model.Indices, Model.Vertices);
            var mainBody = default(RigidBody);
            using (var bodyInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), shape))
            {
                mainBody = new RigidBody(bodyInfo);
                world.DynamicsWorld.AddRigidBody(mainBody, CollisionFilterGroups.StaticFilter,
                    CollisionFilterGroups.AllFilter);
            }

            var builder = Reference || Sink != null ? null : new WaypointGraphBuilder(Graph, Step);
            var dummy = BulletSharp.Math.Vector3.Zero;
            var callback = new ClosestRayResultCallback(ref dummy, ref dummy);
            var maximum = new Vector3(Model.SupportPoint(Vector3.UnitX).X,
                Model.SupportPoint(Vector3.UnitY).Y, Model.SupportPoint(Vector3.UnitZ).Z);
            var minimum = new Vector3(Model.SupportPoint(-Vector3.UnitX).X,
                Model.SupportPoint(-Vector3.UnitY).Y, Model.SupportPoint(-Vector3.UnitZ).Z);
            var boundX = (maximum.X - minimum.X) * 2;
            var boundY = (maximum.Y - minimum.Y) * 2;
            var boundZ = (maximum.Z - minimum.Z) * 2;

            for (var y = 0f; y < boundY; y += Step)
            {
                var offsetY = y - boundY / 2f;
                // Preserve the sample grid; only omit rays wholly outside the mesh bounds.
                if (!Reference && (offsetY + 2 < minimum.Y - 0.1f || offsetY - 2 > maximum.Y + 0.1f)) continue;
                for (var x = 0f; x < boundX; x += Step)
                {
                    var offsetX = x - boundX / 2f;
                    if (!Reference && (offsetX < minimum.X - 0.1f || offsetX > maximum.X + 0.1f)) continue;
                    for (var z = 0f; z < boundZ; z += Step)
                    {
                        var offsetZ = z - boundZ / 2f;
                        if (!Reference && (offsetZ < minimum.Z - 0.1f || offsetZ > maximum.Z + 0.1f)) continue;
                        var offset = new BulletSharp.Math.Vector3(offsetX, offsetY, offsetZ);
                        var from = BulletSharp.Math.Vector3.UnitY * -2 + offset;
                        var to = BulletSharp.Math.Vector3.UnitY * 2 + offset;
                        BulletPhysics.ResetCallback(callback);
                        callback.RayFromWorld = from;
                        callback.RayToWorld = to;
                        world.DynamicsWorld.RayTestRef(ref from, ref to, callback);
                        if (callback.HasHit)
                        {
                            if (Sink != null)
                            {
                                Sink(callback.HitPointWorld.Compatible());
                                continue;
                            }
                            var currentPosition =
                                Vector3.Transform(callback.HitPointWorld.Compatible(), Transformation);
                            if (builder != null) builder.Add(currentPosition);
                            else
                            {
                                var neighbors = Graph.Vertices.Where(v =>
                                    Math.Abs(v.Position.X - currentPosition.X) <= Step &&
                                    Math.Abs(v.Position.Y - currentPosition.Y) <= Step &&
                                    Math.Abs(v.Position.Z - currentPosition.Z) <= Step).ToArray();
                                var waypoint = new Waypoint { Position = currentPosition };
                                Graph.AddVertex(waypoint);
                                foreach (var neighbor in neighbors) Graph.AddEdge(waypoint, neighbor);
                            }
                        }
                    }
                }
            }
            
            callback.Dispose();
            world.DynamicsWorld.RemoveRigidBody(mainBody);
            BulletPhysics.DisposeBody(mainBody);
            world.Dispose();
        }
    }
}
