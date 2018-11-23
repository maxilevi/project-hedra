using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class BuildingOutput
    {
        public IList<VertexData> Models { get; set; }
        public List<CollisionShape> Shapes { get; set; }
        public List<BaseStructure> Structures { get; set; }

        public BuildingOutput()
        {
            Structures = new List<BaseStructure>();
        }
        
        public void Color(Vector4 Find, Vector4 Replacement)
        {
            for (var i = 0; i < Models.Count; i++)
            {
                Models[i].Color(Find, Replacement);
            }
        }

        public CompressedBuildingOutput AsCompressed()
        {
            return new CompressedBuildingOutput
            {
                Models = Models.Select(CompressedVertexData.FromVertexData).ToList(),
                Shapes = Shapes,
                Structures = Structures
            };
        }

        public bool IsEmpty => Models.Count == 0 && Shapes.Count == 0 && Structures.Count == 0;

        public static BuildingOutput Empty => new BuildingOutput
        {
            Models = new List<VertexData>(),
            Shapes = new List<CollisionShape>(),
            Structures = new List<BaseStructure>()
        };
    }
}