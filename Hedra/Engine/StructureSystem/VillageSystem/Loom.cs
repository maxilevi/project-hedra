using System.Numerics;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Loom : CraftingStation
    {
        public Loom(Vector3 Position) : base(Position)
        {
        }

        public override Crafting.CraftingStation StationType => Crafting.CraftingStation.Loom;

        protected override string CraftingMessage => Translations.Get("use_loom");
    }
}