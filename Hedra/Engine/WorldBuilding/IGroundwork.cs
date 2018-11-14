using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public interface IGroundwork
    {
        BlockType Type { get; set; }
        float BonusHeight { get; set; }
        float DensityMultiplier { get; set; }
        bool IsPath { get; }
        bool Affects(Vector2 Sample);
        float Density(Vector2 Sample);
    }
}
