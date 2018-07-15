using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageDesigner
    {
        private static Dictionary<string, VillageDesign> _villageDesigns;

        public VillageDesigner(Dictionary<string, VillageDesign> DesignDictionary)
        {
            _villageDesigns = DesignDictionary;
        }

        public VillageDesign[] Templates => _villageDesigns.Values.ToArray();
        public VillageDesign this[string Key] => _villageDesigns[Key.ToLowerInvariant()];
    }
}