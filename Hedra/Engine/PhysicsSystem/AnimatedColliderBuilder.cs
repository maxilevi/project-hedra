using System;
using System.Collections.Generic;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    public static class AnimatedColliderBuilder
    {
        private static readonly Dictionary<string, AnimatedColliderData> ColliderCache = new Dictionary<string, AnimatedColliderData>();

        public static AnimatedColliderData Build(string Identifier, AnimatedModel Model)
        {
            if (!ColliderCache.ContainsKey(Identifier))
            {
                var bones = BoneData.FromArrays(Model.RootJoint.Index, Model.JointIdsArray, Model.VerticesArray);
                var colliderData = new AnimatedColliderData
                {
                    BonesData = bones,
                    DefaultBoneBoxes = BuildBoneBoxes(bones),
                    DefaultBroadphase = BuildBroadphase(bones)
                };
                ColliderCache.Add(Identifier, colliderData);
            }
            return ColliderCache[Identifier];
        }

        private static BoneBox[] BuildBoneBoxes(BoneData[] Bones)
        {
            var boxes = new List<BoneBox>();
            for (var i = 0; i < Bones.Length; i++)
            {
                boxes.Add(BoneBox.From(Bones[i]));
            }
            return boxes.ToArray();
        }

        private static CollisionPoint[] BuildBroadphase(BoneData[] Bones)
        {
            var vertexList = new List<Vector3>();
            for (var i = 0; i < Bones.Length; i++)
            {
                vertexList.AddRange(Bones[i].Vertices);
            }
            var vertices = vertexList.ToArray();
            var collisionPoints = new CollisionPoint[6];
            collisionPoints[0] = AddSupportWithId(-Vector3.UnitX, vertices, Bones);
            collisionPoints[1] = AddSupportWithId(-Vector3.UnitY, vertices, Bones);
            collisionPoints[2] = AddSupportWithId(-Vector3.UnitZ, vertices, Bones);
            collisionPoints[3] = AddSupportWithId(Vector3.UnitX, vertices, Bones);
            collisionPoints[4] = AddSupportWithId(Vector3.UnitY, vertices, Bones);
            collisionPoints[5] = AddSupportWithId(Vector3.UnitZ, vertices, Bones);
            return collisionPoints;
        }

        private static CollisionPoint AddSupportWithId(Vector3 Direction, Vector3[] Vertices, BoneData[] Bones)
        {
            var vertex = Vertices.SupportPoint(Direction);
            var index = -1;
            for (var i = 0; i < Bones.Length; i++)
            {
                if (Array.IndexOf(Bones[i].Vertices, vertex) != -1)
                {
                    index = Bones[i].Id;
                    break;
                }
            }
            return new CollisionPoint
            {
                Id = index,
                Vertex = vertex
            };
        }
    }
}
