using System.Numerics;
using Hedra.Engine.Generation;

namespace Hedra.Engine.WorldBuilding
{
    public abstract class BaseGroundwork : IGroundwork
    {
        public BlockType Type { get; set; }
        public bool NoTrees { get; set; }
        public bool NoPlants { get; set; }
        public virtual float BonusHeight { get; set; }
        public float DensityMultiplier { get; set; }
        public virtual bool IsPath => false;

        public abstract bool Affects(Vector2 Sample);

        public abstract float Density(Vector2 Sample);

        public abstract BoundingBox ToBoundingBox();
    }
}