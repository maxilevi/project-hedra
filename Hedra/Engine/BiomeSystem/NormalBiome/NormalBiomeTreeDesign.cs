using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
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
                new PineDesign(),
            };
        }
        public override TreeDesign[] AvailableTypes { get; }
    }
}
