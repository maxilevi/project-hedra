using Hedra.Engine.Localization;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Anvil : CraftingStation
    {
        public Anvil(Vector3 Position) : base(Position)
        {
        }

        public override Crafting.CraftingStation StationType => Crafting.CraftingStation.Anvil;
        
        protected override string CraftingMessage => Translations.Get("use_anvil");
    }
}