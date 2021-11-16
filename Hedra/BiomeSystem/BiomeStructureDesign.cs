using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;

namespace Hedra.BiomeSystem
{
    public abstract class BiomeStructureDesign
    {
        private List<StructureDesign> _designs { get; set; } = new List<StructureDesign>();

        public virtual StructureDesign[] Designs => _designs.ToArray();
        public abstract VillageType VillageType { get; }

        protected void AddDesign(StructureDesign Design)
        {
            _designs.Add(Design);
            _designs = _designs.OrderByDescending(D => D.PlateauRadius).ToList();
        }
    }
}