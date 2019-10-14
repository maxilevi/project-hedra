using Hedra.Engine.Localization;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Workbench : CraftingStation
    {
        public Workbench(Vector3 Position) : base(Position)
        {
        }

        public override Crafting.CraftingStation StationType => Crafting.CraftingStation.Workbench;
        
        protected override string CraftingMessage => Translations.Get("use_workbench");
    }
}