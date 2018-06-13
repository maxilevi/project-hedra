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
            var collisionPoints = new CollisionPoint[8];
            var minX = GetSupportWithId(-Vector3.UnitX, vertices, Bones);
            var minY = GetSupportWithId(-Vector3.UnitY, vertices, Bones);
            var minZ = GetSupportWithId(-Vector3.UnitZ, vertices, Bones);
            var maxX = GetSupportWithId(Vector3.UnitX, vertices, Bones);
            var maxY = GetSupportWithId(Vector3.UnitY, vertices, Bones);
            var maxZ = GetSupportWithId(Vector3.UnitZ, vertices, Bones);
            collisionPoints[0] = BuildFromVectors(minX, minY, minZ, GetId(new Vector3(0, 0, 0), vertices, Bones));
            collisionPoints[1] = BuildFromVectors(maxX, minY, minZ, GetId(new Vector3(1, 0, 0), vertices, Bones));
            collisionPoints[2] = BuildFromVectors(minX, minY, maxZ, GetId(new Vector3(0, 0, 1), vertices, Bones));
            collisionPoints[3] = BuildFromVectors(maxX, minY, maxZ, GetId(new Vector3(1, 0, 1), vertices, Bones));
            
            collisionPoints[4] = BuildFromVectors(minX, maxY, minZ, GetId(new Vector3(0, 1, 0), vertices, Bones));
            collisionPoints[5] = BuildFromVectors(maxX, maxY, minZ, GetId(new Vector3(1, 1, 0), vertices, Bones));
            collisionPoints[6] = BuildFromVectors(minX, maxY, maxZ, GetId(new Vector3(0, 1, 1), vertices, Bones));
            collisionPoints[7] = BuildFromVectors(maxX, maxY, maxZ, GetId(new Vector3(1, 1, 1), vertices, Bones));
            return collisionPoints;
        }

        private static CollisionPoint BuildFromVectors(Vector4 DirX, Vector4 DirY, Vector4 DirZ, int Id) 
        {
            return new CollisionPoint
            {
                Id = new Vector3(Id, Id, Id),
                Vertex = new Vector3(DirX.X, DirY.Y, DirZ.Z)
            };
        }
        private static Vector4 GetSupportWithId(Vector3 Direction, Vector3[] Vertices, BoneData[] Bones)
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
            return new Vector4(vertex, index);
        }

        private static int GetId(Vector3 Direction, Vector3[] Vertices, BoneData[] Bones)
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
            return index;
        }
    }
}
