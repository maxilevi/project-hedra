﻿using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageDesigner
    {
        private static Dictionary<string, VillageRoot> _villageDesigns;

        public VillageDesigner(Dictionary<string, VillageRoot> DesignDictionary)
        {
            _villageDesigns = DesignDictionary;
        }

        public VillageRoot[] Templates => _villageDesigns.Values.ToArray();
        public VillageRoot this[string Key] => _villageDesigns[Key.ToLowerInvariant()];
    }
}