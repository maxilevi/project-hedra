using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.TreeSystem;

namespace Hedra.Engine.BiomeSystem.SnowBiome
{
    public class SnowBiomeTreeDesign : BiomeTreeDesign
    {
        public SnowBiomeTreeDesign()
        {
            AvailableTypes = new TreeDesign[]
            {
                new OakDesign(),
                new AppleDesign(),
                new CypressDesign(),
                new TallDesign(),
                new PineDesign(),
            };
            if (World.Seed == World.MenuSeed)
            {
                AvailableTypes = new TreeDesign[]
                {
                    new AppleDesign(),
                };
            }
        }
        public override TreeDesign[] AvailableTypes { get; }
    }
}
