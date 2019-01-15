using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class TempleDesign : StructureDesign
    {
        public override int Radius { get; set; } = 700;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.BossIcon);

        public override void Build(CollidableStructure Structure)
        {

        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            throw new NotImplementedException();
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            return false;
        }
    }
}
