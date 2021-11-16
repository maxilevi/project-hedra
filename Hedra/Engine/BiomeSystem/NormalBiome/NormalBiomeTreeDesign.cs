using Hedra.BiomeSystem;
using Hedra.Engine.TreeSystem;

namespace Hedra.Engine.BiomeSystem.NormalBiome
{
    public class NormalBiomeTreeDesign : BiomeTreeDesign
    {
        public NormalBiomeTreeDesign()
        {
            AvailableTypes = new TreeDesign[]
            {
                new OakDesign(),
                new AppleDesign(),
                new CypressDesign(),
                new TallDesign(),
                new BirchDesign(),
                new PineDesign()
            };
        }

        public override TreeDesign[] AvailableTypes { get; }
    }
}