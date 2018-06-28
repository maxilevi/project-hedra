using Hedra.Engine.TreeSystem;

namespace Hedra.Engine.BiomeSystem.UndeadBiome
{
    internal class UndeadBiomeTreeDesign : BiomeTreeDesign
    {
        public UndeadBiomeTreeDesign()
        {
            AvailableTypes = new TreeDesign[]
            {
                new DeadTreeDesign(),
            };
        }
        public override TreeDesign[] AvailableTypes { get; }
    }
}
