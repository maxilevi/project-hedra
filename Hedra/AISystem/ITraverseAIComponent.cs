using System.Numerics;

namespace Hedra.AISystem
{
    public interface ITraverseAIComponent
    {
        Vector2 GridSize { get; set; }
        Vector3 TargetPoint { get; set; }
    }
}