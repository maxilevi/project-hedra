using System.Collections.Generic;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;

namespace Hedra.BiomeSystem
{
    public abstract class BiomeStructureDesign
    {
        private List<StructureDesign> _designs { get; } = new List<StructureDesign>();

        protected BiomeStructureDesign()
        {
            AddDesign(new SpawnCampfireDesign());
        }
        
        protected void AddDesign(StructureDesign Design)
        {
            this._designs.Add(Design);
        }

        public StructureDesign[] Designs => _designs.ToArray();
        public abstract VillageType VillageType { get; }
    }
}
