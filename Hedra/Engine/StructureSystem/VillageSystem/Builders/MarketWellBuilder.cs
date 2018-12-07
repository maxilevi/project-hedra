using System;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class MarketWellBuilder : Builder<MarketParameters>
    {
        public MarketWellBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override bool Place(MarketParameters Parameters, VillageCache Cache)
        {
            return this.PlaceGroundwork(Parameters.Position, Parameters.WellSize);
        }
        
        public override BuildingOutput Build(MarketParameters Parameters, DesignTemplate Design, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var output = base.Build(Parameters, Design, Cache, Rng, Center);
            output.Structures.Add(new LampPost(Vector3.UnitY * 8f + Parameters.Position)
            {
                Radius = 386,
                LightColor = new Vector3(.25f, .25f, .25f)
            });
            return output;
        }
    }
}