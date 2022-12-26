using Hedra.BiomeSystem;
using Hedra.Engine.TreeSystem;

namespace Hedra.Engine.BiomeSystem.ShroomDimension
{
    public class ShroomDimensionTreeDesign : BiomeTreeDesign
    {
        public override TreeDesign[] AvailableTypes { get; } =
        {
            new MushroomTreeDesign()
        };
    }
}