using Hedra.Engine.Localization;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class Loom : CraftingStation
    {
        public Loom(Vector3 Position) : base(Position)
        {
        }

        public override CraftingSystem.CraftingStation StationType => CraftingSystem.CraftingStation.Loom;
        
        protected override string CraftingMessage => Translations.Get("use_loom");
    }
}