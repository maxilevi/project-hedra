using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class BuildingTemplate<T> where T : DesignTemplate
    {
        public T[] Designs { get; set; }
    }
}