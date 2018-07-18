namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageConfiguration
    {
        public RingConfiguration InnerRing { get; set; } = new RingConfiguration
        {
            BlacksmithChances = (float) (1.0 / 4.0),
            FarmChances = (float) (1.0 / 16.0),
            StableChances = (float) (1.0 / 24.0),
            HouseChances = (float) (1.0 / 16.0),
            MarketChances = (float) (1.0 / 1.0)
        };
        
        public RingConfiguration MiddleRing { get; set; } = new RingConfiguration
        {
            BlacksmithChances = (float) (2.0 / 12.0),
            FarmChances = (float) (2.0 / 8.0),
            StableChances = (float) (1.0 / 8.0),
            HouseChances = (float) (6.0 / 8.0),
            MarketChances = (float) (1.0 / 32.0)
        };
        
        public RingConfiguration OuterRing { get; set; } = new RingConfiguration
        {
            BlacksmithChances = (float) (2.0 / 24.0),
            FarmChances = (float) (4.0 / 6.0),
            StableChances = (float) (2.0 / 16.0),
            HouseChances = (float) (2.0 / 6.0),
            MarketChances = (float) (0.0 / 48.0)
        };
    }

    public class RingConfiguration
    {
        public float HouseChances { get; set; }
        public float BlacksmithChances { get; set; }
        public float StableChances { get; set; }
        public float FarmChances { get; set; }
        public float MarketChances { get; set; }
    }
}