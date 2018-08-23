using System;
using System.Runtime.InteropServices;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class VillageDesign : StructureDesign
    {
        public override int Radius { get; set; } = 1024;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.VillageIcon);

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            var builder = Structure.Parameters.Get<VillageBuilder>("Builder");
            var design = Structure.Parameters.Get<PlacementDesign>("Design");
            builder.Build(design, Structure);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng)
        {
            var plateau = new Plateau(TargetPosition, 200);
            var structure = new CollidableStructure(this, TargetPosition, plateau);
            var region = World.BiomePool.GetRegion(TargetPosition);
            var builder = new VillageBuilder(VillageLoader.Designer[region.Structures.VillageType], Rng);
            var design = builder.DesignVillage();
            design.Translate(TargetPosition);
            builder.PlaceGroundwork(design);

            World.WorldBuilding.AddPlateau(plateau);
            structure.Parameters.Set("Builder", builder);
            structure.Parameters.Set("Design", design);
            return structure;        
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);
            return BiomeGenerator.PathFormula(ChunkOffset.X, ChunkOffset.Y) > 0 && Rng.Next(0, 25) == 1 && height > 0;
        }
    }
}
