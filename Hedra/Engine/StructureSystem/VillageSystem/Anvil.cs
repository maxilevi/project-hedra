using Hedra.Engine.Localization;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Anvil : CraftingStation
    {
        public Anvil(Vector3 Position) : base(Position)
        {
        }

        protected override string CraftingMessage => Translations.Get("use_anvil");
    }
}