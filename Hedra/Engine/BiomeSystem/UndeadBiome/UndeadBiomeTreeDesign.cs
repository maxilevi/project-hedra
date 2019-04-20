using Hedra.BiomeSystem;
using Hedra.Engine.TreeSystem;

namespace Hedra.Engine.BiomeSystem.UndeadBiome
{
    public class UndeadBiomeTreeDesign : BiomeTreeDesign
    {
        public override TreeDesign[] AvailableTypes { get; } = new TreeDesign[]
        {
            new DeadTreeDesign(),
        };
    }
}
