using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class BuildingOutput
    {
        public VertexData Model { get; set; }
        public List<CollisionShape> Shapes { get; set; }
    }
}