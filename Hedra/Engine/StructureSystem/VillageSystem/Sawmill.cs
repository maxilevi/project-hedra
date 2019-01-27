namespace Hedra.Engine.StructureSystem
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