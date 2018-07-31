using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class BuildingOutput
    {
        public VertexData Model { get; set; }
        public List<CollisionShape> Shapes { get; set; }


        public static BuildingOutput Empty => new BuildingOutput
        {
            Model = new VertexData(),
            Shapes = new List<CollisionShape>()
        };
    }
}