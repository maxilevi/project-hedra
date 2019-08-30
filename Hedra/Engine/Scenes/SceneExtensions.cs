using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Scenes
{
    public static class SceneExtensions
    {
        public static bool IsColorCode(this VertexData Data, Vector3 Color)
        {
            if(!Data.HasColors) throw new ArgumentOutOfRangeException("Scene mesh doesn't have colors");
            for (var i = 0; i < Data.Colors.Count; ++i)
            {
                if (Data.Colors[i].Xyz != Color)
                    return false;
            }

            return true;
        }

        public static Vector3 AverageVertices(this VertexData Data)
        {
            return Data.Vertices.AverageVertices();
        }

        public static Vector3 AverageVertices(this IList<Vector3> Vertices)
        {
            if(Vertices.Count == 0)
                return Vector3.Zero;
            return Vertices.Aggregate((V1, V2) => V1 + V2) / Vertices.Count;
        }
    }
}