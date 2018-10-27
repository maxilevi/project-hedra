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
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    
    public class ChunkComparer : IComparer<IPositionable>
    {
        public Vector3 Position { get; set; }

        public int Compare(IPositionable ChunkA, IPositionable ChunkB)
        {
            if (ChunkA == ChunkB) return 0;
            if (ChunkA == null) return -1;
            if (ChunkB == null) return 1;

            float distanceA = (ChunkA.Position - Position).LengthSquared;
            float distanceB = (ChunkB.Position - Position).LengthSquared;

            if (distanceA < distanceB) return -1;
            return Math.Abs(distanceA - distanceB) < 0.0005 ? 0 : 1;
        }
    }
}
