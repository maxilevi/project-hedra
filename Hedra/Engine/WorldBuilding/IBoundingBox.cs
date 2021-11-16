using System.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    public interface IBoundingBox
    {
        Vector2 LeftCorner { get; }
        Vector2 RightCorner { get; }
        Vector2 BackCorner { get; }
        Vector2 FrontCorner { get; }
        bool Collides(Vector2 Point);
    }
}