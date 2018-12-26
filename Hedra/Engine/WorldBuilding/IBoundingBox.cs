using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public interface IBoundingBox
    {
        bool Collides(Vector2 Point);
        Vector2 LeftCorner { get; }
        Vector2 RightCorner { get; }
        Vector2 BackCorner { get; }
        Vector2 FrontCorner { get; }
    }
}