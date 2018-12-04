using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using System;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class CompressedBuildingOutput : IDisposable
    {
        public List<CompressedVertexData> Models { get; set; }
        public List<InstanceData> Instances { get; set; }
        public List<CollisionShape> Shapes { get; set; }
        public List<BaseStructure> Structures { get; set; }

        public void Dispose()
        {
            for (var i = 0; i < Structures.Count; i++)
                Structures[i].Dispose();
        }

        public bool IsEmpty => Models.Count == 0 && Instances.Count == 0 && Shapes.Count == 0 && Structures.Count == 0; 
    }
}