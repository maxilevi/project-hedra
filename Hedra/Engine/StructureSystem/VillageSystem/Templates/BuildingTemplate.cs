namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    internal class BuildingTemplate<T> where T : DesignTemplate
    {
        public T[] Designs { get; set; }
    }
}