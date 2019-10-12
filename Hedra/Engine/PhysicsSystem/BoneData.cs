using System.Collections.Generic;
using System.Linq;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.PhysicsSystem
{
    public class BoneData
    {
        public int Id { get; set; }
        public Vector3[] Vertices { get; set; }
        public bool Empty => Vertices.Length == 0;

        public Vector3 SupportPoint(Vector3 Direction)
        {
            float highest = float.MinValue;
            Vector3 support = Vector3.Zero;
            for (var i = 0; i < Vertices.Length; i++)
            {
                float dot = Vector3.Dot(Direction, Vertices[i]);
                if (dot > highest)
                {
                    highest = dot;
                    support = Vertices[i];
                }
            }
            return support;
        }

        public static BoneData[] FromArrays(int RootId, Vector3[] JointIds, Vector3[] Vertices)
        {
            var ids = new Dictionary<int, HashSet<Vector3>>();
            for (var i = 0; i < JointIds.Length; i++)
            {
                for (var k = 0; k < 1; k++)
                {
                    var id = (int)JointIds[i][k];
                    if (!ids.ContainsKey(id))
                    {
                        ids.Add(id, new HashSet<Vector3>());
                    }
                    if (!ids[id].Contains(Vertices[i])) ids[id].Add(Vertices[i]);
                }
            }
            var boneList = new List<BoneData>();
            foreach (var pair in ids)
            {
                boneList.Add(new BoneData
                {
                    Id = pair.Key,
                    Vertices = pair.Value.ToArray()
                });
            }
            return boneList.Where(B => !B.Empty).ToArray();
        }
    }
}
