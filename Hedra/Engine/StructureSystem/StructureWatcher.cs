using System;
using Hedra.Engine.Generation;

namespace Hedra.Engine.StructureSystem
{
    public class StructureWatcher : IDisposable
    {
        public CollidableStructure Structure { get; }

        public StructureWatcher(CollidableStructure Structure)
        {
            this.Structure = Structure;
        }

        public void Update()
        {
            
        }

        public void Dispose()
        {
            Structure.Dispose();
        }
    }
}