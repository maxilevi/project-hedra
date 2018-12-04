using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class FarmBuilder : Builder<FarmParameters>
    {
        protected override bool LookAtCenter => false;
        protected override bool GraduateColor => false;
        
        public FarmBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public virtual BuildingOutput Build(FarmParameters Parameters, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var output = base.Build(Parameters, Cache, Rng, Center);
            for (var i = 0; i < output.Models.Count; i++)
            {
                output.Models[i].Extradata = output.Models[i].GenerateWindValues(2f).ToList();
            }
            return output;
        }
        
        public override bool Place(FarmParameters Parameters, VillageCache Cache)
        {
            var width = Parameters.GetSize(Cache) * .5f;
            var path = new RoundedGroundwork(Parameters.Position, width, BlockType.Dirt)
            {
                BonusHeight = .25f
            };
            return this.PushGroundwork(path);
        }
    }
}