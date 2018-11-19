using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class CompressedBuildingOutput
    {
        public IList<CompressedVertexData> Models { get; set; }
        public List<CollisionShape> Shapes { get; set; }
        public List<BaseStructure> Structures { get; set; }

        public bool IsEmpty => Models.Count == 0 && Shapes.Count == 0 && Structures.Count == 0; 
    }
}