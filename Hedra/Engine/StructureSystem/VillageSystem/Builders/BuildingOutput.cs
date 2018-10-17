using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class BuildingOutput
    {
        public IList<VertexData> Models { get; set; }
        public List<CollisionShape> Shapes { get; set; }

        public void Color(Vector4 Find, Vector4 Replacement)
        {
            for (var i = 0; i < Models.Count; i++)
            {
                Models[i].Color(Find, Replacement);
            }
        }
        
        public static BuildingOutput Empty => new BuildingOutput
        {
            Models = new List<VertexData>(),
            Shapes = new List<CollisionShape>()
        };
    }
}