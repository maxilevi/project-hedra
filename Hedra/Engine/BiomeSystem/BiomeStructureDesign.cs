using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.StructureSystem;

namespace Hedra.Engine.BiomeSystem
{
    public abstract class BiomeStructureDesign
    {
        private List<StructureDesign> _designs { get; } = new List<StructureDesign>();

        protected void AddDesign(StructureDesign Design)
        {
            this._designs.Add(Design);
        }

        public StructureDesign[] Designs => _designs.ToArray();
    }
}
