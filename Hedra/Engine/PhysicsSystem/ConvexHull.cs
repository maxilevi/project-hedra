using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    internal static class ConvexHull
    {
        public static Vector3 SupportPoint(List<Vector3> Input, Vector3 Direction)
        {
            float highest = float.MinValue;
            Vector3 support = Vector3.Zero;
            for (var i = 0; i < Input.Count; i++)
            {
                float dot = Vector3.Dot(Direction, Input[i]);
                if (dot > highest)
                {
                    highest = dot;
                    support = Input[i];
                }
            }
            return support;
        }

        public static CollisionShape From(Vector3[] Input)
        {
            if(Input.Length < 4) throw new ArgumentException($"A convex hull cannot be created from {Input.Length}");
            var points = new List<Vector3>(Input);
            var a = ConvexHull.SupportPoint(points, -Vector3.One);
            var b = ConvexHull.SupportPoint(points, Vector3.One);
            var hull = new List<Vector3>();

            hull.Add(a);
            hull.Add(b);
            points.Remove(a);
            points.Remove(b);

            var s1 = points.Where(P => Vector3.Cross(a - b, b - P).Y > 0);
            var s2 = points.Where(P => Vector3.Cross(a - b, b - P).Y < 0);
   /*
                Segment AB divides the remaining(n-2) points into 2 groups S1 and S2
            where S1 are points in S that are on the right side of the oriented line from a to B, 
                and S2 are points in S that are on the right side of the oriented line from B to a
            ConvexHull.FindHull(S1, a, B)
            ConvexHull.FindHull(S2, B, a)
            */
            return new CollisionShape(hull);
        }

        private static void FindHull(List<Vector3> Points, Vector3 P, Vector3 Q)
        {
            if (Points.Count == 0) return;
            /*
            var C = ConvexHull.SupportPoint(Points);
            From the given set of points in Sk, find farthest point, say C, from segment PQ
                Add point C to convex hull at the location between P and Q
            Three points P, Q, and C partition the remaining points of Sk into 3 subsets: S0, S1, and S2
            where S0 are points inside triangle PCQ, S1 are points on the right side of the oriented
                line from  P to C, and S2 are points on the right side of the oriented line from C to Q.
                FindHull(S1, P, C)]
            ConvexHull.FindHull(S2, C, Q);*/
        }
    }
}
