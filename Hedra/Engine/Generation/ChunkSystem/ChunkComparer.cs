/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/09/2016
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Hedra.Engine.Core;
using System.Numerics;

namespace Hedra.Engine.Generation.ChunkSystem
{
    
    public class ChunkComparer : ClosestComparer, IComparer<IPositionable>
    {
        public int Compare(IPositionable ChunkA, IPositionable ChunkB)
        {
            if (ChunkA == ChunkB) return 0;
            if (ChunkA == null) return -1;
            if (ChunkB == null) return 1;
            return base.Compare(ChunkA.Position, ChunkB.Position);
        }
    }
    
    public class ClosestComparer : IComparer<Vector3>
    {
        public Vector3 Position { get; set; }

        public int Compare(Vector3 A, Vector3 B)
        {
            if (A == B) return 0;

            var distanceA = (A - Position).LengthSquared();
            var distanceB = (B - Position).LengthSquared();

            if (distanceA < distanceB) return -1;
            return Math.Abs(distanceA - distanceB) < 0.0005 ? 0 : 1;
        }
    }
}
