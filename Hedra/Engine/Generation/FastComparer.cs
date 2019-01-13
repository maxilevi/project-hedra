using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.Generation
{
    public class FastComparer : IEqualityComparer<Vector2>
    {
        public bool Equals(Vector2 x, Vector2 y)
        {
            return x == y;
        }

        public int GetHashCode(Vector2 obj)
        {
            return obj.GetHashCode();
        }
    }
}