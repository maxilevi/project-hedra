using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hedra.Engine.PhysicsSystem
{
    public class BoneBox
    {
        public static readonly uint[] Indices =
        {
            0, 1, 3, 3, 1, 2,
            1, 5, 2, 2, 5, 6,
            5, 4, 6, 6, 4, 7,
            4, 0, 7, 7, 0, 3,
            3, 2, 7, 7, 2, 6,
            4, 5, 0, 0, 5, 1
        };

        public BoneBox(int JointId, Vector3[] Corners)
        {
            if (Corners.Length != 8) throw new ArgumentOutOfRangeException("Bone box should have 8 vertices.");
            this.Corners = Corners;
            this.JointId = JointId;
            Size = Corners[7] - Corners[0];
        }

        public int JointId { get; }
        public Vector3[] Corners { get; }
        public Vector3 Size { get; }

        public void Transform(Matrix4x4 Transformation)
        {
            for (var i = 0; i < Corners.Length; i++) Corners[i] = Vector3.Transform(Corners[i], Transformation);
        }

        public CollisionShape ToShape()
        {
            return new CollisionShape(new List<Vector3>(Corners),
                new List<uint>(Indices)
            );
        }

        public BoneBox Clone()
        {
            return new BoneBox(JointId, new[]
            {
                Corners[0],
                Corners[1],
                Corners[2],
                Corners[3],
                Corners[4],
                Corners[5],
                Corners[6],
                Corners[7]
            });
        }

        public static BoneBox From(BoneData Data)
        {
            var minX = Data.SupportPoint(-Vector3.UnitX).X;
            var minY = Data.SupportPoint(-Vector3.UnitY).Y;
            var minZ = Data.SupportPoint(-Vector3.UnitZ).Z;
            var maxX = Data.SupportPoint(Vector3.UnitX).X;
            var maxY = Data.SupportPoint(Vector3.UnitY).Y;
            var maxZ = Data.SupportPoint(Vector3.UnitZ).Z;
            return new BoneBox(Data.Id, new[]
            {
                new Vector3(minX, minY, minZ),
                new Vector3(maxX, minY, minZ),
                new Vector3(minX, minY, maxZ),
                new Vector3(maxX, minY, maxZ),

                new Vector3(minX, maxY, minZ),
                new Vector3(maxX, maxY, minZ),
                new Vector3(minX, maxY, maxZ),
                new Vector3(maxX, maxY, maxZ)
            });
        }
    }
}