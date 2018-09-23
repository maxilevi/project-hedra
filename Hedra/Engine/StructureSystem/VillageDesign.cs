using System;
using System.Runtime.InteropServices;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
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

        public override void Build(CollidableStructure Structure)
        {
            var builder = Structure.Parameters.Get<VillageBuilder>("Builder");
            var design = Structure.Parameters.Get<PlacementDesign>("Design");
            builder.Build(design, Structure);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng);
            var region = World.BiomePool.GetRegion(TargetPosition);
            var builder = new VillageBuilder(VillageLoader.Designer[region.Structures.VillageType], Rng);
            var design = builder.DesignVillage();
            structure.Mountain.Radius = 200;
            design.Translate(TargetPosition);
            builder.PlaceGroundwork(design);

            World.WorldBuilding.AddPlateau(structure.Mountain);
            structure.Parameters.Set("Builder", builder);
            structure.Parameters.Set("Design", design);
            return structure;
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);
            return BiomeGenerator.PathFormula(ChunkOffset.X, ChunkOffset.Y) > 0 && Rng.Next(0, 25) == 1 && height > BiomePool.SeaLevel;
        }

        private string CreateName(int Seed)
        {
            return NameGenerator.Generate(Seed);
        }

        public override void OnEnter(IPlayer Player)
        {
            Player.MessageDispatcher.ShowTitleMessage($"WELCOME TO {NameGenerator.Generate(World.Seed)}", 6f);
        }
        
        public override int[] AmbientSongs => new []
        {
            SoundtrackManager.VillageIndex
        };
    }
}
