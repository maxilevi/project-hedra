using Hedra.Engine.Generation;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class MarketWellBuilder : Builder<MarketParameters>
    {
        public override bool Place(MarketParameters Parameters, VillageCache Cache)
        {
            return this.PlaceGroundwork(Parameters.Position, Parameters.WellSize);
        }

        public override void Polish(MarketParameters Parameters)
        {
            World.AddStructure(new LampPost(Vector3.UnitY * 8f + Parameters.Position)
            {
                Radius = 386,
                LightColor = new Vector3(.25f, .25f, .25f)
            });
        }
    }
}