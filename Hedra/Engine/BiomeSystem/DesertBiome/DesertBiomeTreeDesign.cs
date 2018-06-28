using Hedra.Engine.Generation;
using Hedra.Engine.TreeSystem;

namespace Hedra.Engine.BiomeSystem.DesertBiome
{
    internal class DesertBiomeTreeDesign : BiomeTreeDesign
    {
        public DesertBiomeTreeDesign()
        {
            AvailableTypes = new TreeDesign[]
            {
                new OakDesign(),
                new AppleDesign(),
                new CypressDesign(),
                new TallDesign(),
                new PineDesign(),
            };
            if (World.Seed == World.MenuSeed) {
                AvailableTypes = new TreeDesign[]
                {
                    new AppleDesign(),
                };
            }
        }
        public override TreeDesign[] AvailableTypes { get; }
    }
}
