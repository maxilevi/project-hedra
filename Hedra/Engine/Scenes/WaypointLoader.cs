using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BulletSharp;
using Hedra.Engine.Bullet;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Rendering;

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
            var graph = new WaypointGraph();
            Sample(model, graph, 3.5f, Transformation);
            return graph;
        }

        private static void Sample(VertexData Model, WaypointGraph Graph, float Step, Matrix4x4 Transformation)
        {
            if(Step >= 4.0f) throw new ArgumentOutOfRangeException();
            var world = new BulletWorld(Gravity: 10);
            var shape = BulletPhysics.CreateTriangleShape(Model.Indices, Model.Vertices);
            var mainBody = default(RigidBody);
            using (var bodyInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), shape))
            {
                mainBody = new RigidBody(bodyInfo);
                world.DynamicsWorld.AddRigidBody(mainBody, CollisionFilterGroups.StaticFilter, CollisionFilterGroups.AllFilter);
            }

            var list = new List<Waypoint>();
            var dummy = BulletSharp.Math.Vector3.Zero;
            var callback = new ClosestRayResultCallback(ref dummy, ref dummy);
            var boundX = (Model.SupportPoint(Vector3.UnitX).X - Model.SupportPoint(-Vector3.UnitX).X) * 2;
            var boundZ = (Model.SupportPoint(Vector3.UnitZ).Z - Model.SupportPoint(-Vector3.UnitZ).Z) * 2;
            var minY = Model.SupportPoint(-Vector3.UnitY).Y;
            var maxY = Model.SupportPoint(Vector3.UnitY).Y;
            for (var x = 0f; x < boundX; x+=Step)
            {
                for (var z = 0f; z < boundZ; z+=Step)
                {
                    var offset = new BulletSharp.Math.Vector3(x - boundX / 2f, 0, z - boundZ / 2f);
                    var from = BulletSharp.Math.Vector3.UnitY * (-32 + minY) + offset;
                    var to = BulletSharp.Math.Vector3.UnitY * (32 + maxY) + offset;
                    BulletPhysics.ResetCallback(callback);
                    callback.RayFromWorld = from;
                    callback.RayToWorld = to;
                    world.DynamicsWorld.RayTestRef(ref from, ref to, callback);
                    if (callback.HasHit)
                    {
                        var currentPosition = Vector3.Transform(callback.HitPointWorld.Compatible(), Transformation);
                        var vertices = Graph.Vertices;
                        for (var i = 0; i < vertices.Length; ++i)
                        {
                            if(Math.Abs(vertices[i].Position.X - currentPosition.X) <= Step && Math.Abs(vertices[i].Position.Z - currentPosition.Z) <= Step)
                                list.Add(vertices[i]);
                        }
                        var waypoint = new Waypoint
                        {
                            Position = currentPosition
                        };
                        Graph.AddVertex(waypoint);
                        for (var i = 0; i < list.Count; ++i)
                        {
                            Graph.AddEdge(waypoint, list[i]);
                        }
                        list.Clear();
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