using Hedra.Engine.Localization;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Workbench : CraftingStation
    {
        public Workbench(Vector3 Position) : base(Position)
        {
        }

        public override CraftingSystem.CraftingStation StationType => CraftingSystem.CraftingStation.Workbench;
        
        protected override string CraftingMessage => Translations.Get("use_workbench");
    }
}