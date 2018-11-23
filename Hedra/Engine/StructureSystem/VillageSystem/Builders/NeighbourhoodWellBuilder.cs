using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class NeighbourhoodWellBuilder : Builder<NeighbourhoodParameters>
    {
        public NeighbourhoodWellBuilder(CollidableStructure Structure) : base(Structure)
        {
        }

        public override bool Place(NeighbourhoodParameters Parameters, VillageCache Cache)
        {
            return true;
        }

        public override BuildingOutput Build(NeighbourhoodParameters Parameters, VillageCache Cache, Random Rng, Vector3 Center)
        {
            if (Parameters.IsSingle)
            {
                return new BuildingOutput
                {
                    Models = new List<VertexData>(),
                    Shapes = new List<CollisionShape>()
                };
            }
            else
            {
                var output = base.Build(Parameters, Cache, Rng, Center);
                output.Structures.Add(new LampPost(Vector3.UnitY * 8f + Parameters.Position)
                {
                    Radius = 386,
                    LightColor = new Vector3(.25f, .25f, .25f)
                });
                return output;
            }
        }
    }
}