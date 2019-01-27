using Hedra.Engine.Localization;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Sawmill : CraftingStation
    {
        public Sawmill(Vector3 Position) : base(Position)
        {
        }

        public override CraftingSystem.CraftingStation StationType => CraftingSystem.CraftingStation.Sawmill;
        
        protected override string CraftingMessage => Translations.Get("use_sawmill");
    }
}