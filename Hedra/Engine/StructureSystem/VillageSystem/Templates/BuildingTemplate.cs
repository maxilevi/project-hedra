namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class BuildingTemplate<T> where T : DesignTemplate
    {
        public T[] Designs { get; set; }
    }
}