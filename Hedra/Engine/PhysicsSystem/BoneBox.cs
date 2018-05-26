using System;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    public class BoneBox
    {
        public int JointId { get; private set; }
        public Vector3[] Corners { get; private set; }

        public BoneBox(int JointId, Vector3[] Corners)
        {
            if(Corners.Length != 8) throw new ArgumentOutOfRangeException($"Bone box should have 8 vertices.");
            this.Corners = Corners;
        }

        public void Transform(Matrix4 Transformation)
        {
            for (var i = 0; i < Corners.Length; i++)
            {
                Corners[i] = Vector3.TransformPosition(Corners[i], Transformation);
            }
        }

        public BoneBox Clone()
        {
            return new BoneBox(JointId, new[]
            {
                this.Corners[0],
                this.Corners[1],
                this.Corners[2],
                this.Corners[3],
                this.Corners[4],
                this.Corners[5],
                this.Corners[6],
                this.Corners[7]
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
                new Vector3(maxX, maxY, maxZ),
            });
        }
    }
}
