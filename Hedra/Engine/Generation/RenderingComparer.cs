using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Generation
{
    public class RenderingComparer : IComparer<KeyValuePair<Vector2, ChunkRenderCommand>>
    {
        public Vector3 Position { get; set; }
        
        /// <summary>
        /// Compares 2 objects for sorting.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns>Return -1 if B is greater than A, 1 is viceversa and 0 if equal</returns>
        public int Compare(KeyValuePair<Vector2, ChunkRenderCommand> A, KeyValuePair<Vector2, ChunkRenderCommand> B)
        {
            if (A.Key == B.Key && A.Value == B.Value) return 0;

            var distanceA = (A.Key - Position.Xz).LengthSquared;
            var distanceB = (B.Key - Position.Xz).LengthSquared;

            if (distanceA < distanceB) return -1;
            return Math.Abs(distanceA - distanceB) < 0.005f ? 0 : 1;
        }
    }
}