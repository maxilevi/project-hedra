using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    internal class WellBuilder : Builder<BuildingParameters>
    {
        public override void Place(BuildingParameters Parameters, VillageCache Cache)
        {
            //this.PlaceGroundwork(Parameters.Position, 96);
        }

        public override void Polish(BuildingParameters Parameters)
        {
            World.AddStructure(new LampPost(Vector3.UnitY * 8f + Parameters.Position)
            {
                Radius = 386,
                LightColor = new Vector3(.25f, .25f, .25f)
            });
        }
    }
}