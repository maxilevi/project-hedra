using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    internal interface IGroundwork
    {
        BlockType Type { get; set; }
        Half Density { get; set; }

        bool Affects(Vector2 Sample);
    }
}
